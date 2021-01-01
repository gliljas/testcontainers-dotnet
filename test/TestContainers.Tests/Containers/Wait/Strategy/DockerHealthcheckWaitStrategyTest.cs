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
            _container = new GenericContainer(new ImageFromDockerfile()
                .withFileFromClasspath("write_file_and_loop.sh", "health-wait-strategy-dockerfile/write_file_and_loop.sh")
                .withFileFromClasspath("Dockerfile", "health-wait-strategy-dockerfile/Dockerfile"))
                .waitingFor(Wait.ForHealthcheck().WithStartupTimeout(TimeSpan.FromSeconds(3)));
        }

        [Fact]
        public async Task StartsOnceHealthy()
        {
            await _container.Start();
        }

        [Fact]
        public void containerStartFailsIfContainerIsUnhealthy()
        {
            _container.WithCommand("tail", "-f", "/dev/null");

            _ = Assert.ThrowsAsync<ContainerLaunchException>(async () => await _container.Start());//"Container launch fails when unhealthy"
        }
    }
}
