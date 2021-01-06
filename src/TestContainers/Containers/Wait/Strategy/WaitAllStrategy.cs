using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public class WaitAllStrategy : AbstractWaitStrategy
    {
        private readonly Mode _mode;
        private List<IWaitStrategy> _strategies = new List<IWaitStrategy>();
        private TimeSpan _timeout;

        public enum Mode
        {

            /**
             * This is the default mode: The timeout of the {@link WaitAllStrategy strategy} is applied to each individual
             * strategy, so that the container waits maximum for
             * {@link org.testcontainers.containers.wait.strategy.WaitAllStrategy#timeout}.
             */
            WITH_OUTER_TIMEOUT,

            /**
             * Using this mode triggers the following behaviour: The outer timeout is disabled and the outer enclosing
             * strategy waits for all inner strategies according to their timeout. Once set, it disables
             * {@link org.testcontainers.containers.wait.strategy.WaitAllStrategy#withStartupTimeout(java.time.Duration)} method,
             * as it would overwrite inner timeouts.
             */
            WITH_INDIVIDUAL_TIMEOUTS_ONLY,

            /**
             * This is the original mode of this strategy: The inner strategies wait with their preconfigured timeout
             * individually and the wait all strategy kills them, if the outer limit is reached.
             */
            WITH_MAXIMUM_OUTER_TIMEOUT
        }

        public WaitAllStrategy() : this(Mode.WITH_OUTER_TIMEOUT)
        {

        }

        public WaitAllStrategy(Mode mode)
        {
            _mode = mode;
        }

        public override async Task WaitUntilReady(IWaitStrategyTarget waitStrategyTarget, CancellationToken cancellationToken = default)
        {
            if (_mode == Mode.WITH_INDIVIDUAL_TIMEOUTS_ONLY)
            {
                await WaitUntilNestedStrategiesAreReady(waitStrategyTarget, cancellationToken);
            }   
            else
            {
                using (var cts = new CancellationTokenSource(_startupTimeout))
                using (var lcts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token,cancellationToken))
                {
                    try
                    {
                        await WaitUntilNestedStrategiesAreReady(waitStrategyTarget, lcts.Token);
                    }
                    catch (OperationCanceledException) when (cts.IsCancellationRequested)
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }

        private async Task WaitUntilNestedStrategiesAreReady(IWaitStrategyTarget waitStrategyTarget, CancellationToken cancellationToken)
        {
            foreach (IWaitStrategy strategy in _strategies)
            {
                await strategy.WaitUntilReady(waitStrategyTarget, cancellationToken);
            }
        }

        public WaitAllStrategy WithStrategy(IWaitStrategy strategy)
        {

            if (_mode == Mode.WITH_OUTER_TIMEOUT)
            {
                ApplyStartupTimeout(strategy);
            }

            _strategies.Add(strategy);
            return this;
        }

        public new WaitAllStrategy WithStartupTimeout(TimeSpan startupTimeout)
        {

            if (_mode == Mode.WITH_INDIVIDUAL_TIMEOUTS_ONLY)
            {
                throw new IllegalStateException($"Changing startup timeout is not supported with mode {_mode}");
            }

            _timeout = startupTimeout;
            _strategies.ForEach(x=>x.WithStartupTimeout(startupTimeout));
            return this;
        }

        private void ApplyStartupTimeout(IWaitStrategy childStrategy)
        {
            childStrategy.WithStartupTimeout(_timeout);
        }

        protected override Task WaitUntilReady(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
