using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                if (log.isDebugEnabled())
                {
                    log.debug("Liveness check ports of {} is empty. Not waiting.", _waitStrategyTarget.GetContainerInfo().Name);
                }
                return;
            }

            var exposedPorts = await _waitStrategyTarget.GetExposedPorts();

            var internalPorts = GetInternalPorts(externalLivenessCheckPorts, exposedPorts);

            Func<bool> internalCheck = new InternalCommandPortListeningCheck(_waitStrategyTarget, internalPorts);

            Func<bool> externalCheck = new ExternalPortListeningCheck(_waitStrategyTarget, externalLivenessCheckPorts);

            try
            {
                Unreliables.retryUntilTrue((int) startupTimeout.getSeconds(), TimeUnit.SECONDS,
                    ()->getRateLimiter().getWhenReady(()->internalCheck.call() && externalCheck.call()));

            }
            catch (TimeoutException e)
            {
                throw new ContainerLaunchException("Timed out waiting for container port to open (" +
                        (await _waitStrategyTarget.GetHost()) +
                        " ports: " +
                        externalLivenessCheckPorts +
                        " should be listening)");
            }
        }
        private ISet<int> GetInternalPorts(ISet<int> externalLivenessCheckPorts, List<int> exposedPorts)
        {
            return new HashSet<int>(exposedPorts.Where(it => externalLivenessCheckPorts.Contains(_waitStrategyTarget.GetMappedPort(it))));
        }
    }
}
