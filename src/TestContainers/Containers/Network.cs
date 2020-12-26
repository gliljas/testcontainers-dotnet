using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace TestContainers
{
    public class Network
    {
        private string _name = Guid.NewGuid().ToString("N");
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
