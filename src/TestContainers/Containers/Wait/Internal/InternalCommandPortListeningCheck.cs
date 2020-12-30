namespace TestContainers.Containers.WaitStrategies
{
    internal class InternalCommandPortListeningCheck
    {
        private IWaitStrategyTarget _waitStrategyTarget;
        private object internalPorts;

        public InternalCommandPortListeningCheck(IWaitStrategyTarget waitStrategyTarget, object internalPorts)
        {
            _waitStrategyTarget = waitStrategyTarget;
            this.internalPorts = internalPorts;
        }
    }
}