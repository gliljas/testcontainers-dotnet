using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using TestContainers.Containers;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Images;
using TestContainers.Lifecycle;

namespace TestContainers.Core.Containers
{
    public class GenericContainer : AbstractWaitStrategyTarget, IStartable
        #if !NETSTANDARD2_0
        , IAsyncDisposable
#endif
    {
        private IStartupCheckStrategy _startupCheckStrategy = new IsRunningStartupCheckStrategy();
        private ILogger _logger = null;
        private string _containerId;
        private ContainerInspectResponse _containerInfo;
        private RemoteDockerImage _image;

        protected IWaitStrategy WaitStrategy => Wait.DefaultWaitStrategy();

        public override ContainerInspectResponse ContainerInfo => _containerInfo;

        public string ImageName { get; private set; }

        //private const string TcpExposedPortFormat = "{0}/tcp";

        //static readonly UTF8Encoding Utf8EncodingWithoutBom = new UTF8Encoding(false);
        private readonly IDockerClient _dockerClient = DockerClientFactory.Instance.Client();

        private static readonly IRateLimiter DockerClientRateLimiter = null;
        private readonly string COPIED_FILES_HASH_LABEL = "testcontainers.dotnet.copied_files.hash";
        private readonly string HASH_LABEL = "testcontainers.dotnet.hash";
        private INetwork _network;
        private bool _shouldBeReused;

        //private readonly RemoteDockerImage _image;

        //string _containerId { get; set; }
        ////public string DockerImageName { get; set; }
        //public int[] ExposedPorts { get; set; }
        //public (int ExposedPort, int PortBinding)[] PortBindings { get; set; }
        //public (string key, string value)[] EnvironmentVariables { get; set; }
        //public (string key, string value)[] Labels { get; set; }
        //public ContainerInspectResponse ContainerInspectResponse { get; set; }
        //public (string SourcePath, string TargetPath, string Type)[] Mounts { get; set; }
        //public string[] Commands { get; set; }
        //public INetwork Network { get; internal set; }
        //public string NetworkMode { get; internal set; }
        //public bool PrivilegedMode { get; internal set; }
        //public string ImageName { get; internal set; }

        public GenericContainer(DockerImageName dockerImageName) : this(new RemoteDockerImage(dockerImageName))
        {
        }
        public GenericContainer(RemoteDockerImage image)
        {
            _image = image;
        }

        public GenericContainer(string dockerImageName)
        {
            SetDockerImageName(dockerImageName);
        }

        public void SetDockerImageName(string dockerImageName)
        {
            _image = new RemoteDockerImage(dockerImageName);
        }

        //override 

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (_containerId != null)
            {
                return;
            }

            await DoStart(cancellationToken);
        }

        protected async Task DoStart(CancellationToken cancellationToken)
        {
            try
            {
                Configure();

                var startedAt = DateTimeOffset.Now;

                _logger.LogDebug("Starting container: {dockerImageBane}", ImageName);

                var attempt = 0;

                var startupAttempts = 3;

                await Policy
                    .Handle<Exception>()//ex => !(ex is OperationCancelledException)
                    .OrResult<bool>(x => x == false)
                    .Retry(startupAttempts-1)
                    .ExecuteAsync(async (token) =>
                    {
                        _logger.LogDebug("Trying to start container: {imageName} (attempt {attempt}/{startupAttempts})", ImageName, Interlocked.Increment(ref attempt), startupAttempts);
                        await TryStart(startedAt, token);
                        return true;
                    }
                    , cancellationToken);

            }
            catch (Exception e)
            {
                throw new ContainerLaunchException("Container startup failed", e);
            }
        }

        protected virtual void Configure()
        {
        }

        private async Task TryStart(DateTimeOffset startedAt, CancellationToken cancellationToken)
        {
            try
            {
                var dockerImageName = ImageName;
                _logger.LogDebug("Starting container: {dockerImageName}", dockerImageName);

                _logger.LogInformation("Creating container for image: {dockerImageName}", dockerImageName);

                var createCommand = new CreateContainerParameters { Image = dockerImageName };

                ApplyConfiguration(createCommand);

                createCommand.Labels[DockerClientFactory.TESTCONTAINERS_LABEL] = "true";

                var reused = false;
                var reusable = false;
                if (_shouldBeReused)
                {
                    if (!CanBeReused())
                    {
                        throw new IllegalStateException("This container does not support reuse");
                    }

                    if (TestContainersConfiguration.Instance.EnvironmentSupportsReuse)
                    {
                  //      createCommand.Labels[COPIED_FILES_HASH_LABEL] = Long.toHexString(hashCopiedFiles().getValue());


                        var hash = Hash(createCommand);

                        var containerId = (await FindContainerForReuse(hash)) ?? null;

                        if (containerId != null)
                        {
                            _logger.LogInformation("Reusing container with ID: {containerId} and hash: {hash}", containerId, hash);
                            reused = true;
                        }
                        else
                        {
                            _logger.LogDebug("Can't find a reusable running container with hash: {hash}", hash);

                            createCommand.Labels[HASH_LABEL] = hash;
                        }
                        reusable = true;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "" +
                                "Reuse was requested but the environment does not support the reuse of containers\n" +
                                "To enable reuse of containers, you must set 'testcontainers.reuse.enable=true' in a file located at {path}",
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".testcontainers.properties")
                        );
                        reusable = false;
                    }
                }
                else
                {
                    reusable = false;
                }

                if (!reusable)
                {
                    createCommand.Labels[DockerClientFactory.TESTCONTAINERS_SESSION_ID_LABEL] = DockerClientFactory.SESSION_ID;
                }

                if (!reused)
                {
                    var createResponse = await _dockerClient.Containers.CreateContainerAsync(createCommand, cancellationToken);

                    _containerId = createResponse.ID;

                    // TODO use single "copy" invocation (and calculate a hash of the resulting tar archive)
                    //copyToFileContainerPathMap.forEach(CopyFileToContainer);
                }

                //  ConnectToPortForwardingNetwork(createCommand.);

                if (!reused)
                {
                    await ContainerIsCreated(_containerId);

                    _logger.LogInformation("Starting container with ID: {containerId}", _containerId);

                    await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters(), cancellationToken);
                }

                _logger.LogInformation("Container {} is starting: {}", dockerImageName, _containerId);

                // For all registered output consumers, start following as close to container startup as possible
                //this.logConsumers.forEach(this::followOutput);

                // Tell subclasses that we're starting
                _containerInfo = await _dockerClient.Containers.InspectContainerAsync(_containerId);
                await ContainerIsStarting(_containerInfo, reused);

                // Wait until the container has reached the desired running state
                if (!await _startupCheckStrategy.WaitUntilStartupSuccessful(_dockerClient, _containerId))
                {
                    // Bail out, don't wait for the port to start listening.
                    // (Exception thrown here will be caught below and wrapped)
                    throw new IllegalStateException("Container did not start correctly.");
                }

                // Wait until the process within the container has become ready for use (e.g. listening on network, log message emitted, etc).
                try
                {
                    await WaitUntilContainerStarted(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogDebug("Wait strategy threw an exception", e);
                    ContainerInspectResponse inspectContainerResponse = null;
                    try
                    {
                        inspectContainerResponse = await _dockerClient.Containers.InspectContainerAsync(_containerId);
                    }
                    catch (DockerContainerNotFoundException notFoundException)
                    {
                        _logger.LogDebug("Container {} not found", _containerId, notFoundException);
                    }

                    if (inspectContainerResponse == null)
                    {
                        throw new IllegalStateException("Container is removed");
                    }

                    ContainerState state = inspectContainerResponse.State;

                    if (state.Dead)
                    {
                        throw new IllegalStateException("Container is dead");
                    }

                    if (state.OOMKilled)
                    {
                        throw new IllegalStateException("Container crashed with out-of-memory (OOMKilled)");
                    }

                    var error = state.Error;

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        throw new IllegalStateException("Container crashed: " + error);
                    }

                    if (!state.Running)
                    {
                        throw new IllegalStateException("Container exited with code " + state.ExitCode);
                    }

                    throw e;
                }

                _logger.LogInformation("Container {} started in {}", dockerImageName, DateTimeOffset.Now - startedAt);

                await ContainerIsStarted(_containerInfo, reused);
            }
            catch (Exception e)
            {

                _logger.LogError("Could not start container", e);

                if (_containerId != null)
                {
                    // Log output if startup failed, either due to a container failure or exception (including timeout)
                    string containerLogs = await GetLogs();

                    if (containerLogs.Length > 0)
                    {
                        _logger.LogError("Log output from the failed container:\n{}", containerLogs);
                    }
                    else
                    {
                        _logger.LogError("There are no stdout/stderr logs available for the failed container");
                    }
                }

                throw new ContainerLaunchException("Could not create/start container", e);
            }
        }

        private string Hash(CreateContainerParameters createCommand)
        {
            var json = JsonConvert.SerializeObject(new { TestContainersVersion = "", Command = createCommand });
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(json));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
            
        }

        private Task<string> FindContainerForReuse(string hash)
        {
            throw new NotImplementedException();
        }

        private bool CanBeReused()
        {
            throw new NotImplementedException();
        }

        private void ApplyConfiguration(CreateContainerParameters createCommand)
        {
            createCommand.HostConfig = BuildHostConfig();

            //createCommand.ExposedPorts = _ex

            //createCommand.p

        }

        private HostConfig BuildHostConfig()
        {
            throw new NotImplementedException();
        }

        private Task<string> GetLogs()
        {
            throw new NotImplementedException();
        }


        protected Task ContainerIsStarted(ContainerInspectResponse containerInfo)
        {
            return Task.CompletedTask;
        }
        protected Task ContainerIsStarted(ContainerInspectResponse containerInfo, bool reused) => ContainerIsStarted(containerInfo);

        protected virtual async Task WaitUntilContainerStarted(CancellationToken cancellationToken)
        {
            await WaitStrategy?.WaitUntilReady(this, cancellationToken);
        }


        protected Task ContainerIsStarting(ContainerInspectResponse containerInfo)
        {
            return Task.CompletedTask;
        }
        protected Task ContainerIsStarting(ContainerInspectResponse containerInfo, bool reused) => ContainerIsStarting(containerInfo);


        protected Task ContainerIsStopping(ContainerInspectResponse containerInfo)
        {
            return Task.CompletedTask;
        }

        protected Task ContainerIsStopped(ContainerInspectResponse containerInfo)
        {
            return Task.CompletedTask;
        }
        protected Task ContainerIsCreated(string containerId)
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }


        //{
        //    var started = await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());

        //    if (started)
        //    {
        //        using (var logs = await _dockerClient.Containers.GetContainerLogsAsync(_containerId,
        //            new ContainerLogsParameters
        //            {
        //                ShowStderr = true,
        //                ShowStdout = true,
        //            }, default(CancellationToken)))
        //        {
        //            using (var reader = new StreamReader(logs, Utf8EncodingWithoutBom))
        //            {
        //                string nextLine;
        //                while ((nextLine = await reader.ReadLineAsync()) != null)
        //                {
        //                    Debug.WriteLine(nextLine);
        //                }
        //            }
        //        }
        //    }

        //    await WaitUntilContainerStarted();
        //}

        //protected virtual async Task WaitUntilContainerStarted()
        //{
        //    var retryUntilContainerStateIsRunning = Policy
        //                        .HandleResult<ContainerInspectResponse>(c => !c.State.Running)
        //                        .RetryForeverAsync();

        //    var containerInspectPolicy = await Policy
        //        .TimeoutAsync(TimeSpan.FromMinutes(1))
        //        .WrapAsync(retryUntilContainerStateIsRunning)
        //        .ExecuteAndCaptureAsync(async () => ContainerInspectResponse = await _dockerClient.Containers.InspectContainerAsync(_containerId));

        //    if (containerInspectPolicy.Outcome == OutcomeType.Failure)
        //        throw new ContainerLaunchException("Container startup failed", containerInspectPolicy.FinalException);
        //}

        //async Task<string> Create()
        //{
        //    var progress = new Progress<JSONMessage>(async (m) =>
        //    {
        //        Console.WriteLine(m.Status);
        //        if (m.Error != null)
        //            await Console.Error.WriteLineAsync(m.ErrorMessage);

        //    });

        //    var tag = ImageName.Split(':').Last();
        //    var imagesCreateParameters = new ImagesCreateParameters
        //    {
        //        FromImage = ImageName,
        //        Tag = tag,
        //    };

        //    var images = await this._dockerClient.Images.ListImagesAsync(new ImagesListParameters { MatchName = ImageName });

        //    if (!images.Any())
        //    {
        //        await this._dockerClient.Images.CreateImageAsync(imagesCreateParameters, new AuthConfig(), progress, CancellationToken.None);
        //    }

        //    var createContainersParams = ApplyConfiguration();
        //    var containerCreated = await _dockerClient.Containers.CreateContainerAsync(createContainersParams);

        //    return containerCreated.ID;
        //}

        //CreateContainerParameters ApplyConfiguration()
        //{
        //    var exposedPorts = ExposedPorts?.ToList() ?? new List<int>();

        //    var cfg = new Config
        //    {
        //        Image = ImageName,
        //        Env = EnvironmentVariables?.Select(ev => $"{ev.key}={ev.value}").ToList(),
        //        ExposedPorts = exposedPorts.ToDictionary(e => $"{e}/tcp", e => default(EmptyStruct)),
        //        Labels = Labels?.ToDictionary(l => l.key, l => l.value),
        //        Tty = true,
        //        Cmd = Commands,
        //        AttachStderr = true,
        //        AttachStdout = true,
        //    };

        //    return new CreateContainerParameters(cfg)
        //    {
        //        HostConfig = new HostConfig
        //        {
        //            Privileged = PrivilegedMode,
        //            PortBindings = PortBindings?.ToDictionary(
        //                e => string.Format(TcpExposedPortFormat, e.ExposedPort),
        //                e => (IList<PortBinding>) new List<PortBinding>
        //                {
        //                    new PortBinding
        //                    {
        //                        HostPort = e.PortBinding.ToString()
        //                    }
        //                }),
        //            Mounts = Mounts?.Select(m => new Mount
        //            {
        //                Source = m.SourcePath,
        //                Target = m.TargetPath,
        //                Type = m.Type,
        //                }).ToList(),
        //            PublishAllPorts = true,
        //            NetworkMode = NetworkMode
        //        }
        //    };
        //}

        //public async Task Stop()
        //{
        //    if (string.IsNullOrWhiteSpace(_containerId))
        //    {
        //        return;
        //    }

        //    await _dockerClient.Containers.StopContainerAsync(ContainerInspectResponse.ID, new ContainerStopParameters());
        //    await _dockerClient.Containers.RemoveContainerAsync(ContainerInspectResponse.ID, new ContainerRemoveParameters());
        //}


        //public int GetMappedPort(int exposedPort)
        //{
        //    if (ContainerInspectResponse == null)
        //    {
        //        throw new InvalidOperationException(
        //            "Container must be started before mapped ports can be retrieved");
        //    }

        //    var tcpExposedPort = string.Format(TcpExposedPortFormat, exposedPort);

        //    if (ContainerInspectResponse.NetworkSettings.Ports.TryGetValue(tcpExposedPort, out var binding) &&
        //        binding.Count > 0 &&
        //        int.TryParse(binding[0].HostPort, out var mappedPort))
        //    {
        //        return mappedPort;
        //    }

        //    throw new InvalidOperationException($"ExposedPort[{exposedPort}] is not mapped");
        //}

        //public async Task ExecuteCommand(params string[] command)
        //{
        //    var containerExecCreateParams = new ContainerExecCreateParameters
        //    {
        //        AttachStderr = true,
        //        AttachStdout = true,
        //        Cmd = command
        //    };

        //    var response = await _dockerClient.Containers.ExecCreateContainerAsync(_containerId, containerExecCreateParams);

        //    await _dockerClient.Containers.StartContainerExecAsync(response.ID);
        //}


        //protected virtual Task ContainerStarting() => Task.CompletedTask;

        //protected virtual Task ContainerStarted() => Task.CompletedTask;

        //public override ContainerInspectResponse GetContainerInfo()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
