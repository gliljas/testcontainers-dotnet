using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Lifecycle;

namespace TestContainers.Containers
{
    public class DockerComposeContainer : IStartable
    {
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

        private static readonly SemaphoreSlim MUTEX = new SemaphoreSlim(1,1);

        private List<String> _services = new List<string>();

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
            _parsedComposeFiles = new HashSet<ParsedDockerComposeFile>(composeFiles.Select(x=>new ParsedDockerComposeFile(x)));

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
                        PullImages();
                    }
                    catch (ContainerLaunchException e)
                    {
                        log.warn("Exception while pulling images, using local images if available", e);
                    }
                }
                CreateServices();
                StartAmbassadorContainers();
                WaitUntilServiceStarted();
            }
            finally
            {
                MUTEX.Release();
            }
        }

        private async Task PullImages()
        {
            // Pull images using our docker client rather than compose itself,
            // (a) as a workaround for https://github.com/docker/compose/issues/5854, which prevents authenticated image pulls being possible when credential helpers are in use
            // (b) so that credential helper-based auth still works when compose is running from within a container
            _parsedComposeFiles.SelectMany(it=>it.GetDependencyImageNames()).S
                .flatMap(it->it.getDependencyImageNames().stream())
                .forEach(imageName-> {
                try
                {
                    log.info("Preemptively checking local images for '{}', referenced via a compose file or transitive Dockerfile. If not available, it will be pulled.", imageName);
                    DockerClientFactory.instance().checkAndPullImage(dockerClient, imageName);
                }
                catch (Exception e)
                {
                    log.warn("Unable to pre-fetch an image ({}) depended upon by Docker Compose build - startup will continue but may fail. Exception message was: {}", imageName, e.getMessage());
                }
            });
        }

        private string RandomProjectId()
        {
            throw new NotImplementedException();
        }
    }
}
