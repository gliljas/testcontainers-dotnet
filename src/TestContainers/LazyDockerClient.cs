//using System;
//using Docker.DotNet;

//namespace TestContainers
//{
//    internal class LazyDockerClient : IDockerClient
//    {
//        private static Lazy<LazyDockerClient> _instance = new Lazy<LazyDockerClient>();
//        private static Lazy<IDockerClient> _dockerClient = new Lazy<IDockerClient>(() => DockerClientFactory.Instance.Client());

//        public static LazyDockerClient Instance = _instance.Value;

//        public DockerClientConfiguration Configuration => _dockerClient.Value.Configuration;

//        public TimeSpan DefaultTimeout { get => _dockerClient.Value.DefaultTimeout; set => _dockerClient.Value.DefaultTimeout = value; }

//        public IContainerOperations Containers => _dockerClient.Value.Containers;

//        public IImageOperations Images => _dockerClient.Value.Images;

//        public INetworkOperations Networks => _dockerClient.Value.Networks;

//        public IVolumeOperations Volumes => _dockerClient.Value.Volumes;

//        public ISecretsOperations Secrets => _dockerClient.Value.Secrets;

//        public ISwarmOperations Swarm => _dockerClient.Value.Swarm;

//        public ITasksOperations Tasks => _dockerClient.Value.Tasks;

//        public ISystemOperations System => _dockerClient.Value.System;

//        public IPluginOperations Plugin => _dockerClient.Value.Plugin;

//        public void Dispose()
//        {
//        }
//    }
//}
