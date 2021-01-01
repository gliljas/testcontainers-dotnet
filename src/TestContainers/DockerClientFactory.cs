using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers
{
    public sealed class DockerClientFactory
    {
        static volatile DockerClientFactory instance;
        static object syncRoot = new Object();
        internal IDockerClient _dockerClient;
        internal object _cachedClientFailure;
        public static readonly string TESTCONTAINERS_SESSION_ID_LABEL;
        public static readonly string TESTCONTAINERS_LABEL;
        public static readonly string SESSION_ID = Guid.NewGuid().ToString("N");

        DockerClientProviderStrategy strategy { get; } = DockerClientProviderStrategy.GetFirstValidStrategy();
        public static DockerClientFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DockerClientFactory();
                    }
                }
                return instance;
            }
        }

        public static IDockerClient LazyClient => LazyDockerClient.Instance;

        public IDockerClient Client() => strategy.GetClient();
    }
}
