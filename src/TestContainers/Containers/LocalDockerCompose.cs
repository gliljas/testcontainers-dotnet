using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Utility;

namespace TestContainers.Containers
{
    internal class LocalDockerCompose : IDockerCompose
    {
        string ENV_PROJECT_NAME = "COMPOSE_PROJECT_NAME";
        string ENV_COMPOSE_FILE = "COMPOSE_FILE";

        /**
         * Executable name for Docker Compose.
         */
        private static readonly string COMPOSE_EXECUTABLE = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "docker-compose.exe" : "docker-compose";

        private readonly IReadOnlyList<FileInfo> _composeFiles;
        private readonly string _identifier;
        private string _cmd = "";
        private Dictionary<string, string> _env = new Dictionary<string, string>();
        private ILogger _logger;

        public LocalDockerCompose(IReadOnlyList<FileInfo> composeFiles, String identifier)
        {
            _composeFiles = composeFiles;
            _identifier = identifier;
        }

        public IDockerCompose WithCommand(string cmd)
        {
            _cmd = cmd;
            return this;
        }


        public IDockerCompose WithEnv(Dictionary<string, string> env)
        {
            _env = env;
            return this;
        }

        internal static bool ExecutableExists()
        {
            return CommandLine.ExecutableExists(COMPOSE_EXECUTABLE);
        }

        public async Task Invoke()
        {
            // bail out early
            if (!ExecutableExists())
            {
                throw new ContainerLaunchException("Local Docker Compose not found. Is " + COMPOSE_EXECUTABLE + " on the PATH?");
            }

            var environment = _env.ToDictionary(x => x.Key, x => x.Value);
            environment[ENV_PROJECT_NAME] = _identifier;

            var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
            if (dockerHost == null)
            {
                TransportConfig transportConfig = DockerClientFactory.Instance.GetTransportConfig();
                SslConfig sslConfig = transportConfig.SslConfig;
                if (sslConfig != null)
                {
                    if (sslConfig is LocalDirectorySslConfig localSslCOnfig)
                    {
                        environment["DOCKER_CERT_PATH"] = localSslCOnfig.DockerCertPath;
                        environment["DOCKER_TLS_VERIFY"] = "true";
                    }
                    else
                    {
                        _logger.LogWarning("Couldn't set DOCKER_CERT_PATH. `sslConfig` is present but it's not LocalDirectorySSLConfig.");
                    }
                }
                dockerHost = transportConfig.DockerHost.ToString();
            }
            environment["DOCKER_HOST"] = dockerHost;

            var absoluteDockerComposeFilePaths = _composeFiles
                .Select(x => x.FullName);

            var composeFileEnvVariableValue = string.Join(Path.PathSeparator.ToString(), absoluteDockerComposeFilePaths);
            Logger.LogDebug("Set env COMPOSE_FILE={composeFileEnvVariableValue}", composeFileEnvVariableValue);

            var pwd = _composeFiles[0].Directory.FullName;
            environment[ENV_COMPOSE_FILE] = composeFileEnvVariableValue;

            Logger.LogInformation("Local Docker Compose is running command: {cmd}", _cmd);

            var command = (COMPOSE_EXECUTABLE + " " + _cmd).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var process = new Process();

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = pwd,
                    CreateNoWindow = false,
                    UseShellExecute = true
                };

                foreach (var entry in environment)
                {
                    processStartInfo.EnvironmentVariables[entry.Key] = entry.Value;
                }

                process.StartInfo = processStartInfo;


                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => _logger.LogInformation(e.Data);
                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => _logger.LogInformation(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                Logger.LogInformation("Docker Compose has finished running");

            }
            catch (SystemException e)
            {
                throw new ContainerLaunchException("Local Docker Compose exited abnormally with code " +
                                                   process.ExitCode + " whilst running command: " + _cmd);

            }
            catch (Exception e)
            {
                throw new ContainerLaunchException("Error running local Docker Compose command: " + _cmd, e);
            }
        }

        private ILogger Logger => DockerLoggerFactory.GetLogger(COMPOSE_EXECUTABLE);



        /**
         * @return a logger
         */

    }
}
