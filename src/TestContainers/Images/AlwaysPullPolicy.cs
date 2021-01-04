using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestContainers.Images
{
    public class AlwaysPullPolicy : IImagePullPolicy
    {
        public Task<bool> ShouldPull(DockerImageName dockerImageName) => Task.FromResult(true);
    }

  
}
