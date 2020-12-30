using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.WaitStrategies
{
    /// <summary>
    /// Strategy for waiting for services in the container to start
    /// </summary>
    public interface IWaitStrategy
    {
        Task WaitUntilReady(IWaitStrategyTarget waitStrategyTarget, CancellationToken cancellationToken);

        IWaitStrategy WithStartupTimeout(TimeSpan startupTimeout);
    }
}
