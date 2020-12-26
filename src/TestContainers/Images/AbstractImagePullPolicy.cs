using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers.Images
{
    public abstract class AbstractImagePullPolicy : IImagePullPolicy
    {
        public bool ShouldPull(DockerImageName dockerImageName)
        {
            throw new NotImplementedException();
        }
    }
}
