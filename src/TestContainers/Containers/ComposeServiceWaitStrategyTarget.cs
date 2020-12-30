using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;

namespace TestContainers.Containers
{
    public class ComposeServiceWaitStrategyTarget : AbstractWaitStrategyTarget
    {
        private readonly IContainer _container;
        private readonly GenericContainer _proxyContainer;
        private readonly Dictionary<int, int> _mappedPorts;

        public ComposeServiceWaitStrategyTarget(IContainer container, GenericContainer proxyContainer, Dictionary<int,int> mappedPorts)
        {
            _container = container;
            _proxyContainer = proxyContainer;
            _mappedPorts = mappedPorts.ToDictionary(x => x.Key, x => x.Value);
        }

        public override Task<IReadOnlyList<int>> GetExposedPorts(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<int>>(_mappedPorts.Keys.ToList());
        }

        public override Task<int> GetMappedPort(int originalPort, CancellationToken cancellationToken = default)
        {
            return _proxyContainer.GetMappedPort(_mappedPorts[originalPort], cancellationToken);
        }
        public override string Host => _proxyContainer.Host;

        public override string ContainerId => _container.ContainerId;
    }
}
