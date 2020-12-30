using System.Threading.Tasks;
using Docker.DotNet;
using TestContainers.Images;

namespace TestContainers.Utility
{
    public sealed class ResourceReaper
    {
        private ResourceReaper()
        {
        }

        public static async Task<string> Start(string hostIpAddress, IDockerClient dockerClient)
        {
            var ryukImage = ImageNameSubstitutor.Instance
            .Apply(DockerImageName.Parse("testcontainers/ryuk:0.3.0"))
            .AsCanonicalNameString();

            //??DockerClientFactory.Instance.CheckAndPullImage(client, ryukImage);


        }
    }
}
