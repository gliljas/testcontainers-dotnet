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
                GenericContainer foo = new GenericContainer(TestImages.TINY_IMAGE)
                        .WithNetwork(network)
                        .WithNetworkAliases("foo")
                        .WithCommand("/bin/sh", "-c", "while true ; do printf 'HTTP/1.1 200 OK\\n\\nyay' | nc -l -p 8080; done"))
            await using (GenericContainer bar = new GenericContainer(TestImages.TINY_IMAGE)
                        .WithNetwork(network)
                        .WithCommand("top")
            )
            {
                await foo.Start();
                await bar.Start();

                var response = (await bar.ExecInContainer("wget", "-O", "-", "http://foo:8080")).GetStdout();
                Assert.Equals("yay", response); //"received response"
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
                string id = network.Id;
                Assert.Equal(
                        "macvlan",
                        (await DockerClientFactory.Instance.Client().Networks.InspectNetworkAsync(id)).Driver//"Flag is set",

                );
            }
        }

        [Fact]
        public async Task TestModifiers()
        {
            await using (
                    var network = Network.Builder
                            //.createNetworkCmdModifier(cmd->cmd.withDriver("macvlan"))
                            .Build()
            )
            {
                string id = network.Id;
                Assert.Equal(
                        "macvlan",
                        (await DockerClientFactory.Instance.Client().Networks.InspectNetworkAsync(id)).Driver // "Flag is set",

                );
            }
        }


        [Fact]
        public async Task TestReusability()
        {
            await using (var network = Network.NewNetwork())
            {
                string firstId = network.Id;
                Assert.NotNull(
                        await DockerClientFactory.Instance.Client().Networks.InspectNetworkAsync(firstId)// "Network exists",

                );

                network.Close();

                Assert.NotEqual(

                        firstId,
                        (await DockerClientFactory.Instance.Client().Networks.InspectNetworkAsync(network.Id)).ID//"New network created",
                );
            }
        }
    }
}
