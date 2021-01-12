using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestContainers.Containers.Output
{
    public class WaitingConsumer : IConsumer<OutputFrame>
    {
        public void Accept(OutputFrame value)
        {
            throw new NotImplementedException();
        }

        internal Task WaitUntilEnd(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        internal Task WaitUntil(Func<OutputFrame, bool> predicate)
        {
            throw new NotImplementedException();
        }

        internal Task WaitUntil(Func<OutputFrame, bool> predicate, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }

}
