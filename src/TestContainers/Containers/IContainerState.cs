using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace TestContainers.Containers
{
    public interface IContainerState
    {
        Task<string> GetContainerIpAddress(CancellationToken cancellationToken);


        string Host { get; }

        Task<bool> IsRunning(CancellationToken cancellationToken);

        Task<bool> IsCreated(CancellationToken cancellationToken);

        Task<bool> IsHealthy(CancellationToken cancellationToken);

        Task<ContainerInspectResponse> GetCurrentContainerInfo(CancellationToken cancellationToken);

        Task<int> GetFirstMappedPort(CancellationToken cancellationToken);

        Task<int> GetMappedPort(int originalPort, CancellationToken cancellationToken);

        Task<IReadOnlyList<int>> GetExposedPorts(CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetPortBindings(CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetBoundPortNumbers(CancellationToken cancellationToken);

        Task CopyFileToContainer(FileInfo fileInfo, string containerPath, CancellationToken cancellationToken);

        Task CopyFileFromContainer(string containerPath, string destinationPath, CancellationToken cancellationToken);

        Task CopyFileFromContainer(string containerPath, Func<Stream> destinationFunc, CancellationToken cancellationToken);

        Task CopyFileFromContainer(string containerPath, Func<Stream, Task> destinationFunc, CancellationToken cancellationToken);

        ContainerInspectResponse ContainerInfo { get; }

        string ContainerId { get; }

    }
}
