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
    public class DockerNetworkModeTest
    {
        [Fact]
        public async Task TestNoNetworkContainer()
        {
            await using (
            var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                .WithCommand("true")
                .WithNetworkMode("none")
                .Build()
        )
            {
                await container.Start();
                var networkSettings = container.ContainerInfo.NetworkSettings;

                networkSettings.Networks.Should().HaveCount(1, "only one network is set");
                networkSettings.Networks.Should().ContainKey("none", "network is 'none'");
            }
        }

        [Fact]
        public async Task TestHostNetworkContainer()
        {
            await using (
                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .WithCommand("true")
                    .WithNetworkMode("host")
                    .Build()
            )
            {
                await container.Start();
                var networkSettings = container.ContainerInfo.NetworkSettings;

                networkSettings.Networks.Should().HaveCount(1, "only one network is set");
                networkSettings.Networks.Should().ContainKey("host", "network is 'host'");
            }
        }
    }
}
