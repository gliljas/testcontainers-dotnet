using System;
using Docker.DotNet;
using TestContainers.DockerClient;

namespace TestContainers
{
    public class NpipeSocketClientProviderStrategy : DockerClientProviderStrategy
    {
        protected static readonly string DOCKER_SOCK_PATH = "//./pipe/docker_engine";
        private static readonly string SOCKET_LOCATION = "npipe:" + DOCKER_SOCK_PATH;

        public static readonly int PRIORITY = EnvironmentAndSystemPropertyClientProviderStrategy.PRIORITY - 20;
        protected override DockerClientConfiguration Config { get; } =
            new DockerClientConfiguration(new Uri(SOCKET_LOCATION)) { NamedPipeConnectTimeout = TimeSpan.FromSeconds(1) };

        protected override int Priority=>PRIORITY;

        protected override bool IsApplicable() => EnvironmentHelper.IsWindows();

        protected override string Description => "Docker for Windows (via named pipes)";

    }
}
