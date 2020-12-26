using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.StartupStrategies
{
    public class MinimumDurationRunningStartupCheckStrategy : AbstractStartupCheckStrategy
    {
        private readonly TimeSpan _minimumRunningDuration;

        public MinimumDurationRunningStartupCheckStrategy(TimeSpan minimumRunningDuration)
        {
            _minimumRunningDuration = minimumRunningDuration;
        }

        public override async Task<StartupStatus> CheckStartupState(IDockerClient dockerClient, string containerId)
        {
            // record "now" before fetching status; otherwise the time to fetch the status
            // will contribute to how long the container has been running.
            var now = DateTimeOffset.Now;

            var state = await GetCurrentState(dockerClient, containerId);

            if (state.IsContainerRunning(_minimumRunningDuration, now))
            {
                return StartupStatus.Successful;
            }
            else if (state.IsContainerStopped())
            {
                return StartupStatus.Failed;
            }
            return StartupStatus.NotYetKnown;
        }
    }
}
