using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using TestContainers.Containers;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers
{
    class GenericContainerTest
    {
        [Fact]
        public async Task ShouldReportOOMAfterWait()
        {
            var info = await DockerClientFactory.Instance.Client().System.GetSystemInfoAsync();// ;/.infoCmd().exec();
            // Poor man's rootless Docker detection :D
            Assert.DoesNotContain("vfs", info.Driver);
            //Assumptions.assumeThat(info.getDriver()).doesNotContain("vfs");
            await using (
                GenericContainer container = new GenericContainer(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new NoopStartupCheckStrategy())
                    .WaitingFor(new WaitForExitedState(ContainerState::getOOMKilled))
                    .WithCreateContainerCmdModifier(it-> {
                it.getHostConfig()
                    .withMemory(20 * FileUtils.ONE_MB)
                    .withMemorySwappiness(0L)
                    .withMemorySwap(0L)
                    .withMemoryReservation(0L)
                    .withKernelMemory(16 * FileUtils.ONE_MB);
            })
                .WithCommand("sh", "-c", "A='0123456789'; for i in $(seq 0 32); do A=$A$A; done; sleep 10m")
            ) {
                var ex = await Assert.ThrowsAsync(() => container.Start());
                Assert.Contains("Container crashed with out-of-memory", ex.StackTrace);
            }
        }

        [Fact]
        public async Task ShouldReportErrorAfterWait()
        {
            await using (
                var container = new GenericContainer(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new NoopStartupCheckStrategy())
                    .WaitingFor(new WaitForExitedState(state->state.getExitCode() > 0))
                    .WithCommand("sh", "-c", "usleep 100; exit 123")

            )
            {
                var ex = await Assert.ThrowsAsync(() => container.Start());
                Assert.Contains("Container exited with code 123", ex.StackTrace);
            }
        }

        private class NoopStartupCheckStrategy : AbstractStartupCheckStrategy
        {
            public override Task<StartupStatus> CheckStartupState(IDockerClient dockerClient, string containerId)
            {
                return Task.FromResult(StartupStatus.Successful);
            }
        }

        private class WaitForExitedState : AbstractWaitStrategy
        {

            Predicate<IContainerState> _predicate;

            protected override void WaitUntilReady()
            {
                Unreliables.retryUntilTrue(5, TimeUnit.SECONDS, ()-> {
                    ContainerState state = waitStrategyTarget.getCurrentContainerInfo().getState();

                    _logger.Debug("Current state: {}", state);
                    if (!"exited".equalsIgnoreCase(state.getStatus()))
                    {
                        Thread.sleep(100);
                        return false;
                    }
                    return predicate.test(state);
                });

                throw new IllegalStateException("Nope!");
            }
        }
    }
}
