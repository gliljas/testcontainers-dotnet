using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public class DockerHealthcheckWaitStrategy : IWaitStrategy
    {
        public Task WaitUntilReady(IWaitStrategyTarget waitStrategyTarget, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IWaitStrategy WithStartupTimeout(TimeSpan startupTimeout)
        {
            throw new NotImplementedException();
        }
    }
}
