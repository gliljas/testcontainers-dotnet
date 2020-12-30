using System;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Polly;
using TestContainers.Core.Containers;
using TestContainers.Images;

namespace TestContainers.MySql
{
    public sealed class MySqlContainer : DatabaseContainer
    {
        public const string NAME = "mysql";
        public const string IMAGE = "mysql";
        public const int MYSQL_PORT = 3306;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "root";
        string _password = "Password123";

        public MySqlContainer() : base(DockerImageName.Parse(NAME))
        {

        }

        public override string ConnectionString => $"Server={Host};Port={GetMappedPort(MYSQL_PORT)};UID={UserName};pwd={Password};SslMode=none;";

        protected override string TestQueryString => "SELECT 1";

        protected override async Task WaitUntilContainerStarted(CancellationToken cancellationToken)
        {
            await base.WaitUntilContainerStarted(cancellationToken);

            var connection = new MySqlConnection(ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<MySqlException>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = new MySqlCommand(TestQueryString, connection);
                    var reader = await cmd.ExecuteScalarAsync();
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                connection.Dispose();
                throw new Exception(result.FinalException.Message);
            }
        }
    }
}
