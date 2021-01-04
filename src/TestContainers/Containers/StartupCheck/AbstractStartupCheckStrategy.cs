using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Polly;

namespace TestContainers.Containers.StartupStrategies
{
    public abstract class AbstractStartupCheckStrategy : IStartupCheckStrategy
    {
        private Policy<StartupStatus> p = Policy
               .TimeoutAsync<StartupStatus>(TimeSpan.FromMinutes(2))
               .WrapAsync<StartupStatus>(Policy
                    .HandleResult<StartupStatus>(x => x == StartupStatus.NotYetKnown)
                    .WaitAndRetryForeverAsync(
                       iteration => TimeSpan.FromSeconds(1),
                       (exception, timespan) => Console.WriteLine("hjhj")));

        public async Task<bool> WaitUntilStartupSuccessful(string containerId)
        {
            return (await p.ExecuteAsync(async () => await CheckStartupState(containerId))) == StartupStatus.Successful;
        }

        public abstract Task<StartupStatus> CheckStartupState(string containerId);


        protected async Task<ContainerState> GetCurrentState(string containerId)
        {
            return (await DockerClientFactory.Instance.Execute(c=>c.Containers.InspectContainerAsync(containerId))).State;
        }
        public enum StartupStatus
        {
            NotYetKnown,
            Successful,
            Failed
        }
    }
}
