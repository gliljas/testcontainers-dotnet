using TestContainers.Images;

namespace TestContainers.Utility
{
    internal class DefaultImageNameSubstitutor : ImageNameSubstitutor
    {
        private readonly ConfigurationFileImageNameSubstitutor _configurationFileImageNameSubstitutor;

        public DefaultImageNameSubstitutor()
        {
            _configurationFileImageNameSubstitutor = new ConfigurationFileImageNameSubstitutor();
        }

        internal DefaultImageNameSubstitutor(ConfigurationFileImageNameSubstitutor configurationFileImageNameSubstitutor)
        {
            _configurationFileImageNameSubstitutor = configurationFileImageNameSubstitutor ?? throw new System.ArgumentNullException(nameof(configurationFileImageNameSubstitutor));
        }

        protected override string Description => $"DefaultImageNameSubstitutor ({_configurationFileImageNameSubstitutor})";

        public override DockerImageName Apply(DockerImageName original) => _configurationFileImageNameSubstitutor.Apply(original);
    }
}
