namespace TestContainers
{
    public class TransportConfig
    {
        public object DockerHost { get; internal set; }
        internal SslConfig SslConfig { get; set; }
    }
}