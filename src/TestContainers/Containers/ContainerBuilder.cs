using System;
using System.Collections.Generic;
using System.IO;
using Docker.DotNet.Models;
using TestContainers.Containers;
using TestContainers.Containers.Mounts;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Images;

namespace TestContainers.Core.Containers
{
    public class ContainerBuilder<T> where T : IContainer
    {
        private ContainerOptions _options = new ContainerOptions();

        public ContainerBuilder(DockerImageName dockerImage)
        {

        }
        private ContainerBuilder<T> SetOptionAndReturnSelf(Action<ContainerOptions> optionAction)
        {
            optionAction(_options);
            return this;
        }
        public ContainerBuilder<T> WaitingFor(IWaitStrategy waitStrategy) => SetOptionAndReturnSelf(o => o.WaitingFor(waitStrategy));

        public ContainerBuilder<T> WithFileSystemBind(string hostPath, string containerPath) => this;

        public ContainerBuilder<T> WithFileSystemBind(string hostPath, string containerPath, AccessMode accessMode) => this;

        public ContainerBuilder<T> WithVolumesFrom(IContainer container, AccessMode accessMode) => this;

        public ContainerBuilder<T> WithExposedPorts(params int[] ports) => this;
        //public ContainerBuilder<T> WithCopyFileToContainer(MountableFile mountableFile, string containerPath);

        public ContainerBuilder<T> WithEnv(string key, string value) => SetOptionAndReturnSelf(o => { });

        public ContainerBuilder<T> WithEnv(string key, Func<string, string> value) => SetOptionAndReturnSelf(o => { });

        public ContainerBuilder<T> WithEnv(IDictionary<string, string> env) => SetOptionAndReturnSelf(o => { });
        public ContainerBuilder<T> WithLabel(string key, string value) => SetOptionAndReturnSelf(o => { });
        public ContainerBuilder<T> WithLabels(IDictionary<string, string> labels) => SetOptionAndReturnSelf(o => { });

        public ContainerBuilder<T> WithCommand(params string[] commandParts) => this;

        public ContainerBuilder<T> WithExtraHost(string hostname, string ipAddress) => this;

        public ContainerBuilder<T> WithNetworkMode(string networkMode) => this;

        public ContainerBuilder<T> WithNetwork(INetwork network) => this;

        public ContainerBuilder<T> WithNetworkAliases(params string[] aliases) => this;

        public ContainerBuilder<T> WithImagePullPolicy(IImagePullPolicy policy) => this;

        public ContainerBuilder<T> WithImagePullPolicy(Func<DockerImageName, bool> policy) => this;

        public ContainerBuilder<T> WithStartupTimeout(TimeSpan duration) => this;

        public ContainerBuilder<T> WithPrivilegedMode(bool mode) => this;

        public ContainerBuilder<T> WithMinimumRunningDuration(TimeSpan minimumRunningDuration) => this;

        public ContainerBuilder<T> WithStartupCheckStrategy(IStartupCheckStrategy strategy) => this;

        public ContainerBuilder<T> WithWorkingDirectory(string workingDirectory) => this;

        public ContainerBuilder<T> WithCreateContainerCmdModifier(Action<CreateContainerParameters> modifier) => this;
        public T Build()
        {
            return default;
        }
    }

    public class ContainerOptions
    {
        internal void WaitingFor(IWaitStrategy waitStrategy)
        {
            throw new NotImplementedException();
        }
    }

    public class DockerComposeContainerBuilder  : IBuilder<DockerComposeContainer>
    {
        public DockerComposeContainerBuilder(FileInfo dockerComposeFile)
        {

        }
        public DockerComposeContainer Build() => default;

        internal DockerComposeContainerBuilder WithScaledService(string v1, int v2)
        {
            throw new NotImplementedException();
        }

        internal DockerComposeContainerBuilder WithServices(string v)
        {
            throw new NotImplementedException();
        }
    }
}
