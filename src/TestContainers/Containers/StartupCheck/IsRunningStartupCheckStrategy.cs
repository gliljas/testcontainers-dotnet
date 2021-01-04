using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.StartupStrategies
{
    public class IsRunningStartupCheckStrategy : AbstractStartupCheckStrategy
    {
        public override Task<StartupStatus> CheckStartupState(string containerId)
        {
            throw new NotImplementedException();
        }
    }
}
