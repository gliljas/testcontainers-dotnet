using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Polly;
using TestContainers.Containers;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Images;

namespace TestContainers.Core.Containers
{
    public class GenericContainer : AbstractWaitStrategyTarget
    {
        private IStartupCheckStrategy _startupCheckStrategy = new IsRunningStartupCheckStrategy();
        private ILogger _logger = null;
        private string _containerId;
        private ContainerInspectResponse _containerInfo;

        protected IWaitStrategy WaitStrategy => Wait.DefaultWaitStrategy();

        //private const string TcpExposedPortFormat = "{0}/tcp";

        //static readonly UTF8Encoding Utf8EncodingWithoutBom = new UTF8Encoding(false);
        private readonly IDockerClient _dockerClient = DockerClientFactory.Instance.Client();

        private static readonly IRateLimiter DockerClientRateLimiter = null;

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
            _image = image;
        }

        //override 

        public async Task Start()
        {
            if (_containerId != null)
            {
                return;
            }

            _containerId = await Create();
            await TryStart();
        }

        protected async Task DoStart(CancellationToken cancellationToken)
        {
            try
            {
                Configure();

                var startedAt = DateTimeOffset.Now;

                _logger.LogDebug("Starting container: {dockerImageBane}", DockerImageName);

                var attempt = 0;

                Policy
                    .Handle<Exception>()//ex => !(ex is OperationCancelledException)
                    .OrResult<bool>(x => x == false)
                    .Retry(1)
                    .ExecuteAsync(async (context, token) =>
                    {
                        _logger.LogDebug("Trying to start container: {} (attempt {}/{})", DockerImageName, Interlocked.Increment(ref attempt), startupAttempts);
                        await TryStart(cancellationToken);
                        return true;
                    }

                    , cancellationToken);

            }
            catch (Exception e)
            {
                throw new ContainerLaunchException("Container startup failed", e);
            }
        }

        protected void Configure()
        {
        }

        private async Task TryStart(DateTimeOffset startedAt, CancellationToken cancellationToken)
        {
            try
            {
                var dockerImageName = DockerImageName;
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
                        createCommand.Labels[COPIED_FILES_HASH_LABEL] = Long.toHexString(hashCopiedFiles().getValue());


                        var hash = hash(createCommand);

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
                            Paths.get(System.getProperty("user.home"), ".testcontainers.properties")
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
                    copyToFileContainerPathMap.forEach(this::copyFileToContainer);
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
                    await WaitUntilContainerStarted();
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

        private Task<string> GetLogs()
        {
            throw new NotImplementedException();
        }


        protected Task ContainerIsStarted(ContainerInspectResponse containerInfo)
        {
            return Task.CompletedTask;
        }
        protected Task ContainerIsStarted(ContainerInspectResponse containerInfo, bool reused) => ContainerIsStarted(containerInfo);

        private async Task WaitUntilContainerStarted()
        {
            await WaitStrategy?.WaitUntilReady(this);
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

    public static class DockerClientExtensions
    {
        public static string GetDockerHostIpAddress(this IDockerClient dockerClient)
        {
            var dockerHostUri = dockerClient.Configuration.EndpointBaseUri;

            switch (dockerHostUri.Scheme)
            {
                case "http":
                case "https":
                case "tcp":
                    return dockerHostUri.Host;
                case "npipe": //will have to revisit this for LCOW/WCOW
                              //case "unix":
                              //    return File.Exists("/.dockerenv")
                              //        ? ContainerInspectResponse.NetworkSettings.Gateway
                              //        : "localhost";
                default:
                    return null;
            }
        }
    }
}
