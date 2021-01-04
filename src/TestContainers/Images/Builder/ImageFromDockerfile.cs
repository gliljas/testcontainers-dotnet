using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Utility;

namespace TestContainers.Images.Builder
{
    public class ImageFromDockerfile
    {
        private readonly string _dockerImageName;
        private readonly bool _deleteOnExit;
        private Dictionary<string, ITransferable> _transferables = new Dictionary<string, ITransferable>();
        private ILogger _logger;
        public ImageFromDockerfile() : this("testcontainers/" + Base58.RandomString(16).ToLower())
        {
        }

        internal ImageFromDockerfile WithFileFromClasspath(string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public ImageFromDockerfile(string dockerImageName) : this(dockerImageName, true)
        {
        }

        public ImageFromDockerfile(string dockerImageName, bool deleteOnExit)
        {
            _dockerImageName = dockerImageName;
            _deleteOnExit = deleteOnExit;
        }

        public ImageFromDockerfile WithFileFromTransferable(string path, ITransferable transferable)
        {
            _transferables.TryGetValue(path, out var oldValue);
            _transferables[path] = transferable;

            if (oldValue != null)
            {
                _logger.LogWarning("overriding previous mapping for '{path}'", path);
            }

            return this;
        }

        public Task<DockerImageName> GetTask()
        {
            return Task.FromResult<DockerImageName>(null);
        }
    }
}
