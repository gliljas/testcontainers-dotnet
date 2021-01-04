using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using TestContainers.Core.Containers;
using TestContainers.Images;
using Xunit;

namespace TestContainers.Tests.Images
{
    public class DockerRegistryFixture : IAsyncLifetime
    {
        public static GenericContainer _registry = new GenericContainer(DOCKER_REGISTRY_IMAGE)
        .withExposedPorts(5000);
        private DockerImageName _imageName;

        public DockerImageName ImageName => _imageName;

        public Task DisposeAsync()
        {
            await RemoveImage();
        }

        public async Task InitializeAsync()
        {
            var testRegistryAddress = _registry.Host + ":" + await _registry.GetFirstMappedPort(default);
            var testImageName = testRegistryAddress + "/image-pull-policy-test";
            var tag = Guid.NewGuid().ToString("N");

            ImageName = DockerImageName.Parse(testImageName).WithTag(tag);

            string dummySourceImage = "hello-world:latest";
            await DockerClientFactory.Instance.Execute(c=>c.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = dummySourceImage }, new Progress<JSONMessage>()));//.exec(new PullImageResultCallback()).awaitCompletion();

            var dummyImageId = (await DockerClientFactory.Instance.Execute(c => c.Images.InspectImageAsync(dummySourceImage))).ID;

            // push the image to the registry
            await DockerClientFactory.Instance.Execute(c => c.Images.TagImageAsync(dummyImageId, new ImageTagParameters { Tag = tag }));

            DockerClientFactory.Instance.Execute(c => c.Images.PushImageAsync(_imageName.AsCanonicalNameString())
                .exec(new ResultCallback.Adapter<>())
                .awaitCompletion(1, TimeUnit.MINUTES));
        }

        public async Task RemoveImage()
        {
            try
            {
                await DockerClientFactory.Instance.Execute(c=>c
                    .Images.DeleteImageAsync(_imageName.AsCanonicalNameString(), new ImageDeleteParameters { Force = true }));
            }
            catch (DockerImageNotFoundException)
            {
            }
        }
    }
}
