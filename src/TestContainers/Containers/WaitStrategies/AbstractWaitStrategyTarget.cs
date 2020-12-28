using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public abstract class AbstractWaitStrategyTarget : AbstractContainerState, IWaitStrategyTarget
    {
        public async Task<IReadOnlyList<int>> GetLivenessCheckPortNumbers(CancellationToken cancellationToken)
        {
            return (await Task.WhenAll((await GetExposedPorts(cancellationToken)).Select(async port => await GetMappedPort(port, cancellationToken)))).Distinct().ToList();
        }
    }
}
