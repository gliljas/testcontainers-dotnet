using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Core.Containers;
using TestContainers.Images;
using Xunit;

namespace TestContainers.Tests.Images
{
    public class ImagePullPolicyTest : IAsyncLifetime
    {
        
        [Fact]
        public async Task PullsByDefault()
        {
            await using (GenericContainer container = new GenericContainer(imageName)
                .WithStartupCheckStrategy(new OneShotStartupCheckStrategy()))
            {
                await container.Start();
            }
        }

        [Fact]
        public async Task ShouldAlwaysPull()
        {

            await using (GenericContainer container = new GenericContainer(imageName)
                .withStartupCheckStrategy(new OneShotStartupCheckStrategy()))
            {

                await container.Start();

                await RemoveImage();
            }
            await using (
                GenericContainer  container = new GenericContainer(imageName)
                    .withStartupCheckStrategy(new OneShotStartupCheckStrategy())

            ) {
                expectToFailWithNotFoundException(container);
            }

            await using (
                // built_in_image_pull_policy {
                GenericContainer  container = new GenericContainer(imageName)
                    .WithImagePullPolicy(PullPolicy.AlwaysPull)
                    // }
            ) {
                container.WithStartupCheckStrategy(new OneShotStartupCheckStrategy());
                await container.Start();
            }
            }

    [Fact]
            public void ShouldSupportCustomPolicies()
            {
                await using (
                    // custom_image_pull_policy {
                    GenericContainer container = new GenericContainer(imageName)
                        .WithImagePullPolicy(new AbstractImagePullPolicy() {
                    @Override
                                protected boolean shouldPullCached(DockerImageName imageName, ImageData localImageData)
                {
                    return System.getenv("ALWAYS_PULL_IMAGE") != null;
                }
                })
            // }
        ) {
                container.withStartupCheckStrategy(new OneShotStartupCheckStrategy());
                container.start();
            }
            }

        [Fact]
            public void shouldCheckPolicy()
            {
                ImagePullPolicy policy = Mockito.spy(new AbstractImagePullPolicy() {
            @Override
            protected boolean shouldPullCached(DockerImageName imageName, ImageData localImageData)
                {
                    return false;
                }
            });
            try (
                GenericContainer <?> container = new GenericContainer<>(imageName)
                    .withImagePullPolicy(policy)
                    .withStartupCheckStrategy(new OneShotStartupCheckStrategy())


            ) {
                container.start();

                Mockito.verify(policy).shouldPull(any());
            }
            }

[Fact]
            public void shouldNotForcePulling()
            {
                try (
                    GenericContainer <?> container = new GenericContainer<>(imageName)
                        .withImagePullPolicy(__-> false)
                        .withStartupCheckStrategy(new OneShotStartupCheckStrategy())



                ) {
                    expectToFailWithNotFoundException(container);
                }
                }

    private void expectToFailWithNotFoundException(GenericContainer<?> container)
                {
                    try
                    {
                        container.start();
                        fail("Should fail");
                    }
                    catch (ContainerLaunchException e)
                    {
                        Throwable throwable = e;
                        while (throwable.getCause() != null)
                        {
                            throwable = throwable.getCause();
                            if (throwable.getCause() instanceof NotFoundException) {
                VisibleAssertions.pass("Caused by NotFoundException");
                return;
            }
        }

        public async Task InitializeAsync()
        {
            // Clean up local cache
            await RemoveImage();

            LocalImagesCache.Instance.Cache.Remove(imageName);
        }

        public Task DisposeAsync()
        {
            throw new NotImplementedException();
        }

        


    private static async Task RemoveImage()
    {
        try
        {
            DockerClientFactory.Instance.Client()
                .Images.DeleteImageAsync(imageName.AsCanonicalNameString())
                .withForce(true)
                .exec();
        }
        catch (NotFoundException ignored)
        {
        }
    }
}
}
