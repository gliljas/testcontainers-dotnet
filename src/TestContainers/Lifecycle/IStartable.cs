using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestContainers.Lifecycle
{
    public interface IStartable
    {
        Task StartAsync();
        Task StopAsync();
    }
}
