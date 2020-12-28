using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TestContainers.Containers;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;

namespace TestContainers
{
    public class ComposeServiceWaitStrategyTarget : AbstractWaitStrategyTarget
    {
        private readonly IContainer _container;
        private readonly GenericContainer _proxyContainer;
        private readonly Dictionary<int, int> _mappedPorts;

        public ComposeServiceWaitStrategyTarget(IContainer container, GenericContainer proxyContainer, Dictionary<int,int> mappedPorts)
        {
            if (mappedPorts is null)
            {
                throw new ArgumentNullException(nameof(mappedPorts));
            }

            _container = container ?? throw new ArgumentNullException(nameof(container));
            _proxyContainer = proxyContainer ?? throw new ArgumentNullException(nameof(proxyContainer));
            _mappedPorts = mappedPorts.ToDictionary(x => x.Key, x => x.Value);
        }

        public override string Host => _proxyContainer.Host;

        public override ContainerInspectResponse GetContainerInfo()
        {
            throw new NotImplementedException();
        }

        public override string ContainerId => _container.ContainerId;
    }
}
