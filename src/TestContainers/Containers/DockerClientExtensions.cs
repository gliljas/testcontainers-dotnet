using Docker.DotNet;

namespace TestContainers.Core.Containers
{
    public static class DockerClientExtensions
    {
        public static string GetDockerHostIpAddress(this IDockerClient dockerClient)
        {
            var dockerHostUri = dockerClient.Configuration.EndpointBaseUri;

            switch (dockerHostUri.Scheme)
            {
                case "http":
                case "https":
                case "tcp":
                    return dockerHostUri.Host;
                case "npipe": //will have to revisit this for LCOW/WCOW
                              //case "unix":
                              //    return File.Exists("/.dockerenv")
                              //        ? ContainerInspectResponse.NetworkSettings.Gateway
                              //        : "localhost";
                default:
                    return null;
            }
        }
    }
}
