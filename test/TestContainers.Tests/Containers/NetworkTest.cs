using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers
{
    public class NetworkTest
    {
        [Fact]
        public async Task TestNetworkSupport()
        {
            // useCustomNetwork {
            await using (
                    Network network = Network.NewNetwork())
            await using (
                var foo = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                        .WithNetwork(network)
                        .WithNetworkAliases("foo")
                        .WithCommand("/bin/sh", "-c", "while true ; do printf 'HTTP/1.1 200 OK\\n\\nyay' | nc -l -p 8080; done")
                        .Build())
            await using (var bar = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                        .WithNetwork(network)
                        .WithCommand("top")
                        .Build()
            )
            {
                await foo.Start();
                await bar.Start();

                var response = (await bar.ExecInContainer("wget", "-O", "-", "http://foo:8080")).Stdout;
                Assert.Equal("yay", response); //"received response"
            }
        }


        [Fact]
        public async Task TestBuilder()
        {
            await using (
                    Network network = Network.Builder
                            .Driver("macvlan")
                            .Build()
            )
            {
                string id = await network.GetId();
                Assert.Equal(
                        "macvlan",
                        (await DockerClientFactory.Instance.Execute(c=>c.Networks.InspectNetworkAsync(id))).Driver//"Flag is set",

                );
            }
        }

        [Fact]
        public async Task TestModifiers()
        {
            await using (
                    var network = Network.Builder
                            .WithCreateNetworkCmdModifier(cmd=>cmd.Driver="macvlan")
                            .Build()
            )
            {
                string id = await network.GetId();
                Assert.Equal(
                        "macvlan",
                        (await DockerClientFactory.Instance.Execute(c=>c.Networks.InspectNetworkAsync(id))).Driver // "Flag is set",

                );
            }
        }


        [Fact]
        public async Task TestReusability()
        {
            await using (var network = Network.NewNetwork())
            {
                string firstId = await network.GetId();
                Assert.NotNull(
                        await DockerClientFactory.Instance.Execute(c=>c.Networks.InspectNetworkAsync(firstId))// "Network exists",

                );

                await network.Close();

                Assert.NotEqual(

                        firstId,
                        (await DockerClientFactory.Instance.Execute(async c=>await c.Networks.InspectNetworkAsync(await network.GetId()))).ID//"New network created",
                );
            }
        }
    }
}
