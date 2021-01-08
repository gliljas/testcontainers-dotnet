using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Core.Containers;
using TestContainers.Images;

namespace TestContainers.Utility
{
    public class PrefixingImageNameSubstitutor
    {
        internal static readonly string PREFIX_PROPERTY_KEY = "hub.image.name.prefix";
        private TestContainersConfiguration _configuration;
        private ILogger _logger = StaticLoggerFactory.CreateLogger<PrefixingImageNameSubstitutor>();

        public PrefixingImageNameSubstitutor(TestContainersConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<DockerImageName> Apply(DockerImageName original)
        {
            
            var configuredPrefix = _configuration.GetEnvVarOrProperty(PREFIX_PROPERTY_KEY, "");

            if (string.IsNullOrEmpty(configuredPrefix))
            {
                _logger.LogDebug("No prefix is configured");
                return Task.FromResult(original);
            }

            var isAHubImage = string.IsNullOrEmpty(original.Registry);
            if (!isAHubImage)
            {
                _logger.LogDebug("Image {original} is not a Docker Hub image - not applying registry/repository change", original);
                return Task.FromResult(original);
            }

            _logger.LogDebug(
                "Applying changes to image name {original}: applying prefix '{configuredPrefix}'",
                original,
                configuredPrefix
            );

            var prefixAsImage = DockerImageName.Parse(configuredPrefix);

            return Task.FromResult(original
                .WithRegistry(prefixAsImage.Registry)
                .WithRepository(prefixAsImage.Repository + original.Repository));
        }
    }
}
