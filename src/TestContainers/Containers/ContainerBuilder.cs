using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TestContainers.Containers;
using TestContainers.Containers.Mounts;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Images;
using TestContainers.Lifecycle;
using TestContainers.Utility;

namespace TestContainers.Core.Containers
{
    public class ContainerBuilder<T> where T : GenericContainer
    {
        private ContainerOptions _options = new ContainerOptions();
        private readonly DockerImageName _dockerImage;

        public ContainerBuilder(DockerImageName dockerImage) : this(Task.FromResult(dockerImage))
        {
            _dockerImage = dockerImage;
        }

        internal ContainerBuilder<T> WithCopyFileToContainer(MountableFile mountableFile, string containerPath)
        {
            throw new NotImplementedException();
        }

        public ContainerBuilder(Task<DockerImageName> dockerImage)
        {

        }
        private ContainerBuilder<T> SetOptionAndReturnSelf(Action<ContainerOptions> optionAction)
        {
            optionAction(_options);
            return this;
        }

        public ContainerBuilder<T> DependsOn(params IStartable[] startables)
        {
            _options.DependsOn = startables;
            return this;
        }

        public ContainerBuilder<T> WaitingFor(IWaitStrategy waitStrategy) => SetOptionAndReturnSelf(o => o.WaitStrategy = waitStrategy);

        public ContainerBuilder<T> WithFileSystemBind(string hostPath, string containerPath) => this;

        public ContainerBuilder<T> WithFileSystemBind(string hostPath, string containerPath, BindMode bindMode) => this;

        public ContainerBuilder<T> WithVolumesFrom(IContainer container, BindMode bindMode) => this;

        public ContainerBuilder<T> WithExposedPorts(params int[] ports) => this;
        //public ContainerBuilder<T> WithCopyFileToContainer(MountableFile mountableFile, string containerPath);

        public ContainerBuilder<T> WithEnv(string key, string value) => SetOptionAndReturnSelf(o => { });

        internal ContainerBuilder<T> WithClasspathResourceMapping(string resource, string v, BindMode readOnly)
        {
            throw new NotImplementedException();
        }

        public ContainerBuilder<T> WithEnv(string key, Func<string, string> value) => SetOptionAndReturnSelf(o => { });

        public ContainerBuilder<T> WithEnv(IDictionary<string, string> env) => SetOptionAndReturnSelf(o => { });
        public ContainerBuilder<T> WithLabel(string key, string value) => SetOptionAndReturnSelf(o => { });
        public ContainerBuilder<T> WithLabels(IDictionary<string, string> labels) => SetOptionAndReturnSelf(o => { });


        public ContainerBuilder<T> WithCommand(string command) => SetOptionAndReturnSelf(o => o.CommandParts = command.Split(' '));
        public ContainerBuilder<T> WithCommand(params string[] commandParts) => SetOptionAndReturnSelf(o => o.CommandParts = commandParts);

        public ContainerBuilder<T> WithExtraHost(string hostname, string ipAddress) => this;

        public ContainerBuilder<T> WithNetworkMode(string networkMode) => this;

        public ContainerBuilder<T> WithNetwork(INetwork network) => SetOptionAndReturnSelf(o => o.Network = network);

        public ContainerBuilder<T> WithNetworkAliases(params string[] aliases) => SetOptionAndReturnSelf(o => o.NetworkAliases = aliases);

        public ContainerBuilder<T> WithImagePullPolicy(IImagePullPolicy policy) => this;

        public ContainerBuilder<T> WithImagePullPolicy(Func<DockerImageName, bool> policy) => this;

        public ContainerBuilder<T> WithStartupTimeout(TimeSpan duration) => this;

        public ContainerBuilder<T> WithPrivilegedMode(bool mode) => this;

        public ContainerBuilder<T> WithMinimumRunningDuration(TimeSpan minimumRunningDuration) => this;

        public ContainerBuilder<T> WithStartupCheckStrategy(IStartupCheckStrategy strategy) => SetOptionAndReturnSelf(x => x.StartupCheckStrategy = strategy); 

        public ContainerBuilder<T> WithWorkingDirectory(string workingDirectory) => this;

        public ContainerBuilder<T> WithCreateContainerCmdModifier(Action<CreateContainerParameters> modifier) => SetOptionAndReturnSelf(x => x.CreateContainerParametersModifiers.Add(modifier));

        public ContainerBuilder<T> WithLogConsumer(IProgress<string> consumer) => this;
        public T Build()
        {
            return Activator.CreateInstance(typeof(T), _dockerImage, _options) as T;
        }
    }

    public class ContainerOptions
    {
        public IWaitStrategy WaitStrategy { get; internal set; }
        public IStartupCheckStrategy StartupCheckStrategy { get; internal set; } 
        public List<Action<CreateContainerParameters>> CreateContainerParametersModifiers { get; internal set; } = new List<Action<CreateContainerParameters>>();
        public string[] CommandParts { get; internal set; }
        public Dictionary<string, string> Env { get; internal set; } = new Dictionary<string, string>();
        public long? ShmSize { get; internal set; }
        public List<int> ExposedPorts { get; internal set; } = new List<int>();
        public string[] NetworkAliases { get; internal set; }
        public INetwork Network { get; internal set; }
        public IStartable[] DependsOn { get; internal set; }
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

        internal DockerComposeContainerBuilder WithCommand(string cmd) => this;

        internal DockerComposeContainerBuilder WithEnv(Dictionary<string, string> env) => this;
    }
}
