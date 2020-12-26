using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.WaitStrategies
{
    public abstract class AbstractWaitStrategy : IWaitStrategy
    {
        private TimeSpan _startupTimeout;
        protected IWaitStrategyTarget _waitStrategyTarget;

        public Task WaitUntil(IDockerClient dockerClient, IContainer container, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task WaitUntilReady(IWaitStrategyTarget target, CancellationToken cancellationToken = default)
        {
            _waitStrategyTarget = target;
            return WaitUntilReady(cancellationToken);
        }

        protected abstract Task WaitUntilReady(CancellationToken cancellationToken);

        /**
     * Set the duration of waiting time until container treated as started.
     *
     * @param startupTimeout timeout
     * @return this
     * @see WaitStrategy#waitUntilReady(WaitStrategyTarget)
     */
        public IWaitStrategy WithStartupTimeout(TimeSpan startupTimeout)
        {
            _startupTimeout = startupTimeout;
            return this;
        }

        /**
    * @return the ports on which to check if the container is ready
    */
        protected Task<IReadOnlyList<int>> GetLivenessCheckPorts(CancellationToken cancellationToken = default)
        {
            return _waitStrategyTarget.GetLivenessCheckPortNumbers(cancellationToken);
        }
    }
}
