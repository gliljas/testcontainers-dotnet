using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestContainers.Containers;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers
{
    public class DockerComposeContainerWithServicesTest
    {
        public static readonly FileInfo SIMPLE_COMPOSE_FILE = new FileInfo("src/test/resources/compose-scaling-multiple-containers.yml");
        public static readonly FileInfo COMPOSE_FILE_WITH_INLINE_SCALE = new FileInfo("src/test/resources/compose-with-inline-scale-test.yml");

        [Fact]
        public async Task TestDesiredSubsetOfServicesAreStarted()
        {
            await using (
                var compose = new DockerComposeContainerBuilder(SIMPLE_COMPOSE_FILE)
                    .WithServices("redis")
                    .Build()

            )
            {
                await compose.Start();

                await VerifyStartedContainers(compose, "redis_1");
            }
        }

        [Fact]
        public async Task TestDesiredSubsetOfScaledServicesAreStarted()
        {
            await using (
                var compose = new DockerComposeContainerBuilder(SIMPLE_COMPOSE_FILE)
                    .WithScaledService("redis", 2)
                    .Build()

            )
            {
                await compose.Start();

                await VerifyStartedContainers(compose, "redis_1", "redis_2");
            }
        }

        [Fact]
        public async Task TestDesiredSubsetOfSpecifiedAndScaledServicesAreStarted()
        {
            await using (
                var compose = new DockerComposeContainerBuilder(SIMPLE_COMPOSE_FILE)
                    .WithServices("redis")
                    .WithScaledService("redis", 2)
                    .Build()


            )
            {
                await compose.Start();

                await VerifyStartedContainers(compose, "redis_1", "redis_2");
            }
        }

        [Fact]
        public async Task TestDesiredSubsetOfSpecifiedOrScaledServicesAreStarted()
        {
            await using (
                var compose = new DockerComposeContainerBuilder(SIMPLE_COMPOSE_FILE)
                    .WithServices("other")
                    .WithScaledService("redis", 2)
                    .Build()



            )
            {
                await compose.Start();

                await VerifyStartedContainers(compose, "redis_1", "redis_2", "other_1");
            }
        }

        [Fact]
        public async Task TestAllServicesAreStartedIfNotSpecified()
        {
            await using (
                var compose = new DockerComposeContainerBuilder(SIMPLE_COMPOSE_FILE).Build()




            )
            {
                await compose.Start();

                await VerifyStartedContainers(compose, "redis_1", "other_1");
            }
        }

        [Fact]
        public async Task TestScaleInComposeFileIsRespected()
        {
            await using (
                var compose = new DockerComposeContainerBuilder(COMPOSE_FILE_WITH_INLINE_SCALE).Build()//()





            )
            {
                await compose.Start();

                // the compose file includes `scale: 3` for the redis container
                await VerifyStartedContainers(compose, "redis_1", "redis_2", "redis_3");
            }
        }

        private async Task VerifyStartedContainers(DockerComposeContainer compose, params string[] names)
        {
            var containerNames = (await compose.ListChildContainers())
                .SelectMany(container => container.Names)
                .ToList();

            Assert.Equal(
                names.Length, containerNames.Count); //"number of running services of docker-compose is the same as length of listOfServices"

            foreach (var expectedName in names)
            {
                var matches = containerNames.Count(foundName => foundName.EndsWith(expectedName));

                Assert.Equal(1L, matches); //"container with name starting '" + expectedName + "' should be running"
            }
        }
    }
}
