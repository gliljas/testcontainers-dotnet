using System;
using System.Collections.Generic;

namespace TestContainers.Containers.WaitStrategies
{
    internal class ExternalPortListeningCheck
    {
        private IWaitStrategyTarget _waitStrategyTarget;
        private IReadOnlyList<int> externalLivenessCheckPorts;

        public ExternalPortListeningCheck(IWaitStrategyTarget waitStrategyTarget, IReadOnlyList<int> externalLivenessCheckPorts)
        {
            _waitStrategyTarget = waitStrategyTarget;
            this.externalLivenessCheckPorts = externalLivenessCheckPorts;
        }

        internal bool Check()
        {
            throw new NotImplementedException();
        }
    }
}
