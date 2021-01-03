using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TestContainers.Containers.Output
{
    public class FrameConsumerResultCallback
    {
    }

    public class LoggerConsumer : IProgress<string>
    {
        private readonly ILogger _logger;
        private readonly bool _separateOutputStreams;
        private string _prefix;

        public LoggerConsumer(ILogger logger) : this(logger, false)
        {
        }

        public LoggerConsumer(ILogger logger, bool separateOutputStreams)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _separateOutputStreams = separateOutputStreams;
        }

        public LoggerConsumer WithPrefix(string prefix)
        {
            _prefix = "[" + prefix + "] ";
            return this;
        }
        public void Report(string value)
        {
            if (_separateOutputStreams)
            {
                _logger.LogInformation("{prefix}{value}", string.IsNullOrEmpty(_prefix) ? "" : (_prefix + ": "), value);
            }
            else
            {
                _logger.LogInformation("{prefix}{}: {}", _prefix, ""/*outputType*/, value);
            }
        }
    }
}
