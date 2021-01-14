using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TestContainers.Images
{
    public class AgeBasedPullPolicy : AbstractImagePullPolicy
    {
        private TimeSpan _maxAge;
        private ILogger _logger = StaticLoggerFactory.CreateLogger<AgeBasedPullPolicy>();

        public AgeBasedPullPolicy(TimeSpan maxAge)
        {
            _maxAge = maxAge;
        }

        public override Task<bool> ShouldPull(DockerImageName dockerImageName)
        {
            throw new NotImplementedException();
        }

        protected internal override bool ShouldPullCached(DockerImageName imageName, ImageData localImageData)
        {
            var imageAge = DateTimeOffset.Now-localImageData.CreatedAt;
            var result = imageAge > _maxAge;
            if (result)
            {
                _logger.LogTrace("Should pull image: {imageName}", imageName);
            }
            return result;
        }
    }
}
