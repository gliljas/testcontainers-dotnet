using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Core.Containers;
using TestContainers.Images;

namespace TestContainers.Utility
{
    internal class ConfigurationFileImageNameSubstitutor : ImageNameSubstitutor
    {
        private TestContainersConfiguration _configuration;
        private ILogger _logger = StaticLoggerFactory.CreateLogger<ConfigurationFileImageNameSubstitutor>();
        public ConfigurationFileImageNameSubstitutor() : this(TestContainersConfiguration.Instance)
        {
        }
        public ConfigurationFileImageNameSubstitutor(TestContainersConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override string Description => throw new System.NotImplementedException();

        public override Task<DockerImageName> Apply(DockerImageName original)
        {
            var result = _configuration
            .GetConfiguredSubstituteImage(original)
            .AsCompatibleSubstituteFor(original);

            if (!result.Equals(original))
            {
                _logger.LogWarning("Image name {} was substituted by configuration to {}. This approach is deprecated and will be removed in the future",
                    original,
                    result
                );
            }

            return Task.FromResult(result);
        }
    }
}
