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
        private IImagePullPolicy _imagePullPolicy = PullPolicy.DefaultPolicy();
        private ILogger _logger = StaticLoggerFactory.CreateLogger<RemoteDockerImage>();

        public RemoteDockerImage(DockerImageName dockerImageName)
        {
            _dockerImageNameTask = Task.FromResult(dockerImageName);
        }

        public RemoteDockerImage(Task<string> dockerImageNameTask)
        {
            _dockerImageNameTask = dockerImageNameTask.ContinueWith(x => DockerImageName.Parse(x.Result), TaskContinuationOptions.ExecuteSynchronously);
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
                if (!await _imagePullPolicy.ShouldPull(imageName))
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
                        await DockerClientFactory.Instance.Execute(c=>c.Images.CreateImageAsync(
                                new ImagesCreateParameters { FromImage = imageName.UnversionedPart, Tag = imageName.VersionPart },
                                new AuthConfig(),
                                new Progress<JSONMessage>(m=>Console.WriteLine(m.ProgressMessage)),
                                cancellationToken
                            ));

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

        private string ImageNameToString()
        {
            if (!_dockerImageNameTask.IsCompleted)
            {
                return "<resolving>";
            }

            try
            {
                return _dockerImageNameTask.Result.AsCanonicalNameString();
            }
            catch (AggregateException e)
            { //InterruptedException | ExecutionException 
                return e.Flatten().Message;
            }
            catch (Exception e)
            { //InterruptedException | ExecutionException 
                return e.Message;
            }
        }

        public override string ToString()
        {
            return "imageName=" + ImageNameToString();
        }
    }
}
