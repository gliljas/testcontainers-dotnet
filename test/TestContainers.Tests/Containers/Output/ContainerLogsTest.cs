using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers.Output
{
    public class ContainerLogsTest
    {
        [Fact]
        //@Ignore("fails due to the timing of the shell's decision to flush")
        public async Task GetLogsReturnsAllLogsToDate()
        {
            await using (var container = ShortLivedContainer())
            {
                await container.Start();

                var logs = await container.GetLogs();
                Assert.Equal("stdout\nstderr", logs); // "stdout and stderr are reflected in the returned logs"
            }
        }

        [Fact]
        public async Task GetLogsContainsBothOutputTypes()
        {
            await using (var container = ShortLivedContainer())
            {
                await container.Start();

                // docsGetAllLogs {
                var logs = await container.GetLogs();
                // }
                Assert.Contains("stdout", logs); //"stdout is reflected in the returned logs"
                Assert.Contains("stderr", logs); // "stderr is reflected in the returned logs"
            }
        }

        [Fact]
        public async Task GetLogsReturnsStdOutToDate()
        {
            await using (var container = ShortLivedContainer())
            {
                await container.Start();

                // docsGetStdOut {
                var logs = await container.GetLogs(OutputType.STDOUT);
                // }
                Assert.Contains("stdout", logs); //"stdout is reflected in the returned logs"
                Assert.DoesNotContain("stderr", logs); //"stderr is not reflected in the returned logs"
            }
        }

        [Fact]
        public async Task GetLogsReturnsStdErrToDate()
        {
            await using (var container = ShortLivedContainer())
            {
                await container.Start();

                // docsGetStdOut {
                var logs = await container.GetLogs(OutputType.STDERR);
                // }
                Assert.Contains("stderr", logs); //"stderr is reflected in the returned logs"
                Assert.DoesNotContain("stdout", logs); //"stdout is not reflected in the returned logs"
            }
        }

        [Fact]
        public async Task GetLogsForLongRunningContainer()
        {
            await using (var container = LongRunningContainer())
            {
                await container.Start();
                await Task.Delay(1000);
                // docsGetStdOut {
                var logs = await container.GetLogs(OutputType.STDOUT);
                // }
                Assert.Contains("seq=0", logs); //"stdout is reflected in the returned logs for a running container"
            }
        }

        private static GenericContainer ShortLivedContainer()
        {
            return new ContainerBuilder<GenericContainer>(TestImages.ALPINE_IMAGE)
                .WithCommand("/bin/sh", "-c", "echo -n 'stdout' && echo -n 'stderr' 1>&2")
                .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                .Build();
        }

        private static GenericContainer LongRunningContainer()
        {
            return new ContainerBuilder<GenericContainer>(TestImages.ALPINE_IMAGE)
               .WithCommand("ping -c 100 127.0.0.1")
               .Build();
        }
    }
}
