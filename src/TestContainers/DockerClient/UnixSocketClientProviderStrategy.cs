using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers
{
    public class UnixSocketClientProviderStrategy : DockerClientProviderStrategy
    {
        protected static readonly string DOCKER_SOCK_PATH = "/var/run/docker.sock";
        private static readonly string SOCKET_LOCATION = "unix://" + DOCKER_SOCK_PATH;
        private static readonly int SOCKET_FILE_MODE_MASK = 0xc000;
        protected override DockerClientConfiguration Config { get; } =
            new DockerClientConfiguration(new Uri(SOCKET_LOCATION));

        protected override bool IsApplicable() => EnvironmentHelper.IsOSX() || EnvironmentHelper.IsLinux();

        protected override bool IsPersistable()
        {
            throw new NotImplementedException();
        }

        protected override string Description => "Docker for Linux/Mac (via socket)";

        protected override int Priority => throw new NotImplementedException();
    }
}
