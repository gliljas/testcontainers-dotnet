using System;
using System.Threading.Tasks;
using Polly;
using RabbitMQ.Client;
using TestContainers.Core.Containers;

namespace TestContainers.RabbitMQ
{
    public class RabbitMQContainer : GenericContainer
    {
        public const string IMAGE = "rabbitmq:3.7-alpine";
        public const int Port = 5672;
        public const int DefaultRequestedHeartbeatInSec = 60;

        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public IConnection Connection { get; private set; }

        IConnectionFactory _connectionFactory;

        IConnectionFactory ConnectionFactory =>
            _connectionFactory ?? (_connectionFactory = new ConnectionFactory
            {
                HostName = GetDockerHostIpAddress(),
                Port = GetMappedPort(Port),
                VirtualHost = VirtualHost,
                UserName = UserName,
                Password = Password,
                RequestedHeartbeat = TimeSpan.FromSeconds(DefaultRequestedHeartbeatInSec)
            });

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<Exception>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(() =>
               {
                   Connection = ConnectionFactory.CreateConnection();
                   if (!Connection.IsOpen)
                   {
                       throw new Exception("Connection not open");
                   }

                   return Task.CompletedTask;
               });

            if (result.Outcome == OutcomeType.Failure)
            {
                Connection.Dispose();
                throw new Exception(result.FinalException.Message);
            }
        }
    }
}
