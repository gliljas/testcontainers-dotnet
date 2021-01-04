using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.StartupStrategies
{
    public class OneShotStartupCheckStrategy : AbstractStartupCheckStrategy
    {
        public override async Task<StartupStatus> CheckStartupState(string containerId)
        {
            var state = await GetCurrentState(containerId);

            if (!state.IsContainerStopped())
            {
                return StartupStatus.NotYetKnown;
            }

            if (state.IsContainerStopped() && state.IsContainerExitCodeSuccess())
            {
                return StartupStatus.Successful;
            }
            else
            {
                return StartupStatus.Failed;
            }
        }
    }
}
