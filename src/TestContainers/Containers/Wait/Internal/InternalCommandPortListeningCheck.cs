using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    internal class InternalCommandPortListeningCheck
    {
        private IWaitStrategyTarget _waitStrategyTarget;
        private IImmutableSet<int> _internalPorts;

        public InternalCommandPortListeningCheck(IWaitStrategyTarget waitStrategyTarget, IImmutableSet<int> internalPorts)
        {
            _waitStrategyTarget = waitStrategyTarget;
            _internalPorts = internalPorts;
        }

        public async Task<bool> Invoke()
        {
            string command = "true";

            foreach (var internalPort in _internalPorts)
            {
                command += " && ";
                command += " (";
                command += $"cat /proc/net/tcp* | awk '{{print $2}}' | grep -i ':0*{internalPort}'";
                command += " || ";
                command += $"nc -vz -w 1 localhost {internalPort}";
                command += " || ";
                command += $"/bin/bash -c '</dev/tcp/localhost/{internalPort}'";
                command += ")";
            }

            var before = DateTimeOffset.Now;
            try
            {
                ExecResult result = await _waitStrategyTarget.ExecInContainer("/bin/sh", "-c", command);
                //log.trace("Check for {} took {}", internalPorts, Duration.between(before, Instant.now()));
                return result.ExitCode == 0;
            }
            catch (Exception e)
            {
                throw new IllegalStateException("", e);
            }
        }
    }
}
