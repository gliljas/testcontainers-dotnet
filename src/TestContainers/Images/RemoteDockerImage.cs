using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace TestContainers.Images
{
    public class RemoteDockerImage 
    {
        private readonly TimeSpan PULL_RETRY_TIME_LIMIT = TimeSpan.FromMinutes(2);
        private DockerImageName _dockerImageName;
        private Task<DockerImageName> _dockerImageNameTask;
        private IImagePullPolicy _imagePullPolicy;
        private IDockerClient _dockerClient = DockerClientFactory.LazyClient;
        private ILogger _logger;
        
        public RemoteDockerImage(DockerImageName dockerImageName) 
        {
            _dockerImageNameTask = Task.FromResult(dockerImageName);
        }

        public RemoteDockerImage(string dockerImageName) : this(DockerImageName.Parse(dockerImageName))
        {
        }

        public async Task<string> Resolve(CancellationToken cancellationToken)
        {
            var imageName = await GetImageName(cancellationToken);
            //Logger logger = DockerLoggerFactory.getLogger(imageName.toString());
            try
            {
                if (!_imagePullPolicy.ShouldPull(imageName))
                {
                    return imageName.AsCanonicalNameString();
                }

                // The image is not available locally - pull it
                _logger.LogInformation("Pulling docker image: {imageName}. Please be patient; this may take some time but only needs to be done once.", imageName);

                Exception lastFailure = null;
                var lastRetryAllowed = DateTimeOffset.Now + PULL_RETRY_TIME_LIMIT;

                while (DateTimeOffset.Now < lastRetryAllowed)
                {
                    try
                    {
                        //
                        await _dockerClient.Images.CreateImageAsync(
                                new ImagesCreateParameters {FromImage = imageName.UnversionedPart,Tag = imageName.VersionPart },
                                null,
                                null,
                                cancellationToken
                            );
                            
                        LocalImagesCache.Instance.RefreshCache(imageName);

                        return imageName.AsCanonicalNameString();
                    }
                    catch (Exception e)  //(InterruptedException | InternalServerErrorException e) {
                    {    // these classes of exception often relate to timeout/connection errors so should be retried
                        lastFailure = e;
                        _logger.LogWarning("Retrying pull for image: {imageName} ({seconds}s remaining)",
                            imageName,
                            (lastRetryAllowed - DateTimeOffset.Now).TotalSeconds);
                    }
                }

                _logger.LogError(lastFailure, "Failed to pull image: {imageName}. Please check output of `docker pull {imageName}`", imageName, imageName);

                    throw new ContainerFetchException("Failed to pull image: " + imageName, lastFailure);
            }
            catch (Exception e)//(DockerClientException e)
            {
                throw new ContainerFetchException("Failed to get Docker client for " + imageName, e);
            }
        }

        private async Task<DockerImageName> GetImageName(CancellationToken cancellationToken)
        {
            var specifiedImageName = await _dockerImageNameTask;

            // Allow the image name to be substituted
            return specifiedImageName;//ImageNameSubstitutor.instance().apply(specifiedImageName);
        }
    }
}
