using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.StartupStrategies
{
    public interface IStartupCheckStrategy
    {
        Task<bool> WaitUntilStartupSuccessful(IDockerClient dockerClient, string containerId);

    }
}