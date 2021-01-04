using System.Threading.Tasks;

namespace TestContainers.Images
{
    public interface IImagePullPolicy
    {
        Task<bool> ShouldPull(DockerImageName dockerImageName);
    }
}
