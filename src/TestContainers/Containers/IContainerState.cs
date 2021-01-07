using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TestContainers.Containers;
using TestContainers.Utility;

namespace TestContainers
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

    public static class ContainerStateExtensions
    {
        public static async Task<ExecResult> ExecInContainer(this IContainerState containerState, params string[] command)
        {
            //if (!TestEnvironment.dockerExecutionDriverSupportsExec())
            //{
            //    // at time of writing, this is the expected result in CircleCI.
            //    throw new UnsupportedOperationException(
            //        "Your docker daemon is running the \"lxc\" driver, which doesn't support \"docker exec\".");

            //}

            if (!await containerState.IsRunning(default))
            {
                throw new IllegalStateException("execInContainer can only be used while the Container is running");
            }

            var containerId = containerState.ContainerInfo.ID;
            var containerName = containerState.ContainerInfo.Name;

            //log.debug("{}: Running \"exec\" command: {}", containerName, String.join(" ", command));
            var execCreateCmdResponse = await DockerClientFactory.Instance.Execute(c => c.Exec.ExecCreateContainerAsync(containerId, new ContainerExecCreateParameters { AttachStderr = true, AttachStdout = true, Cmd = command }));

          
            var execResult = new ExecResult();
            using (var stream = await DockerClientFactory.Instance.Execute(c => c.Exec.StartAndAttachContainerExecAsync(execCreateCmdResponse.ID, false)))
            {
                var result = await stream.ReadOutputToEndAsync(default);
                execResult.Stdout = result.stdout;
                execResult.Stderr = result.stderr;
            }
            // }
            var exitCode = (await DockerClientFactory.Instance.Execute(c => c.Exec.InspectContainerExecAsync(execCreateCmdResponse.ID))).ExitCode;
            execResult.ExitCode = exitCode;

            //log.trace("{}: stdout: {}", containerName, result.getStdout());
            //log.trace("{}: stderr: {}", containerName, result.getStderr());
            return execResult;
        }


        public static async Task<string> GetLogs(this IContainerState containerState) => await LogUtils.GetOutput(containerState.ContainerId);
        public static async Task<string> GetLogs(this IContainerState containerState, params OutputType[] outputTypes) => await LogUtils.GetOutput(containerState.ContainerId, outputTypes);
    }
}
