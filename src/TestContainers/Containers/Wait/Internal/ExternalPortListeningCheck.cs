using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

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

        internal bool Invoke()
        {
            var address = _waitStrategyTarget.Host;

            Parallel.ForEach(externalLivenessCheckPorts, externalPort  => {
                try
                {
                    var tc = new TcpClient();
                    try
                    {

                        tc.Connect(address, externalPort);
                        bool stat = tc.Connected;
                        tc.Close();
                    }
                    finally
                    {
                        tc.Close();
                    }
                }
                catch 
                {
                    throw new IllegalStateException("Socket not listening yet: " + externalPort);
                }

            });
            return true;
        }
    }
}
