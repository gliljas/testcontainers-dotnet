using System;

namespace TestContainers.Containers.Output
{
    public class ToStringConsumer : IConsumer<OutputFrame>
    {
        public void Accept(OutputFrame value)
        {
            throw new System.NotImplementedException();
        }

        internal string ToUtf8String()
        {
            throw new NotImplementedException();
        }
    }

}
