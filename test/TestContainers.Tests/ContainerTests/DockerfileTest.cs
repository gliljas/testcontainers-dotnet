//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using TestContainers.Containers.Output;
//using TestContainers.Containers.StartupStrategies;
//using TestContainers.Core.Containers;
//using TestContainers.Images.Builder;
//using TestContainers.Images.Builder.Dockerfile.Traits;
//using TestContainers.Utility;
//using Xunit;

//namespace TestContainers.Tests.ContainerTests
//{
//    public class DockerfileTest
//    {
//        private static readonly ILogger LOGGER = StaticLoggerFactory.CreateLogger<DockerfileTest>();

//        [Fact]
//        public async Task SimpleDockerfileWorks()
//        {
//            var image = new ImageFromDockerfile()
//                    .WithFileFromString("folder/someFile.txt", "hello")
//                    .WithFileFromClasspath("test.txt", "mappable-resource/test-resource.txt")
//                    .WithFileFromClasspath("Dockerfile", "mappable-dockerfile/Dockerfile");

//            await VerifyImage(image);
//        }

//        //    [Fact]
//        //    public void customizableImage()
//        //    {
//        //        ImageFromDockerfile image = new ImageFromDockerfile() {
//        //        @Override
//        //        protected void configure(BuildImageCmd buildImageCmd)
//        //        {
//        //            super.configure(buildImageCmd);

//        //            List<String> dockerfile = Arrays.asList(
//        //                    "FROM alpine:3.2",
//        //                    "RUN echo 'hello from Docker build process'",
//        //                    "CMD yes"
//        //            );
//        //            withFileFromString("Dockerfile", String.join("\n", dockerfile));

//        //            buildImageCmd.WithNoCache(true);
//        //        }
//        //    };

//        //    await VerifyImage(image);
//        //}

//        [Fact]
//        public async Task DockerfileBuilderWorks()
//        {
//            var image = new ImageFromDockerfile()
//                    .WithFileFromClasspath("test.txt", "mappable-resource/test-resource.txt")
//                    .WithFileFromString("folder/someFile.txt", "hello")
//                    .WithDockerfileFromBuilder(builder => builder
//                            .From("alpine:3.2")
//                            .WorkDir("/app")
//                            .Add("test.txt", "test file.txt")
//                            .Run("ls", "-la", "/app/test file.txt")
//                            .Copy("folder/someFile.txt", "/someFile.txt")
//                            .Expose(80, 8080)
//                            .Cmd("while true; do cat /someFile.txt | nc -l -p 80; done")
//                    );

//            await VerifyImage(image);
//        }

//        [Fact]
//        public async Task FilePermissions()
//        {

//            var consumer = new WaitingConsumer();

//            ImageFromDockerfile image = new ImageFromDockerfile()
//                    .WithFileFromTransferable("/someFile.txt", new MockTransferable(() => 0, () => new byte[0], () => "test file", () => 0123))
//                    .WithDockerfileFromBuilder(builder => builder
//                            .From("alpine:3.2")
//                            .Copy("someFile.txt", "/someFile.txt")
//                            .Cmd("stat -c \"%a\" /someFile.txt")
//                    );

//            var container = new ContainerBuilder<GenericContainer>(image)
//                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
//                    .WithLogConsumer(consumer)
//                    .Build();

//            try
//            {
//                await container.Start();

//               // consumer.WaitUntil(frame=>frame.Type == OutputType.STDOUT && frame.getUtf8String().contains("123"), 5, TimeUnit.SECONDS);

//            }
//            finally
//            {
//                await container.Stop();
//            }
//        }

//        private class MockTransferable : ITransferable
//        {
//            private readonly Func<int> _size;
//            private readonly Func<byte[]> _bytes;
//            private readonly Func<string> _description;
//            private readonly Func<int> _fileMode;

//            public MockTransferable(Func<int> size, Func<byte[]> bytes, Func<string> description, Func<int> fileMode)
//            {
//                _size = size;
//                _bytes = bytes;
//                _description = description;
//                _fileMode = fileMode;
//            }

//            public int FileMode => _fileMode();

//            public long Size => _size();

//            public string Description => _description();

//            public byte[] GetBytes() => _bytes();
//        }

//        protected async Task VerifyImage(ImageFromDockerfile image)
//        {
//            var container = new ContainerBuilder<GenericContainer>(image).Build();

//            try
//            {
//                await container.Start();

//             //   pass("Should start from Dockerfile");
//            }
//            finally
//            {
//                await container.Stop();
//            }
//        }

//    }
//}
