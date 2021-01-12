using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Output;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class OutputStreamTest : IAsyncLifetime
    {
        private GenericContainer _container;
        private static readonly ILogger LOGGER = StaticLoggerFactory.CreateLogger<OutputStreamTest>();
        public OutputStreamTest()
        {
            _container = new ContainerBuilder<GenericContainer>(TestImages.ALPINE_IMAGE)
            .WithCommand("ping -c 5 127.0.0.1")
            .Build();
        }



        [Fact(Timeout = 60000)]
        public async Task testFetchStdout()
        {

            var consumer = new WaitingConsumer();

            await _container.FollowOutput(consumer, OutputType.STDOUT);

            consumer.WaitUntil(frame => frame.OutputType == OutputType.STDOUT && frame.Utf8String.Contains("seq=2"),
                    TimeSpan.FromSeconds(30));
        }

        [Fact(Timeout = 60000)]
        public async Task testFetchStdoutWithTimeout()
        {

            var consumer = new WaitingConsumer();

            await _container.FollowOutput(consumer, OutputType.STDOUT);

            await Assert.ThrowsAsync<TimeoutException>(async () => await consumer.WaitUntil(frame => frame.OutputType == OutputType.STDOUT && frame.Utf8String.Contains("seq=5"), TimeSpan.FromSeconds(2)));

        }


        [Fact(Timeout = 60000)]
        public async Task testFetchStdoutWithNoLimit()
        {

            var consumer = new WaitingConsumer();

            await _container.FollowOutput(consumer, OutputType.STDOUT);

            await consumer.WaitUntil(frame => frame.OutputType == OutputType.STDOUT && frame.Utf8String.Contains("seq=2"));
        }

        [Fact(Timeout = 60000)]
        public async Task testLogConsumer()
        {

            var waitingConsumer = new WaitingConsumer();

            var logConsumer = new LoggerConsumer(LOGGER);

            var composedConsumer = logConsumer.AndThen(waitingConsumer);

            await _container.FollowOutput(composedConsumer);

            await waitingConsumer.WaitUntil(frame => frame.OutputType == OutputType.STDOUT && frame.Utf8String.Contains("seq=2"));
        }

        [Fact(Timeout = 60000)]
        public async Task testToStringConsumer()
        {

            var waitingConsumer = new WaitingConsumer();
            var toStringConsumer = new ToStringConsumer();

            var composedConsumer = toStringConsumer.AndThen(waitingConsumer);

            await _container.FollowOutput(composedConsumer);

            await waitingConsumer.WaitUntilEnd(TimeSpan.FromSeconds(30));

            var utf8String = toStringConsumer.ToUtf8String();
            utf8String.Should().Contain("seq-1", "the expected first value was found");
            utf8String.Should().Contain("seq-4", "the expected first value was found");
            utf8String.Should().NotContain("seq-42", "a non-expected value was found");
        }

        public async Task InitializeAsync()
        {
            await _container.Start();
        }

        public async Task DisposeAsync()
        {
            await _container.Stop();
        }
    }
}
