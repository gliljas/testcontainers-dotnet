using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace TestContainers.Utility
{
    public static class LogUtils
    {
        public static async Task FollowOutput(
                            string containerId,
                            IProgress<string> consumer,
                            params OutputType[] types)
        {

            await AttachConsumer(containerId, consumer, true, types);
        }

        public static async Task<string> GetOutput(
                            string containerId,
                            params OutputType[] types)
        {

            if (containerId == null)
            {
                return "";
            }

            if (types.Length == 0)
            {
                types = new[] { OutputType.STDOUT, OutputType.STDERR };
            }

            var parameters = new ContainerLogsParameters { Follow = true, Since = "0", ShowStderr=true, ShowStdout=true };


            using (var r = new StreamReader(await DockerClientFactory.Instance.Execute(c => c.Containers.GetContainerLogsAsync(containerId, parameters, default))))
            {
                return r.ReadToEnd();
            }


            //final ToStringConsumer consumer = new ToStringConsumer();
            //final WaitingConsumer wait = new WaitingConsumer();
            //try (Closeable closeable = attachConsumer(dockerClient, containerId, consumer.andThen(wait), false, types)) {
            //    wait.waitUntilEnd();
            //    return consumer.toUtf8String();
            //}
        }

        private static async Task AttachConsumer(
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

                await DockerClientFactory.Instance.Execute(c => c.Containers.GetContainerLogsAsync(containerId, parameters, default, callback));
            }
        }
    }
