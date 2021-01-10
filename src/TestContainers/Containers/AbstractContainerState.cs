using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;
using TestContainers.Core.Containers;
using TestContainers.Utility;

namespace TestContainers.Containers
{
    public abstract class AbstractContainerState : IContainerState
    {
        public Task CopyFileFromContainer(string containerPath, string destinationPath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileFromContainer(string containerPath, Func<Stream> destinationFunc, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CopyFileFromContainer(string containerPath, Func<Stream, Task> destinationFunc, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task CopyFileToContainer(FileInfo fileStream, string containerPath, CancellationToken cancellationToken)
        {
            if (!await IsCreated(cancellationToken))
            {
                throw new IllegalStateException("copyFileToContainer can only be used with created / running container");
            }


            //  DockerClientFactory.Instance.Client().Containers.ExtractArchiveToContainerAsync()
        }

        public async Task CopyFileToContainer(MountableFile mountableFile, string containerPath, CancellationToken cancellationToken)
        {
            var sourceFile = new FileInfo(mountableFile.ResolvedPath);

            if (containerPath.EndsWith("/") && sourceFile.Exists)
            {
                var logger = StaticLoggerFactory.CreateLogger<GenericContainer>();
                logger.LogWarning("folder-like containerPath in copyFileToContainer is deprecated, please explicitly specify a file path");
                await CopyFileToContainer((AbstractTransferable) mountableFile, containerPath + sourceFile.Name, cancellationToken);
            }
            else
            {
                await CopyFileToContainer((AbstractTransferable) mountableFile, containerPath, cancellationToken);
            }
        }

        /**
     *
     * Copies a file to the container.
     *
     * @param transferable file which is copied into the container
     * @param containerPath destination path inside the container
     */
        public virtual async Task CopyFileToContainer(AbstractTransferable transferable, string containerPath, CancellationToken cancellationToken)
        {
            if (!await IsCreated(cancellationToken))
            {
                throw new IllegalStateException("copyFileToContainer can only be used with created / running container");
            }

            using (
                var byteArrayOutputStream = new MemoryStream())
            using (
                var tarArchive = new TarOutputStream(byteArrayOutputStream)
            )
            {
                //tarArchive.S .setLongFileMode(TarArchiveOutputStream.LONGFILE_POSIX);

                await transferable.TransferTo(tarArchive, containerPath);
                tarArchive.Finish();

                await DockerClientFactory.Instance.Execute(c =>
                    c.Containers.ExtractArchiveToContainerAsync(ContainerId, new ContainerPathStatParameters { Path = "/" }, new MemoryStream(byteArrayOutputStream.ToArray())
                    ));
            }
        }

        public virtual Task<IReadOnlyList<string>> GetBoundPortNumbers(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual Task<string> GetContainerIpAddress(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<ContainerInspectResponse> GetCurrentContainerInfo(CancellationToken cancellationToken)
        {
            return await DockerClientFactory.Instance.Execute(c => c.Containers.InspectContainerAsync(ContainerId, cancellationToken));
        }

        public abstract Task<IReadOnlyList<int>> GetExposedPorts(CancellationToken cancellationToken);

        public virtual async Task<int> GetFirstMappedPort(CancellationToken cancellationToken)
        {
            var mappedPort = await (await GetExposedPorts(cancellationToken)).Take(1).Select(async port => await GetMappedPort(port, cancellationToken)).FirstOrDefault();

            throw new IllegalStateException("Container doesn't expose any ports");

        }

        public virtual string Host => null;// DockerClientFactory.Instance.Client().GetDockerHostIpAddress();

        public virtual Task<int> GetMappedPort(int originalPort, CancellationToken cancellationToken = default)
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



        public Task<IReadOnlyList<string>> GetPortBindings(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<string>>(ContainerInfo.HostConfig.PortBindings.SelectMany(x => x.Value).Select(x => $"{x.HostPort}:z").ToList());
        }

        public async Task<bool> IsCreated(CancellationToken cancellationToken)
        {
            if (ContainerId == null)
            {
                return false;
            }

            try
            {
                var state = (await GetCurrentContainerInfo(cancellationToken)).State;
                return "created".Equals(state.Status) || state.Running;
            }
            catch (DockerApiException)
            {
                return false;
            }
        }

        public async Task<bool> IsHealthy(CancellationToken cancellationToken)
        {
            if (ContainerId == null)
            {
                return false;
            }

            try
            {
                var inspectContainerResponse = await GetCurrentContainerInfo(cancellationToken);
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

        public async Task<bool> IsRunning(CancellationToken cancellationToken)
        {
            if (ContainerId == null)
            {
                return false;
            }

            try
            {
                return (await GetCurrentContainerInfo(cancellationToken)).State.Running;
            }
            catch (DockerApiException)
            {
                return false;
            }
        }

        public abstract ContainerInspectResponse ContainerInfo { get; }

        public virtual string ContainerId => ContainerInfo?.ID;
    }
}
