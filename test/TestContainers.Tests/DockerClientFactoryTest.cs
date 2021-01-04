//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Docker.DotNet;
//using Docker.DotNet.Models;
//using TestContainers.Containers;
//using TestContainers.Core.Containers;
//using Xunit;

//namespace TestContainers.Tests
//{
//    public class DockerClientFactoryTest
//    {
//        [Fact]
//        public async Task RunCommandInsideDockerShouldNotFailIfImageDoesNotExistLocally()
//        {

//            var dockFactory = DockerClientFactory.Instance;
//            try
//            {
//                //remove tiny image, so it will be pulled during next command run
//                await dockFactory.Execute(c=>
//                        c.Images
//                        .DeleteImageAsync(TestImages.TINY_IMAGE.AsCanonicalNameString(), new ImageDeleteParameters { Force=true}));
//            }
//            catch (NotFoundException ignored)
//            {
//                // Do not fail if it's not pulled yet
//            }

//            dockFactory.runInsideDocker(
//                    cmd->cmd.withCmd("sh", "-c", "echo 'SUCCESS'"),
//                    (client, id)->
//                            client.logContainerCmd(id)
//                                    .withStdOut(true)
//                                    .exec(new LogToStringContainerCallback())
//                                    .toString()
//            );
//        }

//        [Fact]
//        public void ShouldHandleBigDiskSize()
//        {
//            var dfOutput = "/dev/disk1     2982480572 1491240286 2982480572    31%    /";
//            var usage = DockerClientFactory.Instance.ParseAvailableDiskSpace(dfOutput);

//            //Assert.Equal("Available MB is correct", 2982480572L / 1024L, usage.availableMB.orElse(0L));
//            //VisibleAssertions.assertEquals("Available percentage is correct", 31, usage.usedPercent.orElse(0));
//        }

//        [Fact]
//        public void DockerHostIpAddress()
//        {
//            DockerClientFactory instance = new DockerClientFactory();
//            instance._strategy = null;
//            Assert.NotNull(instance.DockerHostIpAddress);
//        }

//        [Fact]
//        public void FailedChecksFailFast()
//        {
//            var configInstance = TestContainersConfiguration.Instance;
//            try
//            {
//            //    Mockito.doReturn(false).when(TestContainersConfiguration.Instance).isDisableChecks();

//                // Make sure that Ryuk is started
//                Assert.NotNull(DockerClientFactory.Instance.Client());

//                DockerClientFactory instance = new DockerClientFactory();
//                IDockerClient dockerClient = instance._dockerClient;
//                Assert.NotNull(instance._cachedClientFailure);
//                try
//                {
//                    // Remove cached client to force the initialization logic
//                    instance._dockerClient = null;

//                    // Ryuk should fail to start twice due to the name conflict (equal to the session id)
//                    Assert.Throws<DockerApiException>(() => instance.Client());

//                    var failure = new IllegalStateException("Boom!");
//                    instance._cachedClientFailure = failure;
//                    // Fail fast
//                    var ex = Assert.Throws<Exception>(() => instance.Client());
//                    Assert.Equal(failure, ex);
//                }
//                finally
//                {
//                    instance._dockerClient = dockerClient;
//                    instance._cachedClientFailure = null;
//                }
//            }
//            finally
//            {
//                TestContainersConfiguration.SetInstance(configInstance);
//            }
//        }
//    }
//}