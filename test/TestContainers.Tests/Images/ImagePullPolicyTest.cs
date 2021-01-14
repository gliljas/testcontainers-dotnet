using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using NSubstitute;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Core.Containers;
using TestContainers.Images;
using Xunit;
using Xunit.Sdk;

namespace TestContainers.Tests.Images
{
    public class ImagePullPolicyTest : IAsyncLifetime, IClassFixture<DockerRegistryFixture>
    {
        private readonly DockerRegistryFixture _dockerRegistryFixture;

        public ImagePullPolicyTest(DockerRegistryFixture dockerRegistryFixture)
        {
            _dockerRegistryFixture = dockerRegistryFixture;
        }
        [Fact]
        public async Task PullsByDefault()
        {
            await using (var container = new ContainerBuilder<GenericContainer>(_dockerRegistryFixture.ImageName)
                .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                .Build())
            {
                await container.Start();
            }
        }

        [Fact]
        public async Task ShouldAlwaysPull()
        {

            await using (var container = new ContainerBuilder<GenericContainer>(_dockerRegistryFixture.ImageName)
                .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                .Build())
            {

                await container.Start();

                await _dockerRegistryFixture.RemoveImage();
            }
            await using (
                var container = new ContainerBuilder<GenericContainer>(_dockerRegistryFixture.ImageName)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .Build()
            )
            {
                await ExpectToFailWithNotFoundException(container);
            }

            await using (
                // built_in_image_pull_policy {
                var container = new ContainerBuilder<GenericContainer>(_dockerRegistryFixture.ImageName)
                    .WithImagePullPolicy(PullPolicy.AlwaysPull)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .Build()
            // }
            )
            {
                await container.Start();
            }
        }

        [Fact]
        public async Task ShouldSupportCustomPolicies()
        {
            await using (
                // custom_image_pull_policy {
                var container = new ContainerBuilder<GenericContainer>(_dockerRegistryFixture.ImageName)
                    .WithImagePullPolicy(new EnvironmentBasedImagePullPolicy())
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .Build()
    // }
    )
            {
                await container.Start();
            }
        }

        [Fact]
        public async Task ShouldCheckPolicy()
        {
            var policy = Substitute.For<IImagePullPolicy>();
            policy.ShouldPull(Arg.Any<DockerImageName>()).Returns(false);
            //policy.ShouldPullCached(Arg.Any<DockerImageName>(), Arg.Any<ImageData>()).Returns(false);

            await using (
                var container = new ContainerBuilder<GenericContainer>(_dockerRegistryFixture.ImageName)
                    .WithImagePullPolicy(policy)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .Build()


            )
            {
                await container.Start();

                _ = policy.Received().ShouldPull(Arg.Any<DockerImageName>());
            }
        }
        private class EnvironmentBasedImagePullPolicy : AbstractImagePullPolicy
        {
            public override Task<bool> ShouldPull(DockerImageName dockerImageName)
            {
                return Task.FromResult(Environment.GetEnvironmentVariable("ALWAYS_PULL_IMAGE") != null);
            }

            protected internal override bool ShouldPullCached(DockerImageName imageName, ImageData localImageData)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task ShouldNotForcePulling()
        {
            await using (
                var container = new ContainerBuilder<GenericContainer>(_dockerRegistryFixture.ImageName)
                    .WithImagePullPolicy(_ => false)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .Build()




            )
            {
                await ExpectToFailWithNotFoundException(container);
            }
        }

        private async Task ExpectToFailWithNotFoundException(GenericContainer container)
        {
            try
            {
                await container.Start();
                throw new XunitException("Should fail");
            }
            catch (ContainerLaunchException e)
            {
                Exception t = e;
                while (t.InnerException != null)
                {
                    if (t.InnerException is DockerImageNotFoundException)
                    {
                        //VisibleAssertions.pass("Caused by NotFoundException");
                        return;
                    }
                    t = t.InnerException;
                }
            }
        }

        public async Task InitializeAsync()
        {
            // Clean up local cache
            await _dockerRegistryFixture.RemoveImage();

            LocalImagesCache.Instance.Cache.TryRemove(_dockerRegistryFixture.ImageName, out _);
        }

        public async Task DisposeAsync()
        {
            await _dockerRegistryFixture.RemoveImage();
        }





    }
}
