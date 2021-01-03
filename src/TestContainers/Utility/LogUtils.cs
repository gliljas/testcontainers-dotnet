using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace TestContainers.Utility
{
    public static class LogUtils
    {
        public static async Task FollowOutput(IDockerClient dockerClient,
                            string containerId,
                            IProgress<string> consumer,
                            params OutputType[] types)
        {

            await AttachConsumer(dockerClient, containerId, consumer, true, types);
        }

        private static async Task AttachConsumer(
        IDockerClient dockerClient,
        string containerId,
        IProgress<string> consumer,
        bool followStream,
        params OutputType[] types
    )
        {
            var parameters = new ContainerLogsParameters { Follow = true, Since = "0" };
                    
            var callback = new ProgressCallback<string>();
            foreach (var type in types)
            {
                callback.AddConsumer(type, consumer);
                if (type == OutputType.STDOUT)
                {
                    parameters.ShowStdout = true;
                }
                else if (type == OutputType.STDERR)
                {
                    parameters.ShowStderr = true;
                }
            }

            await dockerClient.Containers.GetContainerLogsAsync(containerId, parameters, default, callback);
        }
    }
}
