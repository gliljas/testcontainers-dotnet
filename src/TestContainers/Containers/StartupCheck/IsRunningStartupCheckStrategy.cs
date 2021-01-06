using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.StartupStrategies
{
    public class IsRunningStartupCheckStrategy : AbstractStartupCheckStrategy
    {
        public override async Task<StartupStatus> CheckStartupState(string containerId)
        {
            var state = await GetCurrentState(containerId);
            if (state.Running)
            {
                return StartupStatus.Successful;
            }
            else if (!state.IsContainerExitCodeSuccess())
            {
                return StartupStatus.Failed;
            }
            else
            {
                return StartupStatus.NotYetKnown;
            }
        }
    }
}
