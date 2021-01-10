//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using TestContainers.Containers.Mounts;
//using TestContainers.Core.Containers;
//using TestContainers.Utility;
//using Xunit;

//namespace TestContainers.Tests.ContainerTests
//{
//    public class CopyFileToContainerTest
//    {

//        private static string containerPath = "/tmp/mappable-resource/";
//        private static string fileName = "test-resource.txt";

//        [Fact]
//        public async Task CheckFileCopied()
//        {
//            await using (
//                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
//                    .WithCommand("sleep", "3000")
//                    .WithCopyFileToContainer(MountableFile.ForClasspathResource("/mappable-resource/"), containerPath)
//                    .Build()
//                )
//            {
//                await container.Start();
//                string filesList = container.ExecInContainer("ls", "/tmp/mappable-resource").getStdout();
//                filesList.Should().Contain(fileName, "file list should contain the file");

//            }
//        }

//        [Fact]
//        public async Task ShouldUseCopyForReadOnlyClasspathResources()
//        {
//            await using (
//                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
//                    .WithCommand("sleep", "3000")
//                    .WithClasspathResourceMapping("/mappable-resource/", containerPath, AccessMode.ReadOnly)
//                    .Build()
//                )
//            {
//                await container.Start();
//                string filesList = (await container.ExecInContainer("ls", "/tmp/mappable-resource")).Stdout;
//                filesList.Should().Contain(fileName, "file list should contain the file");
//            }
//        }

//        [Fact]
//        public void shouldUseCopyOnlyWithReadOnlyClasspathResources()
//        {
//            string resource = "/test_copy_to_container.txt";
//            var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
//                .WithClasspathResourceMapping(resource, "/readOnly", BindMode.ReadOnly)
//                .WithClasspathResourceMapping(resource, "/readOnlyNoSelinux", BindMode.ReadOnly)

//                .WithClasspathResourceMapping(resource, "/readOnlyShared", BindMode.ReadOnly, SelinuxContext.Shared)
//                .WithClasspathResourceMapping(resource, "/readWrite", BindMode.ReadWrite)
//                .Build();

//            var copyMap = container.getCopyToFileContainerPathMap();
//            assertTrue("uses copy for read-only", copyMap.containsValue("/readOnly"));
//            assertTrue("uses copy for read-only and no Selinux", copyMap.containsValue("/readOnlyNoSelinux"));

//            assertFalse("uses mount for read-only with Selinux", copyMap.containsValue("/readOnlyShared"));
//            assertFalse("uses mount for read-write", copyMap.containsValue("/readWrite"));
//        }

//        [Fact]
//        public void shouldCreateFoldersStructureWithCopy()
//        {
//            string resource = "/test_copy_to_container.txt";
//            await using (
//                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
//                    .WithCommand("sleep", "3000")
//                    .WithClasspathResourceMapping(resource, "/a/b/c/file", BindMode.ReadOnly)
//                    .Build()
//                        )
//            {
//                await container.Start();
//                string filesList = container.execInContainer("ls", "/a/b/c/").getStdout();
//                assertTrue("file list contains the file", filesList.contains("file"));
//            }
//        }
//    }
//}


