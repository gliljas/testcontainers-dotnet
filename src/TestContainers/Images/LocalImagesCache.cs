using System;

namespace TestContainers.Images
{
    internal class LocalImagesCache
    {
        public static LocalImagesCache Instance { get; internal set; }

        internal void RefreshCache(DockerImageName imageName)
        {
            throw new NotImplementedException();
        }
    }
}
