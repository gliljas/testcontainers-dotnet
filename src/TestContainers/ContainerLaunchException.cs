using System;

namespace TestContainers
{
    public class ContainerLaunchException : Exception
    {
        public ContainerLaunchException(string message) : base(message) { }
        public ContainerLaunchException(string message, Exception exception) : base(message, exception) { }
    }
}
