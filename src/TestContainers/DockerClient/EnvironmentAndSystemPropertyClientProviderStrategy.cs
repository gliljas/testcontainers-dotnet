using System;
using System.Collections.Generic;
using System.Text;
using Docker.DotNet;

namespace TestContainers.DockerClient
{

    public class EnvironmentAndSystemPropertyClientProviderStrategy : DockerClientProviderStrategy
    {
        public static int PRIORITY => 100;

        protected override DockerClientConfiguration Config => new DockerClientConfiguration(new Uri(Environment.GetEnvironmentVariable("DOCKER_HOST")));

        protected override int Priority => PRIORITY;

        protected override string Description => throw new NotImplementedException();

        protected override bool IsApplicable()
        {
            return Environment.GetEnvironmentVariable("DOCKER_HOST") != null;
        }

        
    }
}
