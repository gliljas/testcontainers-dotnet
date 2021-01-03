using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Docker.DotNet;
using Microsoft.Extensions.Logging;

namespace TestContainers
{
    public sealed class DockerClientFactory
    {
        static ILogger _logger;
        static volatile DockerClientFactory instance;
        static object syncRoot = new Object();
        internal IDockerClient _dockerClient;
        internal Exception _cachedClientFailure;
        public static readonly string TESTCONTAINERS_SESSION_ID_LABEL;
        public static readonly string TESTCONTAINERS_LABEL;
        public static readonly string SESSION_ID = Guid.NewGuid().ToString("N");

        //DockerClientProviderStrategy strategy { get; } = DockerClientProviderStrategy.GetFirstValidStrategy();
        public static DockerClientFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DockerClientFactory();
                    }
                }
                return instance;
            }
        }

        public static IDockerClient LazyClient => LazyDockerClient.Instance;

        public IDockerClient Client()
        {
            return default;
        }

        //public IDockerClient Client()
        //{
        //    if (_dockerClient != null)
        //    {
        //        return _dockerClient;
        //    }

        //    // fail-fast if checks have failed previously
        //    if (_cachedClientFailure != null)
        //    {
        //        _logger.LogDebug(_cachedClientFailure, "There is a cached checks failure - throwing");
        //        throw _cachedClientFailure;
        //    }

        //    var strategy = GetOrInitializeStrategy();

        //    _logger.LogInformation("Docker host IP address is {ipaddress}", strategy.getDockerHostIpAddress());
        //    IDockerClient client = new DelegatingDockerClient(strategy.GetDockerClient())
        //    {
        //        //@Override
        //        //public void close()
        //        //{
        //        //    throw new IllegalStateException("You should never close the global DockerClient!");
        //        //}
        //    };

        //    var dockerInfo = await client.System.GetSystemInfoAsync();
        //    var version = await client.System.GetVersionAsync();
        //    _activeApiVersion = version.APIVersion;
        //    _activeExecutionDriver = dockerInfo.Driver;
        //    _logger.LogInformation("Connected to docker: \n" +
        //            "  Server Version: {serverVersion}\n" +
        //            "  API Version: {activeApiVersion}\n" +
        //            "  Operating System: {operatingSystem}\n" +
        //            "  Total Memory: {totalMemoryMB} MB", dockerInfo.ServerVersion, _activeApiVersion, dockerInfo.OperatingSystem, dockerInfo.MemTotal / (1024 * 1024));

        //    string ryukContainerId;

        //    var useRyuk = !bool.Parse(Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED"));
        //    if (useRyuk)
        //    {
        //        _logger.LogDebug("Ryuk is enabled");
        //        try
        //        {
        //            //noinspection deprecation
        //            ryukContainerId = await ResourceReaper.Start(client);
        //        }
        //        catch (Exception e)
        //        {
        //            _cachedClientFailure = e;
        //            throw e;
        //        }
        //        _logger.LogInformation("Ryuk started - will monitor and terminate Testcontainers containers on exit");
        //    }
        //    else
        //    {
        //        _logger.LogDebug("Ryuk is disabled");
        //        _ryukContainerId = null;
        //    }

        //    var checksEnabled = !TestContainersConfiguration.Instance.IsDisableChecks();
        //    if (checksEnabled)
        //    {
        //        _logger.LogDebug("Checks are enabled");

        //        try
        //        {
        //            _logger.LogInfo("Checking the system...");
        //            CheckDockerVersion(version.Version);
        //            if (_ryukContainerId != null)
        //            {
        //                CheckDiskSpace(client, _ryukContainerId);
        //            }
        //            else
        //            {
        //                RunInsideDocker(
        //                    client,
        //                    createContainerCmd-> {
        //                    createContainerCmd.withName("testcontainers-checks-" + SESSION_ID);
        //                    createContainerCmd.getHostConfig().withAutoRemove(true);
        //                    createContainerCmd.withCmd("tail", "-f", "/dev/null");
        //                },
        //                (__, containerId)-> {
        //                    checkDiskSpace(client, containerId);
        //                    return "";
        //                }
        //            );
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            _cachedClientFailure = e;
        //            throw e;
        //        }
        //    }
        //    else
        //    {
        //        _logger.LogDebug("Checks are disabled");
        //    }

        //    _dockerClient = client;
        //    return _dockerClient;
        //}
    }
}
