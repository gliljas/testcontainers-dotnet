using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Polly;
using TestContainers.Containers;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers
{
    public class GenericContainerTest
    {
        [Fact]
        public async Task ShouldReportOOMAfterWait()
        {
            var info = await DockerClientFactory.Instance.Execute(c => c.System.GetSystemInfoAsync());// ;/.infoCmd().exec();
            // Poor man's rootless Docker detection :D
            Assert.DoesNotContain("vfs", info.Driver);
            //Assumptions.assumeThat(info.getDriver()).doesNotContain("vfs");
            await using (
                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new NoopStartupCheckStrategy())
                    .WaitingFor(new WaitForExitedState(state => state.OOMKilled))
                    .WithCreateContainerCmdModifier(it=> {
                        it.HostConfig.Memory = 20 * 1024 * 1024;// FileUtils.ONE_MB;
                        it.HostConfig.MemorySwappiness = 0L;
                        it.HostConfig.MemorySwap = 0L;
                        it.HostConfig.MemoryReservation = 0L;
                        it.HostConfig.KernelMemory = 16 * 1024 * 1024;// FileUtils.ONE_MB;
                    })
                    .WithCommand("sh", "-c", "A='0123456789'; for i in $(seq 0 32); do A=$A$A; done; sleep 10m")
                    .Build()
            ) {
                var ex = await Assert.ThrowsAnyAsync<Exception>(() => container.Start());
                Assert.Contains("Container crashed with out-of-memory", ex.ToString());
            }
        }

        [Fact]
        public async Task ShouldReportErrorAfterWait()
        {
            await using (
                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new NoopStartupCheckStrategy())
                    .WaitingFor(new WaitForExitedState(state => state.ExitCode > 0))
                    .WithCommand("sh", "-c", "usleep 100; exit 123")
                    .Build()

            )
            {
                var ex = await Assert.ThrowsAnyAsync<Exception>(async () => await container.Start());
                Assert.Contains("Container exited with code 123", ex.ToString());
            }
        }

        private class NoopStartupCheckStrategy : AbstractStartupCheckStrategy
        {
            public override Task<StartupStatus> CheckStartupState(string containerId)
            {
                return Task.FromResult(StartupStatus.Successful);
            }
        }

        private class WaitForExitedState : AbstractWaitStrategy
        {
            private ILogger _logger;
            public WaitForExitedState(Predicate<ContainerState> predicate)
            {
                _predicate = predicate;
            }

            Predicate<ContainerState> _predicate;

            public override async Task WaitUntilReady(IWaitStrategyTarget target, CancellationToken cancellationToken = default)
            {
                var p = Policy.TimeoutAsync<bool>(5).WrapAsync<bool>(Policy<bool>.Handle<Exception>().OrResult(x => false).RetryForeverAsync());
                await p.ExecuteAsync(async () =>
                {
                    var state = (await target.GetCurrentContainerInfo(cancellationToken)).State;

                    _logger.LogDebug("Current state: {state}", state);
                    if (!"exited".Equals(state.Status, StringComparison.OrdinalIgnoreCase))
                    {
                        await Task.Delay(100);
                        return false;
                    }
                    return _predicate(state);
                });

                throw new IllegalStateException("Nope!");
            }

            protected override Task WaitUntilReady(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
