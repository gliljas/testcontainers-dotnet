using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace TestContainers
{
    public class Network
#if !NETSTANDARD2_0
        : IAsyncDisposable
#endif

    {
        private string _name = Guid.NewGuid().ToString("N");

        public string Id { get; internal set; }

        internal static Network NewNetwork()
        {
            throw new NotImplementedException();
        }

#if !NETSTANDARD2_0
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
#endif

        private async Task<string> Create(CancellationToken cancellationToken)
        {
            var parameters = new NetworksCreateParameters
            {
                Name = _name,
                CheckDuplicate = true
            };

            var response = await DockerClientFactory.Instance.Client().Networks.CreateNetworkAsync(parameters, cancellationToken);

            return response.ID;
        }
    }
}
