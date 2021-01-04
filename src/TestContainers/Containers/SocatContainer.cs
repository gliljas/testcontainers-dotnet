using System;
using System.Collections.Generic;
using System.Linq;
using TestContainers.Core.Containers;
using TestContainers.Images;

namespace TestContainers.Containers
{
    public class SocatContainer : GenericContainer
    {
        private readonly Dictionary<int, string> _targets = new Dictionary<int, string>();

        public SocatContainer() : this(DockerImageName.Parse("alpine/socat:1.7.3.4-r0"))
        {

        }

        public SocatContainer(DockerImageName dockerImageName) : base(dockerImageName)
        {
            //WithCreateContainerCmdModifier(it=>it.WithEntrypoint("/bin/sh"));
            //WithCreateContainerCmdModifier(it=>it.WithName("testcontainers-socat-" + Base58.RandomString(8)));
        }

        public SocatContainer WithTarget(int exposedPort, string host)
        {
            return WithTarget(exposedPort, host, exposedPort);
        }

        public SocatContainer WithTarget(int exposedPort, string host, int internalPort)
        {
            //AddExposedPort(exposedPort);
            _targets[exposedPort]=string.Format("%s:%s", host, internalPort);
            return this;
        }

        protected override void Configure()
        {
            //WithCommand("-c",
            //        string.Join(" & ",_targets.Select(entry=>"socat TCP-LISTEN:" + entry.Key + ",fork,reuseaddr TCP:" + entry.Value))
                            
            //);
        }
    }
}
