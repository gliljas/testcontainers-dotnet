using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TestContainers.Core.Containers;
using TestContainers.Images.Builder;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class ParameterizedDockerfileContainerTest
    {
        [Theory]
        [InlineData("alpine:3.2", "3.2")]
        [InlineData("alpine:3.3", "3.3")]
        [InlineData("alpine:3.4", "3.4")]
        [InlineData("alpine:3.5", "3.5")]
        [InlineData("alpine:3.6", "3.6")]
        public async Task SimpleTest(string baseImage, string expectedVersion)
        {
            await using (var container = new ContainerBuilder<GenericContainer>(new ImageFromDockerfile().WithDockerfileFromBuilder(builder =>
            {
                builder
                        .From(baseImage)
                        // Could potentially customise the image here, e.g. adding files, running
                        //  commands, etc.
                        .Build();
            })).WithCommand("top").Build())
            {
                var release = (await container.ExecInContainer("cat", "/etc/alpine-release")).Stdout;

                release.Should().StartWith(expectedVersion, "/etc/alpine-release should start with " + expectedVersion);
            }
        }
    }
}
