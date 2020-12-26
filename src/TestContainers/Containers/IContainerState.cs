using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        public Task<int> GetFirstMappedPort()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetHost()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetMappedPort(int originalPort)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<string>> GetPortBindings()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsCreated()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsHealthy()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsRunning()
        {
            throw new NotImplementedException();
        }
    }
}
