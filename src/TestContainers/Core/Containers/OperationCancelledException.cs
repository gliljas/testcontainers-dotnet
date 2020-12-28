using System;
using System.Runtime.Serialization;

namespace TestContainers.Core.Containers
{
    [Serializable]
    internal class OperationCancelledException : Exception
    {
        public OperationCancelledException()
        {
        }

        public OperationCancelledException(string message) : base(message)
        {
        }

        public OperationCancelledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OperationCancelledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}