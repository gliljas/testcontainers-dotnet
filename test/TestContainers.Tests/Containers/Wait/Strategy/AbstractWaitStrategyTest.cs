using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers.Wait.Strategy
{
    public abstract class AbstractWaitStrategyTest<W> where W : IWaitStrategy
    {
        private static readonly long WAIT_TIMEOUT_MILLIS = 3000;
        protected bool _ready;
        protected abstract W BuildWaitStrategy(ref bool ready);

        private GenericContainer StartContainerWithCommand(string shellCommand)

        {
            return StartContainerWithCommand(shellCommand, BuildWaitStrategy(ref _ready));
        }

        protected GenericContainer StartContainerWithCommand(string shellCommand, IWaitStrategy waitStrategy)
        {
            return StartContainerWithCommand(shellCommand, waitStrategy, 8080);
        }

        protected GenericContainer StartContainerWithCommand(string shellCommand, IWaitStrategy waitStrategy, params int[] ports)
        {
            // apply WaitStrategy to container
            var container = new ContainerBuilder<GenericContainer>(TestImages.ALPINE_IMAGE)
                    .WithExposedPorts(ports)
                    .WithCommand("sh", "-c", shellCommand)
                    .WaitingFor(waitStrategy.WithStartupTimeout(TimeSpan.FromMilliseconds(WAIT_TIMEOUT_MILLIS)))
                    .Build();


            return container;
        }

        /**
     * Expects that the WaitStrategy returns successfully after connection to a container with a listening port.
     *
     * @param shellCommand the shell command to execute
     */
        protected async Task WaitUntilReadyAndSucceed(String shellCommand)
        {
            await WaitUntilReadyAndSucceed(StartContainerWithCommand(shellCommand));
        }

        /**
         * Expects that the WaitStrategy throws a {@link RetryCountExceededException} after unsuccessful connection
         * to a container with a listening port.
         *
         * @param shellCommand the shell command to execute
         */
        protected async Task WaitUntilReadyAndTimeout(string shellCommand)
        {
            await WaitUntilReadyAndTimeout(StartContainerWithCommand(shellCommand));
        }

        /**
         * Expects that the WaitStrategy throws a {@link RetryCountExceededException} after unsuccessful connection
         * to a container with a listening port.
         *
         * @param container the container to start
         */
        protected async Task WaitUntilReadyAndTimeout(GenericContainer container)
        {
            // start() blocks until successful or timeout

            await Assert.ThrowsAsync<ContainerLaunchException>(() => container.Start());
            // VisibleAssertions.assertThrows("an exception is thrown when timeout occurs (" + WAIT_TIMEOUT_MILLIS + "ms)",
            //         ContainerLaunchException.class,
            //     container::start);

        }

        /**
         * Expects that the WaitStrategy returns successfully after connection to a container with a listening port.
         *
         * @param container the container to start
         */
        protected async Task WaitUntilReadyAndSucceed(GenericContainer container)
        {
            // start() blocks until successful or timeout
            await container.Start();

            _ready.Should().BeTrue();

            //assertTrue(String.format("Expected container to be ready after timeout of %sms",
            //    WAIT_TIMEOUT_MILLIS), ready.get());
        }
    }
}
