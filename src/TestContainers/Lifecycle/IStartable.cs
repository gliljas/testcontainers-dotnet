using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Lifecycle
{
    public interface IStartable
    {
        Task Start(CancellationToken cancellationToken = default);
        Task Stop();
    }
}
