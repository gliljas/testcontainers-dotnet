using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Polly;
using TestContainers.Containers;
using TestContainers.Core.Containers;
using TestContainers.Utility;

namespace TestContainers
{
    public abstract class DockerClientProviderStrategy
    {
        private static ILogger _logger;

        private static bool FAIL_FAST_ALWAYS = false;
        protected abstract DockerClientConfiguration Config { get; }
        protected abstract bool IsApplicable();
        protected abstract bool IsPersistable();

       
        public static async Task<DockerClientProviderStrategy> GetFirstValidStrategy(IEnumerable<DockerClientProviderStrategy> strategies)
        {
            if (FAIL_FAST_ALWAYS)
            {
                throw new IllegalStateException("Previous attempts to find a Docker environment failed. Will not retry. Please see logs and check configuration");
            }

            var configurationFailures = new List<string>();

            var foundStrategy = Enumerable.Concat<DockerClientProviderStrategy>(
                            new[] { TestContainersConfiguration.Instance.DockerClientStrategyClassName }
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Select(x =>
                            {
                                try
                                {
                                    var type = Type.GetType(x);
                                    if (type != null && typeof(DockerClientProviderStrategy).IsAssignableFrom(type))
                                    {
                                        return Activator.CreateInstance(type) as DockerClientProviderStrategy;
                                    }
                                }
                                catch
                                {
                                    //                        catch (ClassNotFoundException e)
                                    //                {
                                    //                    log.warn("Can't instantiate a strategy from {} (ClassNotFoundException). " +
                                    //                            "This probably means that cached configuration refers to a client provider " +
                                    //                            "class that is not available in this version of Testcontainers. Other " +
                                    //                            "strategies will be tried instead.", it);
                                    //                    return Stream.empty();
                                    //                }
                                    //                catch (InstantiationException | IllegalAccessException e) {
                                    //    log.warn("Can't instantiate a strategy from {}", it, e);
                                    //    return Stream.empty();
                                    //}
                                }
                                return null;
                            })
                            // Ignore persisted strategy if it's not persistable anymore
                            .Where(x => x != null && x.IsPersistable())
                            .Peek(strategy => _logger.LogInformation("Loaded {strategyType} from ~/.testcontainers.properties, will try it first", strategy.GetType().Name)),
                            strategies
                            .Where(x => x.IsApplicable())
                            .OrderByDescending(x => x.Priority)
                   );

            foreach (var strategy in foundStrategy)
            {
                try
                {
                    var dockerClient = strategy.GetDockerClient();
                    SystemInfoResponse info;
                    try
                    {
                        var p = Policy
                            .TimeoutAsync(30)
                            .WrapAsync<SystemInfoResponse>(
                                Policy<SystemInfoResponse>.Handle<Exception>().RetryForeverAsync());

                        info = await p.ExecuteAsync(() =>
                        {
                            _logger.LogDebug("Pinging docker daemon...");
                            return dockerClient.System.GetSystemInfoAsync();
                        });


                    }
                    catch (TimeoutException e)
                    {
                        //IOUtils.closeQuietly(dockerClient);
                        throw e;
                    }
                    _logger.LogInformation("Found Docker environment with {description}", strategy.Description);
                    _logger.LogDebug(
                        "Transport type: '{type}', Docker host: '{host}'",
                        TestContainersConfiguration.Instance.TransportType,
                        strategy.TransportConfig.DockerHost
                    );

                    _logger.LogDebug("Checking Docker OS type for {description}", strategy.Description);
                    var osType = info.OSType;
                    if (string.IsNullOrWhiteSpace(osType))
                    {
                        _logger.LogWarning("Could not determine Docker OS type");
                    }
                    else if (!osType.Equals("linux"))
                    {
                        _logger.LogWarning("{osType} is currently not supported", osType);
                        throw new InvalidConfigurationException(osType + " containers are currently not supported");
                    }

                    if (strategy.IsPersistable())
                    {
                        TestContainersConfiguration.Instance.UpdateGlobalConfig("docker.client.strategy", strategy.GetType().AssemblyQualifiedName);
                    }
                    return strategy;
                }
                catch (Exception e) //when (ex is  | ExceptionInInitializerError | NoClassDefFoundError e) {
                {
                    var throwableMessage = e.Message;
                    //@SuppressWarnings("ThrowableResultOfMethodCallIgnored")
                    var rootCause = e.InnerException ?? e;
                    var rootCauseMessage = rootCause.Message;

                    string failureDescription;
                    if (throwableMessage != null && throwableMessage.Equals(rootCauseMessage))
                    {
                        failureDescription = string.Format("%s: failed with exception %s (%s)",
                                strategy.GetType().Name,
                                e.GetType().Name,
                                throwableMessage);
                    }
                    else
                    {
                        failureDescription = string.Format("%s: failed with exception %s (%s). Root cause %s (%s)",
                                strategy.GetType().Name,
                                e.GetType().Name,
                                throwableMessage,
                                rootCause.GetType().Name,
                                rootCauseMessage
                        );
                    }
                    configurationFailures.Add(failureDescription);

                    _logger.LogDebug(failureDescription);
                }
            }
            _logger.LogError("Could not find a valid Docker environment. Please check configuration. Attempted configurations were:");
            foreach (var failureMessage in configurationFailures)
            {
                _logger.LogError("    " + failureMessage);
            }
            _logger.LogError("As no valid configuration was found, execution cannot continue");

            FAIL_FAST_ALWAYS = true;
            throw new IllegalStateException("Could not find a valid Docker environment. Please see logs and check configuration");
        }

        protected abstract IDockerClient GetDockerClient();

        //Assembly.Load("TestContainers")
        //    .GetTypes()
        //    .Where(p => p.GetTypeInfo().IsSubclassOf(typeof(DockerClientProviderStrategy)))
        //    .Select(type => (Activator.CreateInstance(type) as DockerClientProviderStrategy))
        //    .SingleOrDefault(strategy => strategy.IsApplicable());

        public IDockerClient GetClient() => Config.CreateClient();

        protected abstract int Priority { get; }
        protected abstract string Description { get;}
        public TransportConfig TransportConfig { get; private set; }
    }
}

