using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestContainers.Containers
{
    public class ParsedDockerfile
    {
        private ILogger _logger = new NullLoggerFactory().CreateLogger(typeof(ParsedDockerfile));
        private static Regex _fromLineRegex = new Regex("^\\s*FROM (?<arg>--[^\\s]+\\s)*(?<image>[^\\s]+).*", RegexOptions.IgnoreCase);
        private string _dockerFilePath;

        public ISet<string> DependencyImageNames { get; private set; }

        public ParsedDockerfile(string dockerfilePath)
        {
            _dockerFilePath = dockerfilePath;
            Parse(Read());
        }

        internal ParsedDockerfile(IEnumerable<string> lines)
        {
            _dockerFilePath = "dummy.Dockerfile";
            Parse(lines);
        }

        private string[] Read()
        {
            if (!File.Exists(_dockerFilePath))
            {
                _logger.LogWarning("Tried to parse Dockerfile at path {dockerFilePath} but none was found", _dockerFilePath);
                return new string[] { };
            }

            try
            {
                return File.ReadAllLines(_dockerFilePath);
            }
            catch (IOException e)
            {
                _logger.LogWarning(e, "Unable to read Dockerfile at path {dockerFilePath}", _dockerFilePath);
                return new string[] { };
            }
        }

        private void Parse(IEnumerable<string> lines)
        {
            DependencyImageNames = new HashSet<string>(lines
                .Select(x => _fromLineRegex.Match(x))
                .Where(x => x.Success)
                .Select(match => match.Groups["image"].Value)
            );

            if (DependencyImageNames.Any())
            {
                _logger.LogDebug("Found dependency images in Dockerfile {dockerFilePath}: {dependencyImageNames}", _dockerFilePath, DependencyImageNames);
            }
        }
    }
}
