using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public interface IWaitStrategyTarget : IContainerState
    {
        Task<IReadOnlyList<int>> GetLivenessCheckPortNumbers(CancellationToken cancellationToken = default);
    }
}
