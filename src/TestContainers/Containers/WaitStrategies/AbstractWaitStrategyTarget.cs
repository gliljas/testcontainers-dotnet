using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public abstract class AbstractWaitStrategyTarget : AbstractContainerState, IWaitStrategyTarget
    {
        public async Task<IReadOnlyList<int>> GetLivenessCheckPortNumbers()
        {
            return (await Task.WhenAll((await GetExposedPorts()).Select(async port => await GetMappedPort(port)))).Distinct().ToList();
        }
    }
}
