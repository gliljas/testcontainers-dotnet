using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class WorkingDirectoryTest
    {
        [Fact]
        public async Task CheckOutput()
        {
            await using (var container = new ContainerBuilder<GenericContainer>(TestImages.ALPINE_IMAGE)
                .WithWorkingDirectory("/etc")
                .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                .WithCommand("ls", "-al")
                .Build())
            {
                var listing = await container.GetLogs();

                listing.Should().Contain("hostname", "Directory listing should expected /etc content");

                listing.Should().Contain("init.d", "Directory listing should expected /etc content");

                listing.Should().Contain("passwd", "Directory listing should expected /etc content");
            }
        }
    }
}
