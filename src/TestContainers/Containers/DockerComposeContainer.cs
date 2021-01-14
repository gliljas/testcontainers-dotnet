using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using TestContainers;
using TestContainers.Containers.Mounts;
using TestContainers.Containers.Output;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;
using TestContainers.Images;
using TestContainers.Lifecycle;
using TestContainers.Utility;

namespace TestContainers.Containers
{
    public class DockerComposeContainer : IContainer
#if !NETSTANDARD2_0
        , IAsyncDisposable
#endif
    {
        private ILogger _logger;
        /**
     * Random identifier which will become part of spawned containers names, so we can shut them down
     */
        private readonly string _identifier;
        private readonly IReadOnlyList<FileInfo> _composeFiles;
        private ISet<ParsedDockerComposeFile> _parsedComposeFiles;
        private readonly Dictionary<string, int> _scalingPreferences = new Dictionary<string, int>();
        private bool _localCompose;
        private bool _pull = true;
        private bool _build = false;
        private ISet<string> _options = new HashSet<string>();
        private bool _tailChildContainers;

        private string _project;

        private int _nextAmbassadorPort = 2000;
        private readonly ConcurrentDictionary<string, Dictionary<int, int>> _ambassadorPortMappings = new ConcurrentDictionary<string, Dictionary<int, int>>();
        private readonly ConcurrentDictionary<string, ComposeServiceWaitStrategyTarget> _serviceInstanceMap = new ConcurrentDictionary<string, ComposeServiceWaitStrategyTarget>();
        private readonly ConcurrentDictionary<string, WaitAllStrategy> _waitStrategyMap = new ConcurrentDictionary<string, WaitAllStrategy>();
        private readonly SocatContainer _ambassadorContainer = new SocatContainer();
        private readonly ConcurrentDictionary<string, List<IProgress<string>>> _logConsumers = new ConcurrentDictionary<string, List<IProgress<string>>>();

        private static readonly SemaphoreSlim MUTEX = new SemaphoreSlim(1, 1);

        private List<string> _services = new List<string>();

        /**
     * Properties that should be passed through to all Compose and ambassador containers (not
     * necessarily to containers that are spawned by Compose itself)
     */
        private Dictionary<string, string> _env = new Dictionary<string, string>();

        public string ImageName => throw new NotImplementedException();

        public string TestHostIpAddress => throw new NotImplementedException();

        public IReadOnlyList<string> PortBindings => throw new NotImplementedException();

        public IReadOnlyList<string> ExtraHosts => throw new NotImplementedException();

        public IReadOnlyDictionary<string, string> EnvMap => throw new NotImplementedException();

        public string[] CommandParts => throw new NotImplementedException();

        public IReadOnlyList<IBind> Binds => throw new NotImplementedException();

        public string Host => throw new NotImplementedException();

        public ContainerInspectResponse ContainerInfo => throw new NotImplementedException();

        public string ContainerId => throw new NotImplementedException();

        public DockerComposeContainer(params FileInfo[] composeFiles) : this(composeFiles.ToList())
        {
        }

        public DockerComposeContainer(IReadOnlyList<FileInfo> composeFiles) : this(Base58.RandomString(6).ToLower(), composeFiles)
        {

        }

        public DockerComposeContainer(string identifier, params FileInfo[] composeFiles) : this(identifier, composeFiles.ToList())
        {

        }

        public DockerComposeContainer(string identifier, IReadOnlyList<FileInfo> composeFiles)
        {
            _composeFiles = composeFiles;
            _parsedComposeFiles = new HashSet<ParsedDockerComposeFile>(composeFiles.Select(x => new ParsedDockerComposeFile(x)));

            // Use a unique identifier so that containers created for this compose environment can be identified
            _identifier = identifier;
            _project = RandomProjectId();

        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            await MUTEX.WaitAsync(cancellationToken);
            try
            {
                RegisterContainersForShutdown();
                if (_pull)
                {
                    try
                    {
                        await PullImages();
                    }
                    catch (ContainerLaunchException e)
                    {
                        _logger.LogWarning(e, "Exception while pulling images, using local images if available");
                    }
                }
                await CreateServices();
                await StartAmbassadorContainers();
                await WaitUntilServiceStarted();
            }
            finally
            {
                MUTEX.Release();
            }
        }

        public DockerComposeContainer WithServices(params string[] services)
        {
            _services = services.ToList();
            return this;
        }

        private Task StartAmbassadorContainers()
        {
            throw new NotImplementedException();
        }

        private async Task CreateServices()
        {
            // services that have been explicitly requested to be started. If empty, all services should be started.
            var serviceNameArgs = string.Join(" ", _services
                .Concat(_scalingPreferences.Keys)
                .Distinct());

            // Apply scaling for the services specified using `withScaledService`
            var scalingOptions = string.Join(" ", _scalingPreferences
                .Select(entry => entry.Key + "=" + entry.Value)
                .Distinct());

            var command = OptionsAsString() + "up -d";

            if (_build)
            {
                command += " --build";
            }

            if (!string.IsNullOrEmpty(scalingOptions))
            {
                command += " " + scalingOptions;
            }

            if (!string.IsNullOrEmpty(serviceNameArgs))
            {
                command += " " + serviceNameArgs;
            }

            // Run the docker-compose container, which starts up the services
            await RunWithCompose(command);
        }

        private string OptionsAsString()
        {
            string optionsString = string.Join(" ", _options);
            if (optionsString.Length != 0)
            {
                // ensures that there is a space between the options and 'up' if options are passed.
                return optionsString + " ";
            }
            else
            {
                // otherwise two spaces would appear between 'docker-compose' and 'up'
                return string.Empty;
            }
        }

        private async Task WaitUntilServiceStarted()
        {
            foreach (var container in await ListChildContainers())
            {
                await CreateServiceInstance(container);
            }

            var servicesToWaitFor = new HashSet<string>(_waitStrategyMap.Keys);
            var instantiatedServices = new HashSet<string>(_serviceInstanceMap.Keys);
            var missingServiceInstances = servicesToWaitFor.Except(instantiatedServices);

            if (missingServiceInstances.Any())
            {
                throw new IllegalStateException(
                    "Services named " + missingServiceInstances + " " +
                        "do not exist, but wait conditions have been defined " +
                        "for them. This might mean that you misspelled " +
                        "the service name when defining the wait condition.");
            }

            foreach (var item in _serviceInstanceMap)
            {
                await WaitUntilServiceStarted(item.Key, item.Value);
            }
        }

        private async Task CreateServiceInstance(ContainerListResponse container)
        {
            var serviceName = GetServiceNameFromContainer(container);
            var containerInstance = new ComposeServiceWaitStrategyTarget(container.ID,
                _ambassadorContainer, _ambassadorPortMappings.TryGetValue(serviceName, out var mapping) ? mapping : new Dictionary<int, int>());

            var containerId = containerInstance.ContainerId;
            if (_tailChildContainers)
            {
                await FollowLogs(containerId, new LoggerConsumer(_logger).WithPrefix(container.Names[0]));
            }
            //follow logs using registered consumers for this service
            if (_logConsumers.TryGetValue(serviceName, out var consumers))
            {
                foreach (var consumer in consumers)
                {
                    FollowLogs(containerId, consumer);
                }
            }
            _serviceInstanceMap.TryAdd(serviceName, containerInstance);
        }

        private async Task FollowLogs(String containerId, IProgress<string> consumer)
        {
            await LogUtils.FollowOutput(containerId, consumer);
        }

        private string GetServiceNameFromContainer(ContainerListResponse container)
        {
            var containerName = container.Labels["com.docker.compose.service"];
            var containerNumber = container.Labels["com.docker.compose.container-number"];
            return string.Format("%s_%s", containerName, containerNumber);
        }

        private void RegisterContainersForShutdown()
        {
            throw new NotImplementedException();
        }

        private async Task PullImages()
        {
            // Pull images using our docker client rather than compose itself,
            // (a) as a workaround for https://github.com/docker/compose/issues/5854, which prevents authenticated image pulls being possible when credential helpers are in use
            // (b) so that credential helper-based auth still works when compose is running from within a container
            foreach (var imageName in _parsedComposeFiles.SelectMany(it => it.DependencyImageNames))
            {

                try
                {
                    _logger.LogInformation("Preemptively checking local images for '{imageName}', referenced via a compose file or transitive Dockerfile. If not available, it will be pulled.", imageName);
                    await DockerClientFactory.Instance.CheckAndPullImage(imageName);
                }
                catch (Exception e)
                {
                    _logger.LogWarning("Unable to pre-fetch an image ({imageName}) depended upon by Docker Compose build - startup will continue but may fail. Exception message was: {message}", imageName, e.Message);
                }
            };
        }

        private async Task WaitUntilServiceStarted(string serviceName, ComposeServiceWaitStrategyTarget serviceInstance)
        {
            if (_waitStrategyMap.TryGetValue(serviceName, out var waitAllStrategy))
            {
                if (waitAllStrategy != null)
                {
                    await waitAllStrategy.WaitUntilReady(serviceInstance);
                }
            }
        }

        private async Task RunWithCompose(string cmd)
        {
            //checkNotNull(composeFiles);
            //checkArgument(!composeFiles.isEmpty(), "No docker compose file have been provided");

            IDockerCompose dockerCompose;
            if (_localCompose)
            {
                dockerCompose = new LocalDockerCompose(_composeFiles, _project);
            }
            else
            {
                dockerCompose = new ContainerisedDockerCompose(_composeFiles, _project);
            }

            await dockerCompose
                //  .WithCommand(cmd)
                // .WithEnv(_env)
                .Invoke(default);
        }

        internal async Task<List<ContainerListResponse>> ListChildContainers()
        {
            return (await DockerClientFactory.Instance.Execute(c => c.Containers.ListContainersAsync(new ContainersListParameters { All = true })))
                .Where(container => container.Names.Any(name =>
                      name.StartsWith("/" + _project))).ToList();
        }

        private string RandomProjectId()
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        public Task FollowOutput(IProgress<string> consumer)
        {
            throw new NotImplementedException();
        }

        public Task FollowOutput(IProgress<string> consumer, params OutputType[] types)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetImage()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetContainerIpAddress(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsRunning(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsCreated(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsHealthy(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ContainerInspectResponse> GetCurrentContainerInfo(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetFirstMappedPort(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetMappedPort(int originalPort, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<int>> GetExposedPorts(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<string>> GetPortBindings(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<string>> GetBoundPortNumbers(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileToContainer(FileInfo fileInfo, string containerPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileFromContainer(string containerPath, string destinationPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileFromContainer(string containerPath, Func<Stream> destinationFunc, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileFromContainer(string containerPath, Func<Stream, Task> destinationFunc, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

#if !NETSTANDARD2_0
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
#endif


    }

    internal class ContainerisedDockerCompose : GenericContainer, IDockerCompose
    {
        string ENV_PROJECT_NAME = "COMPOSE_PROJECT_NAME";
        string ENV_COMPOSE_FILE = "COMPOSE_FILE";

        public static readonly string UNIX_PATH_SEPARATOR = ":";
        public static readonly DockerImageName DEFAULT_IMAGE_NAME = DockerImageName.Parse("docker/compose:1.24.1");

        public ContainerisedDockerCompose(IEnumerable<FileInfo> composeFiles, string identifier) : base(DEFAULT_IMAGE_NAME,new ContainerOptions())
        {

            AddEnv(ENV_PROJECT_NAME, identifier);

            // Map the docker compose file into the container
            var dockerComposeBaseFile = composeFiles.First();
            var pwd = dockerComposeBaseFile.Directory.FullName;
            var containerPwd = ConvertToUnixFilesystemPath(pwd);

            var absoluteDockerComposeFiles = composeFiles
                .Select(x => x.FullName)
                .Select(x => MountableFile.ForHostPath(x))
                .Select(x => x.FileSystemPath)
                .Select(x => ConvertToUnixFilesystemPath(x))
                .ToList();

            var composeFileEnvVariableValue = string.Join(UNIX_PATH_SEPARATOR, absoluteDockerComposeFiles); // we always need the UNIX path separator
            Logger.LogDebug("Set env COMPOSE_FILE={composeFileEnvVariableValue}", composeFileEnvVariableValue);

            AddEnv(ENV_COMPOSE_FILE, composeFileEnvVariableValue);
            AddFileSystemBind(pwd, containerPwd, BindMode.ReadWrite);

            // Ensure that compose can access docker. Since the container is assumed to be running on the same machine
            //  as the docker daemon, just mapping the docker control socket is OK.
            // As there seems to be a problem with mapping to the /var/run directory in certain environments (e.g. CircleCI)
            //  we map the socket file outside of /var/run, as just /docker.sock
            AddFileSystemBind(DockerClientFactory.Instance.GetRemoteDockerUnixSocketPath(), "/docker.sock", BindMode.ReadWrite);
            AddEnv("DOCKER_HOST", "unix:///docker.sock");
            SetStartupCheckStrategy(new IndefiniteWaitOneShotStartupCheckStrategy());
            SetWorkingDirectory(containerPwd);
        }

        private void AddFileSystemBind(object p, string v, object rEAD_WRITE)
        {
            throw new NotImplementedException();
        }

        private void SetWorkingDirectory(string containerPwd)
        {
            throw new NotImplementedException();
        }

        private void SetStartupCheckStrategy(IndefiniteWaitOneShotStartupCheckStrategy indefiniteWaitOneShotStartupCheckStrategy)
        {
            throw new NotImplementedException();
        }

        private void AddEnv(object eNV_PROJECT_NAME, string identifier)
        {
            throw new NotImplementedException();
        }

        public async Task Invoke(CancellationToken cancellationToken)
        {
            await base.Start(cancellationToken);

            //await FollowOutput(new LoggerConsumer(Logger));

            // wait for the compose container to stop, which should only happen after it has spawned all the service containers
            Logger.LogInformation("Docker Compose container is running for command: {command}", string.Join(" ", CommandParts));

            while (await IsRunning(cancellationToken))
            {
                Logger.LogTrace("Compose container is still running");
                //Uninterruptibles.sleepUninterruptibly(100, TimeUnit.MILLISECONDS);
                await Task.Delay(100);
            }
            Logger.LogInformation("Docker Compose has finished running");

            //AuditLogger.DoComposeLog(CommandParts, Env);

            var exitCode = (await DockerClientFactory.Instance.Execute(c => c.Containers.InspectContainerAsync(ContainerId))).State?.ExitCode;

            if (exitCode == null || exitCode != 0)
            {
                throw new ContainerLaunchException(
                    "Containerised Docker Compose exited abnormally with code " +
                    exitCode +
                    " whilst running command: " +
                    string.Join(" ", CommandParts));
            }
        }


        private string ConvertToUnixFilesystemPath(string path)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? PathUtils.CreateMinGWPath(path).Substring(1)
                : path;
        }
    }
}
