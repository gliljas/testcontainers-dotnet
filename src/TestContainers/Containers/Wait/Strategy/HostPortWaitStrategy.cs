using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using TestContainers.Containers.Wait.Strategy;
using TestContainers.Core.Containers;

namespace TestContainers.Containers.WaitStrategies
{
    public class HostPortWaitStrategy : AbstractWaitStrategy
    {
        protected override async Task WaitUntilReady(CancellationToken cancellationToken = default)
        {
            var externalLivenessCheckPorts = await GetLivenessCheckPorts(cancellationToken);
            if (externalLivenessCheckPorts.Count > 0)
            {
                //if (log.isDebugEnabled())
                //{
                //    log.debug("Liveness check ports of {} is empty. Not waiting.", _waitStrategyTarget.GetContainerInfo().Name);
                //}
                return;
            }

            var exposedPorts = await _waitStrategyTarget.GetExposedPorts(cancellationToken);

            var internalPorts = GetInternalPorts(externalLivenessCheckPorts, exposedPorts);

            Func<Task<bool>> internalCheck = null;// new InternalCommandPortListeningCheck(_waitStrategyTarget, internalPorts);

            Func<Task<bool>> externalCheck = null;// new ExternalPortListeningCheck(_waitStrategyTarget, externalLivenessCheckPorts);

            try
            {
                //Unreliables.retryUntilTrue((int) startupTimeout.getSeconds(), TimeUnit.SECONDS,
                //    ()->getRateLimiter().getWhenReady(()->internalCheck.call() && externalCheck.call()));

                await Policy
                    .Handle<Exception>()
                    .OrResult<bool>(x => false)
                    .RetryForeverAsync()
                    .ExecuteAsync(async () =>
                        await RateLimiter.GetWhenReady(async () => await internalCheck() && await externalCheck())
                    );
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
        private IReadOnlyList<int> GetInternalPorts(IReadOnlyList<int> externalLivenessCheckPorts, IReadOnlyList<int> exposedPorts)
        {
            return null;// exposedPorts.Where(it => externalLivenessCheckPorts.Contains(_waitStrategyTarget.GetMappedPort(it))).;
        }

        public IRateLimiter RateLimiter { get; set; }
    }
}
