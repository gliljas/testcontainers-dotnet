using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace TestContainers.Containers
{
    public interface IContainerState
    {
        Task<string> GetContainerIpAddress();


        Task<string> GetHost();

        Task<bool> IsRunning();

        Task<bool> IsCreated();

        Task<bool> IsHealthy();

        Task<ContainerInspectResponse> GetCurrentContainerInfo();

        Task<int> GetFirstMappedPort();

        Task<int> GetMappedPort(int originalPort);

        Task<IReadOnlyList<int>> GetExposedPorts();

        Task<IReadOnlyList<string>> GetPortBindings();

        Task<IReadOnlyList<string>> GetBoundPortNumbers();

        Task CopyFileToContainer(FileInfo fileInfo, string containerPath);

        Task CopyFileFromContainer(string containerPath, string destinationPath);

        Task CopyFileFromContainer(string containerPath, Func<Stream> destinationFunc);

        Task CopyFileFromContainer(string containerPath, Func<Stream, Task> destinationFunc);

        ContainerInspectResponse GetContainerInfo();

        string ContainerId { get; }

    }

    public abstract class AbstractContainerState : IContainerState
    {
        public Task CopyFileFromContainer(string containerPath, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileFromContainer(string containerPath, Func<Stream> destinationFunc)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileFromContainer(string containerPath, Func<Stream, Task> destinationFunc)
        {
            throw new NotImplementedException();
        }

        public async Task CopyFileToContainer(FileInfo fileStream, string containerPath)
        {
            if (!await IsCreated())
            {
                throw new IllegalStateException("copyFileToContainer can only be used with created / running container");
            }

          
          //  DockerClientFactory.Instance.Client().Containers.ExtractArchiveToContainerAsync()
        }

        public Task<IReadOnlyList<string>> GetBoundPortNumbers()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetContainerIpAddress()
        {
            throw new NotImplementedException();
        }

        public Task<ContainerInspectResponse> GetCurrentContainerInfo()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<int>> GetExposedPorts()
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetFirstMappedPort()
        {
            var mappedPort = await (await GetExposedPorts()).Take(1).Select(async port => await GetMappedPort(port)).FirstOrDefault();

            throw new IllegalStateException("Container doesn't expose any ports");
            
        }

        public Task<string> GetHost()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetMappedPort(int originalPort)
        {
            //Preconditions.checkState(this.getContainerId() != null, "Mapped port can only be obtained after the container is started");

            //Ports.Binding[] binding = new Ports.Binding[0];
            //var containerInfo = await GetCurrentContainerInfo();
            //if (containerInfo != null)
            //{
            //    binding = containerInfo.NetworkSettings.Ports.TryGetValue(originalPort..SelectMany(x=>x).FirstOrDefault(x=>x. ,.getPorts().getBindings().get(new ExposedPort(originalPort));
            //}

            //if (binding != null && binding.length > 0 && binding[0] != null)
            //{
            //    return Integer.valueOf(binding[0].getHostPortSpec());
            //}
            //else
            //{
            //    throw new IllegalArgumentException("Requested port (" + originalPort + ") is not mapped");
            //}
            return Task.FromResult(0);
        }

        

        public Task<IReadOnlyList<string>> GetPortBindings()
        {
            return Task.FromResult<IReadOnlyList<string>>(GetContainerInfo().HostConfig.PortBindings.SelectMany(x => x.Value).Select(x => $"{x.HostPort}:z").ToList());
        }

        public async Task<bool> IsCreated()
        {
            if (ContainerId == null)
            {
                return false;
            }

            try
            {
                var state = (await GetCurrentContainerInfo()).State;
                return "created".Equals(state.Status) || state.Running;
            }
            catch (DockerApiException e)
            {
                return false;
            }
        }

        public async Task<bool> IsHealthy()
        {
            if (ContainerId == null)
            {
                return false;
            }

            try
            {
                var inspectContainerResponse = await GetCurrentContainerInfo();
                var health = inspectContainerResponse.State.Health;
                if (health == null)
                {
                    throw new RuntimeException("This container's image does not have a healthcheck declared, so health cannot be determined. Either amend the image or use another approach to determine whether containers are healthy.");
                }

                return "healthy".Equals(health.Status);
            }
            catch (DockerApiException)
            {
                return false;
            }
        }

        public async Task<bool> IsRunning()
        {
            if (ContainerId == null)
            {
                return false;
            }

            try
            {
                return (await GetCurrentContainerInfo()).State.Running;
            }
            catch (DockerApiException e)
            {
                return false;
            }
        }

        public abstract ContainerInspectResponse GetContainerInfo();

        public string ContainerId => GetContainerInfo()?.ID;
    }
}
