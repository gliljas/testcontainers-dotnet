using TestContainers.Images;

namespace TestContainers.Utility
{
    internal class ConfigurationFileImageNameSubstitutor : ImageNameSubstitutor
    {
        protected override string Description => throw new System.NotImplementedException();

        public override DockerImageName Apply(DockerImageName original)
        {
            throw new System.NotImplementedException();
        }
    }
}
