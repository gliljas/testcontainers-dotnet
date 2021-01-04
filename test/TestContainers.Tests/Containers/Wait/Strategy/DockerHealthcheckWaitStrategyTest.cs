using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers.Wait.Strategy
{
    public class DockerHealthcheckWaitStrategyTest
    {

        private GenericContainer _container;

        public DockerHealthcheckWaitStrategyTest()
        {
            // Using a Dockerfile here, since Dockerfile builder DSL doesn't support HEALTHCHECK
            _container = new ContainerBuilder<GenericContainer>(new ImageFromDockerfile()
                .WithFileFromClasspath("write_file_and_loop.sh", "health-wait-strategy-dockerfile/write_file_and_loop.sh")
                .WithFileFromClasspath("Dockerfile", "health-wait-strategy-dockerfile/Dockerfile"))
                .WaitingFor(Wait.ForHealthcheck().WithStartupTimeout(TimeSpan.FromSeconds(3)))
                .Build();
        }

        [Fact]
        public async Task StartsOnceHealthy()
        {
            await _container.Start();
        }

        [Fact]
        public async Task ContainerStartFailsIfContainerIsUnhealthy()
        {
            //_container.WithCommand("tail", "-f", "/dev/null");

            await Assert.ThrowsAsync<ContainerLaunchException>(async () => await _container.Start());//"Container launch fails when unhealthy"
        }
    }
}
