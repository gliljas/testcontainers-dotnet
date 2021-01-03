using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers.Images
{
    public abstract class AbstractImagePullPolicy : IImagePullPolicy
    {
        public abstract bool ShouldPull(DockerImageName dockerImageName);
    }
}
