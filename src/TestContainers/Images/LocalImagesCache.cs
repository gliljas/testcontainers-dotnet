using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace TestContainers.Images
{
    internal class LocalImagesCache
    {
        private ILogger _logger;
        private bool _initialized;

        public static LocalImagesCache Instance { get; internal set; } = new LocalImagesCache();
        public ConcurrentDictionary<DockerImageName, ImageData> Cache { get; internal set; } = new ConcurrentDictionary<DockerImageName, ImageData>();

        internal async Task<ImageData> RefreshCache(DockerImageName imageName)
        {
            if (!await MaybeInitCache())
            {
                // Cache may be stale, trying inspectImageCmd...

                ImageInspectResponse response = null;
                try
                {
                    response = await DockerClientFactory.Instance.Execute(c => c.Images.InspectImageAsync(imageName.AsCanonicalNameString()));
                }
                catch (DockerApiException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogTrace(e, "Image {imageName} not found", imageName);
                }
                if (response != null)
                {
                    ImageData imageData = ImageData.From(response);
                    Cache[imageName] = imageData;
                    return imageData;
                }
                else
                {
                    Cache.TryRemove(imageName, out _);
                    return null;
                }
            }

            return Cache.TryGetValue(imageName, out var i) ? i : null;
        }

        private async Task<bool> MaybeInitCache()
        {
            //if (!Interlocked.CompareExchange(ref _initialized, true, false))
            //{
            //    return false;
            //}

            //if (Boolean.parseBoolean(System.getProperty("useFilter")))
            //{
            //    return false;
            //}

            PopulateFromList(await DockerClientFactory.Instance.Execute(c => c.Images.ListImagesAsync(new ImagesListParameters())));

            return true;
        }

        private void PopulateFromList(IList<ImagesListResponse> images)
        {
            foreach (var image in images)
            {
                var repoTags = image.RepoTags;
                if (repoTags == null)
                {
                    _logger.LogDebug("repoTags is null, skipping image: {image}", image);
                    continue;
                }

                // Protection against some edge case where local image repository tags end up with duplicates
                // making toMap crash at merge time.

                foreach (var entry in repoTags.Distinct().Select(x => (DockerImageName.Parse(x), ImageData.From(image))))
                {
                    Cache[entry.Item1] = entry.Item2;
                }

            }
        }

        internal ImageData Get(DockerImageName imageName)
        {
            return Cache.TryGetValue(imageName, out var value) ? value : null;
        }
    }
}
