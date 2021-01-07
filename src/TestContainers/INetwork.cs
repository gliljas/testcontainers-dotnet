using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestContainers
{
    public interface INetwork
    {
        string Name { get; }

        Task<string> GetId();
    }
}
