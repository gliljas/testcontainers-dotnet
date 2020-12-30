using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TestContainers
{
    public class ContainerFetchException : Exception
    {
        public ContainerFetchException()
        {
        }

        public ContainerFetchException(string message) : base(message)
        {
        }

        public ContainerFetchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContainerFetchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
