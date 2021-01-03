using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using TestContainers.Containers.Output;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;
using TestContainers.Images;
using TestContainers.Lifecycle;
using TestContainers.Utility;

namespace TestContainers.Containers
{
    public class DockerComposeContainer : IStartable
    {
        private ILogger _logger;
        /**
     * Random identifier which will become part of spawned containers names, so we can shut them down
     */
        private readonly string _identifier;
        private readonly IReadOnlyList<FileInfo> _composeFiles;
        private ISet<ParsedDockerComposeFile> _parsedComposeFiles;
        private readonly Dictionary<string, int> _scalingPreferences = new Dictionary<string, int>();
        private IDockerClient _dockerClient;
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

            _dockerClient = DockerClientFactory.Instance.Client();
        }

        public async Task Start(CancellationToken cancellationToken)
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
            await LogUtils.FollowOutput(DockerClientFactory.Instance.Client(), containerId, consumer);
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
                    await DockerClientFactory.Instance.CheckAndPullImage(_dockerClient, imageName);
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
                dockerCompose = new ContainerisedDockerCompose(composeFiles, project);
            }

            await dockerCompose
                .WithCommand(cmd)
                .WithEnv(env)
                .Invoke();
        }

        internal async Task<List<ContainerListResponse>> ListChildContainers()
        {
            return (await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true }))
                .Where(container => container.Names.Any(name =>
                      name.StartsWith("/" + _project))).ToList();
        }

        private string RandomProjectId()
        {
            throw new NotImplementedException();
        }
    }

    internal class ContainerisedDockerCompose : GenericContainer, IDockerCompose
    {
        public static readonly char UNIX_PATH_SEPERATOR = ':';
        public static readonly DockerImageName DEFAULT_IMAGE_NAME = DockerImageName.Parse("docker/compose:1.24.1");

        public ContainerisedDockerCompose(List<FileInfo> composeFiles, string identifier) : base(DEFAULT_IMAGE_NAME)
        {

            AddEnv(ENV_PROJECT_NAME, identifier);

            // Map the docker compose file into the container
            var dockerComposeBaseFile = composeFiles.First();
            var pwd = dockerComposeBaseFile.Directory.FullName;
            var containerPwd = ConvertToUnixFilesystemPath(pwd);

            var absoluteDockerComposeFiles = composeFiles
                .Select(x => x.FullName)
                .Select(x => MountableFile.ForHostPath(x))
                .Select(x => x.GetFilesystemPath())
                .Select(x => ConvertToUnixFilesystemPath(x))
                .ToList();

            var composeFileEnvVariableValue = string.Join(UNIX_PATH_SEPERATOR, absoluteDockerComposeFiles); // we always need the UNIX path separator
            Logger.LogDebug("Set env COMPOSE_FILE={composeFileEnvVariableValue}", composeFileEnvVariableValue);

            AddEnv(ENV_COMPOSE_FILE, composeFileEnvVariableValue);
            AddFileSystemBind(pwd, containerPwd, READ_WRITE);

            // Ensure that compose can access docker. Since the container is assumed to be running on the same machine
            //  as the docker daemon, just mapping the docker control socket is OK.
            // As there seems to be a problem with mapping to the /var/run directory in certain environments (e.g. CircleCI)
            //  we map the socket file outside of /var/run, as just /docker.sock
            AddFileSystemBind(DockerClientFactory.Instance.GetRemoteDockerUnixSocketPath(), "/docker.sock", READ_WRITE);
            AddEnv("DOCKER_HOST", "unix:///docker.sock");
            SetStartupCheckStrategy(new IndefiniteWaitOneShotStartupCheckStrategy());
            SetWorkingDirectory(containerPwd);
        }


        public override async Task Invoke()
        {
            await _base.Start();

            await FollowOutput(new LoggerConsumer(Logger));

            // wait for the compose container to stop, which should only happen after it has spawned all the service containers
            Logger.LogInformation("Docker Compose container is running for command: {command}", string.Join(" ", GetCommandParts()));

            while (await IsRunning())
            {
                Logger.LogTrace("Compose container is still running");
                //Uninterruptibles.sleepUninterruptibly(100, TimeUnit.MILLISECONDS);
                await Task.Delay(100);
            }
            Logger.LogInformation("Docker Compose has finished running");

            AuditLogger.DoComposeLog(GetCommandParts(), GetEnv());

            var exitCode = (await _dockerClient.Containers.InspectContainerAsync(ContainerId)).State?.ExitCode;

            if (exitCode == null || exitCode != 0)
            {
                throw new ContainerLaunchException(
                    "Containerised Docker Compose exited abnormally with code " +
                    exitCode +
                    " whilst running command: " +
                    string.Join(' ', this.GetCommandParts()));
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

internal interface IDockerCompose
{
    IDockerCompose WithCommand(string cmd);

    IDockerCompose WithEnv(Dictionary<string, string> env);

    Task Invoke();
}

internal class LocalDockerCompose : IDockerCompose
{
    string ENV_PROJECT_NAME = "COMPOSE_PROJECT_NAME";
    string ENV_COMPOSE_FILE = "COMPOSE_FILE";

    /**
     * Executable name for Docker Compose.
     */
    private static readonly string COMPOSE_EXECUTABLE = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "docker-compose.exe" : "docker-compose";

    private readonly IReadOnlyList<FileInfo> _composeFiles;
    private readonly string _identifier;
    private string _cmd = "";
    private Dictionary<string, string> _env = new Dictionary<string, string>();

    public LocalDockerCompose(IReadOnlyList<FileInfo> composeFiles, String identifier)
    {
        _composeFiles = composeFiles;
        _identifier = identifier;
    }

    public IDockerCompose WithCommand(string cmd)
    {
        _cmd = cmd;
        return this;
    }


    public IDockerCompose WithEnv(Dictionary<string, string> env)
    {
        _env = env;
        return this;
    }

    internal static bool ExecutableExists()
    {
        return CommandLine.ExecutableExists(COMPOSE_EXECUTABLE);
    }

    public async Task Invoke()
    {
        // bail out early
        if (!ExecutableExists())
        {
            throw new ContainerLaunchException("Local Docker Compose not found. Is " + COMPOSE_EXECUTABLE + " on the PATH?");
        }

        var environment = _env.ToDictionary(x => x.Key, x => x.Value);
        environment[ENV_PROJECT_NAME] = _identifier;

        var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        if (dockerHost == null)
        {
            TransportConfig transportConfig = DockerClientFactory.instance().getTransportConfig();
            SSLConfig sslConfig = transportConfig.getSslConfig();
            if (sslConfig != null)
            {
                if (sslConfig instanceof LocalDirectorySSLConfig) {
                    environment.put("DOCKER_CERT_PATH", ((LocalDirectorySSLConfig) sslConfig).getDockerCertPath());
                    environment.put("DOCKER_TLS_VERIFY", "true");
                } else
                {
                    logger().warn("Couldn't set DOCKER_CERT_PATH. `sslConfig` is present but it's not LocalDirectorySSLConfig.");
                }
            }
            dockerHost = transportConfig.getDockerHost().toString();
        }
        environment["DOCKER_HOST"] dockerHost);

        var absoluteDockerComposeFilePaths = _composeFiles
            .Select(x => x.FullName);

        var composeFileEnvVariableValue = string.Join(Path.PathSeparator.ToString(), absoluteDockerComposeFilePaths);
        Logger.LogDebug("Set env COMPOSE_FILE={composeFileEnvVariableValue}", composeFileEnvVariableValue);

        var pwd = _composeFiles[0].Directory.FullName;
        environment[ENV_COMPOSE_FILE] = composeFileEnvVariableValue;

        Logger.LogInformation("Local Docker Compose is running command: {cmd}", _cmd);

        var command = (COMPOSE_EXECUTABLE + " " + _cmd).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        try
        {
            new ProcessExecutor().command(command)
                .redirectOutput(Slf4jStream.of(logger()).asInfo())
                .redirectError(Slf4jStream.of(logger()).asInfo()) // docker-compose will log pull information to stderr
                .environment(environment)
                .directory(pwd)
                .exitValueNormal()
                .executeNoTimeout();

            Logger.LogInformation("Docker Compose has finished running");

        }
        catch (InvalidExitValueException e)
        {
            throw new ContainerLaunchException("Local Docker Compose exited abnormally with code " +
                                               e.getExitValue() + " whilst running command: " + cmd);

        }
        catch (Exception e)
        {
            throw new ContainerLaunchException("Error running local Docker Compose command: " + cmd, e);
        }
    }

    /**
     * @return a logger
     */
    private ILogger Logger => DockerLoggerFactory.GetLogger(COMPOSE_EXECUTABLE);

}
}
