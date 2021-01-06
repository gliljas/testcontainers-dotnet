using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestContainers
{
    internal class StaticLoggerFactory
    {
        internal static ILogger CreateLogger(Type type)
        {
            return new NullLoggerFactory().CreateLogger(type.Name);
        }

        internal static ILogger CreateLogger(string name)
        {
            return new NullLoggerFactory().CreateLogger(name);
        }

        internal static ILogger CreateLogger<T>() => CreateLogger(typeof(T));
    }
}
