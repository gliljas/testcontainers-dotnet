using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestContainers.Images
{
    public class AgeBasedPullPolicy : AbstractImagePullPolicy
    {
        private TimeSpan _maxAge;

        public AgeBasedPullPolicy(TimeSpan maxAge)
        {
            _maxAge = maxAge;
        }

        public override Task<bool> ShouldPull(DockerImageName dockerImageName)
        {
            throw new NotImplementedException();
        }

        protected override bool ShouldPullCached(DockerImageName imageName, ImageData localImageData)
        {
            throw new NotImplementedException();
        }
    }
}
