using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace TestContainers.Containers.WaitStrategies
{
    public class LogMessageWaitStrategy : AbstractWaitStrategy
    {
        private int _times = 1;
        private Regex _regex; 

        protected override async Task WaitUntilReady(CancellationToken cancellationToken)
        {
            var stream = await DockerClientFactory.Instance.Client().Containers.GetContainerLogsAsync(_waitStrategyTarget.ContainerId, new ContainerLogsParameters { Follow = true, Since = "0", ShowStdout = true, ShowStderr = true }, cancellationToken);
        }

        public LogMessageWaitStrategy WithRegEx(string regexPattern)
        {
            _regex = new Regex(regexPattern);
            return this;
        }

        public LogMessageWaitStrategy WithTimes(int times)
        {
            _times = times > 0 ? times : throw new ArgumentOutOfRangeException("must be >= 1", nameof(times));
            return this;
        }
    }
}
