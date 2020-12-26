using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers.Images
{
    public class AlwaysPullPolicy : IImagePullPolicy
    {
        public bool ShouldPull(DockerImageName dockerImageName) => true;
    }

  
}
