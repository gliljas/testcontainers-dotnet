using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
                var compose = new DockerComposeContainer(SIMPLE_COMPOSE_FILE)
                    .WithServices("redis")

            )
            {
                await compose.Start();

                VerifyStartedContainers(compose, "redis_1");
            }
        }

        [Fact]
        public async Task TestDesiredSubsetOfScaledServicesAreStarted()
        {
            await using (
                var compose = new DockerComposeContainer(SIMPLE_COMPOSE_FILE)
                    .WithScaledService("redis", 2)

            )
            {
                await compose.Start();

                VerifyStartedContainers(compose, "redis_1", "redis_2");
            }
        }

        [Fact]
        public void TestDesiredSubsetOfSpecifiedAndScaledServicesAreStarted()
        {
            await using (
                var compose = new DockerComposeContainer(SIMPLE_COMPOSE_FILE)
                    .WithServices("redis")
                    .WithScaledService("redis", 2)


            )
            {
                compose.start();

                VerifyStartedContainers(compose, "redis_1", "redis_2");
            }
        }

        [Fact]
        public async Task TestDesiredSubsetOfSpecifiedOrScaledServicesAreStarted()
        {
            await using (
                var compose = new DockerComposeContainer(SIMPLE_COMPOSE_FILE)
                    .WithServices("other")
                    .WithScaledService("redis", 2)



            )
            {
                await compose.Start();

                VerifyStartedContainers(compose, "redis_1", "redis_2", "other_1");
            }
        }

        [Fact]
        public void TestAllServicesAreStartedIfNotSpecified()
        {
            await using (
                var compose = new DockerComposeContainer(SIMPLE_COMPOSE_FILE)




            )
            {
                await compose.Start();

                VerifyStartedContainers(compose, "redis_1", "other_1");
            }
        }

        [Fact]
        public async Task TestScaleInComposeFileIsRespected()
        {
            await using (
                var compose = new DockerComposeContainer(COMPOSE_FILE_WITH_INLINE_SCALE)





            )
            {
                await compose.Start();

                // the compose file includes `scale: 3` for the redis container
                VerifyStartedContainers(compose, "redis_1", "redis_2", "redis_3");
            }
        }

        private void VerifyStartedContainers(DockerComposeContainer compose, params string[] names)
        {
            var containerNames = compose.ListChildContainers()
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
