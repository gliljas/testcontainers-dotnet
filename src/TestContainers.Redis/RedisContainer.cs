using System;
using System.Threading.Tasks;
using Polly;
using StackExchange.Redis;
using TestContainers.Core.Containers;
using TestContainers.Images;

namespace TestContainers.Redis
{
    public sealed class RedisContainer : DatabaseContainer
    {
        public const int Port = 6379;

        public RedisContainer() : base(DockerImageName.Parse("Redis"))
        {

        }
        public override string ConnectionString => $"{Host}:{GetMappedPort(Port)}";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var policyResult = await Policy
               .TimeoutAsync(TimeSpan.FromMinutes(2))
               .WrapAsync(Policy
                   .Handle<RedisConnectionException>()
                   .WaitAndRetryForeverAsync(
                       iteration => TimeSpan.FromSeconds(10),
                       (exception, timespan) => Console.WriteLine(exception.Message)))
               .ExecuteAndCaptureAsync(() =>
                   ConnectionMultiplexer.ConnectAsync(ConnectionString));

            if (policyResult.Outcome == OutcomeType.Failure)
                throw new Exception(policyResult.FinalException.Message);
        }
    }
}
