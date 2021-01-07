using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using TestContainers.Containers.Wait.Strategy;

namespace TestContainers.Containers.WaitStrategies
{
    public class HostPortWaitStrategy : AbstractWaitStrategy
    {
        private ILogger _logger = StaticLoggerFactory.CreateLogger<HostPortWaitStrategy>();
        protected override async Task WaitUntilReady(CancellationToken cancellationToken = default)
        {
            var externalLivenessCheckPorts = await GetLivenessCheckPorts(cancellationToken);
            if (!externalLivenessCheckPorts.Any())
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Liveness check ports of {container} is empty. Not waiting.", _waitStrategyTarget.ContainerInfo.Name);
                }
                return;
            }

            var exposedPorts = await _waitStrategyTarget.GetExposedPorts(cancellationToken);

            var internalPorts = await GetInternalPorts(externalLivenessCheckPorts, exposedPorts);

            var internalCheck = new InternalCommandPortListeningCheck(_waitStrategyTarget, ImmutableHashSet.Create(internalPorts.ToArray()));

            var externalCheck = new ExternalPortListeningCheck(_waitStrategyTarget, externalLivenessCheckPorts);

            try
            {
                //Unreliables.retryUntilTrue((int) startupTimeout.getSeconds(), TimeUnit.SECONDS,
                //    ()->getRateLimiter().getWhenReady(()->internalCheck.call() && externalCheck.call()));

                var p = Policy.TimeoutAsync(_startupTimeout).WrapAsync(
                 Policy
                    .Handle<Exception>()
                    .OrResult<bool>(x => false)
                    .RetryForeverAsync());

                    await p.ExecuteAsync(async () =>

                    //RateLimiter.GetWhenReady(async () => 
                        await internalCheck.Invoke() &&  externalCheck.Invoke()
                    ); ;
            }
            catch (TimeoutException)
            {
                throw new ContainerLaunchException("Timed out waiting for container port to open (" +
                        _waitStrategyTarget.Host +
                        " ports: " +
                        externalLivenessCheckPorts +
                        " should be listening)");
            }
        }
        private async Task<IReadOnlyList<int>> GetInternalPorts(IReadOnlyList<int> externalLivenessCheckPorts, IReadOnlyList<int> exposedPorts)
        {
            var ports = new List<int>();
            foreach (var exposedPort in exposedPorts)
            {
                if (externalLivenessCheckPorts.Contains(await _waitStrategyTarget.GetMappedPort(exposedPort, default)))
                {
                    ports.Add(exposedPort);
                }
            }
            return ports;
        }

        public IRateLimiter RateLimiter { get; set; }
    }
}
