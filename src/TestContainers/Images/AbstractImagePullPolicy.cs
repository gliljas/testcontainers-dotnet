using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Utility;

namespace TestContainers.Images
{
    public abstract class AbstractImagePullPolicy : IImagePullPolicy
    {

        private static readonly LocalImagesCache LOCAL_IMAGES_CACHE = LocalImagesCache.Instance;
        
        public virtual async Task<bool> ShouldPull(DockerImageName imageName)
        {
            var logger = DockerLoggerFactory.GetLogger(imageName.AsCanonicalNameString());

            // Does our cache already know the image?
            ImageData cachedImageData = LOCAL_IMAGES_CACHE.Get(imageName);
            if (cachedImageData != null)
            {
                logger.LogTrace("{imageName} is already in image name cache", imageName);
            }
            else
            {
                logger.LogDebug("{imageName} is not in image name cache, updating...", imageName);
                // Was not in cache, inspecting...
                cachedImageData = await LOCAL_IMAGES_CACHE.RefreshCache(imageName);

                if (cachedImageData == null)
                {
                    logger.LogDebug("Not available locally, should pull image: {}", imageName);
                    return true;
                }
            }

            if (ShouldPullCached(imageName, cachedImageData))
            {
                logger.LogDebug("Should pull locally available image: {}", imageName);
                return true;
            }
            else
            {
                logger.LogDebug("Using locally available and not pulling image: {}", imageName);
                return false;
            }

        }
        /**
     * Implement this method to decide whether a locally available image should be pulled
     * (e.g. to always pull images, or to pull them after some duration of time)
     *
     * @return {@code true} to update the locally available image, {@code false} to use local instead
     */
        abstract protected bool ShouldPullCached(DockerImageName imageName, ImageData localImageData);
    }
}
