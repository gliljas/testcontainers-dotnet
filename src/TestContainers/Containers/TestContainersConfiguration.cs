namespace TestContainers.Core.Containers
{
    internal class TestContainersConfiguration
    {
        public static TestContainersConfiguration Instance { get; internal set; }
        public bool EnvironmentSupportsReuse { get; internal set; }
        public string ImageSubstitutorClassName { get; internal set; }
    }
}
