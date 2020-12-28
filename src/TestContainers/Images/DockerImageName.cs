using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers.Images
{
    public sealed class DockerImageName
    {
        public static DockerImageName Parse(string fullImageName) => new DockerImageName(fullImageName);

        private DockerImageName(string fullImageName)
        {
          
        }

    }

    public class RemoteDockerImage
    {
        private DockerImageName _dockerImageName;

        public RemoteDockerImage(DockerImageName dockerImageName)
        {
            _dockerImageName = dockerImageName;
        }
    }
}
