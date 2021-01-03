using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestContainers.Containers
{
    internal interface IDockerCompose
    {
        IDockerCompose WithCommand(string cmd);

        IDockerCompose WithEnv(Dictionary<string, string> env);

        Task Invoke();
    }
}
