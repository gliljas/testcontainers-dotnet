using System;

namespace TestContainers.Images
{
    public sealed class DockerImageName
    {
        public string UnversionedPart { get; internal set; }
        public string VersionPart { get; internal set; }

        public static DockerImageName Parse(string fullImageName) => new DockerImageName(fullImageName);

        private DockerImageName(string fullImageName)
        {
          
        }

        public string AsCanonicalNameString()
        {
            throw new NotImplementedException();
        }
    }
}
