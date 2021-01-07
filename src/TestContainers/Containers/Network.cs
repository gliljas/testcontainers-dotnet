using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TestContainers.Utility;

namespace TestContainers
{
    public class Network : INetwork
#if !NETSTANDARD2_0
        , IAsyncDisposable
#endif
    {
        private string _name = Guid.NewGuid().ToString("N");

        //public string Id { get; internal set; }
        public static NetworkBuilder Builder => new NetworkBuilder();
        public string Driver { get; internal set; }
        public List<Action<NetworksCreateParameters>> NetworksCreateParametersModifiers { get; internal set; }
        public string Name { get => _name; }

        private Lazy<Task<string>> _lazyId;

        internal static Network NewNetwork()
        {
            return Builder.Build();
        }

        internal Network()
        {
            _lazyId = new Lazy<Task<string>>(() => Create(default));
        }

#if !NETSTANDARD2_0
        public async ValueTask DisposeAsync()
        {
            await Close();
        }
#endif

        public Task<string> GetId() => _lazyId.Value;

        private async Task<string> Create(CancellationToken cancellationToken)
        {
            var parameters = new NetworksCreateParameters
            {
                Name = _name,
                CheckDuplicate = true
            };

            if (Driver != null)
            {
                parameters.Driver = Driver;
            }

            foreach (var modifier in NetworksCreateParametersModifiers)
            {
                modifier(parameters);
            }

            var response = await DockerClientFactory.Instance.Execute(c=>c.Networks.CreateNetworkAsync(parameters, cancellationToken));

            return response.ID;
        }

        internal async Task Close()
        {
            if (_lazyId.IsValueCreated)
            {
                //await ResourceReaper.Instance.RemoveNetworkById();
            }
        }
    }
}
