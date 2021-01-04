using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Containers
{
    internal interface IDockerCompose
    {
        Task Invoke(CancellationToken cancellationToken);
    }
}
