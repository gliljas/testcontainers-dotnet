using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Lifecycle;

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
        private readonly ConcurrentDictionary<string, List<IConsumer<OutputFrame>>> _logConsumers = new ConcurrentDictionary<string, List<IConsumer<OutputFrame>>>();

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
            var serviceNameArgs =  string.Join(" ", _services
                .Concat(_scalingPreferences.Keys) 
                .Distinct());

            // Apply scaling for the services specified using `withScaledService`
            var scalingOptions = string.Join(" ", _scalingPreferences
                .Select(entry=> entry.Key + "=" + entry.Value)
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

            Set<String> servicesToWaitFor = waitStrategyMap.keySet();
            Set<String> instantiatedServices = serviceInstanceMap.keySet();
            Sets.SetView<String> missingServiceInstances =
                Sets.difference(servicesToWaitFor, instantiatedServices);

            if (!missingServiceInstances.isEmpty())
            {
                throw new IllegalStateException(
                    "Services named " + missingServiceInstances + " " +
                        "do not exist, but wait conditions have been defined " +
                        "for them. This might mean that you misspelled " +
                        "the service name when defining the wait condition.");
            }

            serviceInstanceMap.forEach(this::waitUntilServiceStarted);
        }

        private Task CreateServiceInstance(ContainerListResponse container)
        {
            var serviceName = GetServiceNameFromContainer(container);
            var containerInstance = new ComposeServiceWaitStrategyTarget(container,
                _ambassadorContainer, _ambassadorPortMappings.TryGetValue(serviceName, out var mapping) ? mapping : new Dictionary<int, int>());

            var containerId = containerInstance.ContainerId;
            if (_tailChildContainers)
            {
                //FollowLogs(containerId, new Slf4jLogConsumer(log).withPrefix(container.getNames()[0]));
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
            foreach(var imageName in _parsedComposeFiles.SelectMany(it => it.DependencyImageNames))
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
}
