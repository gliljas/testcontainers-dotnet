using System.Collections.Generic;
using System.IO;

namespace TestContainers.Containers
{
    internal class ParsedDockerComposeFile
    {
        private FileInfo _x;

        public ParsedDockerComposeFile(FileInfo x)
        {
            _x = x;
        }

        public IEnumerable<string> DependencyImageNames { get; internal set; }
    }
}
