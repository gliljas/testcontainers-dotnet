using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TestContainers.Containers;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.Containers.Wait.Strategy
{
    public class WaitAllStrategyTest
    {
        private GenericContainer _container;
        private IWaitStrategy _strategy1;
        private IWaitStrategy _strategy2;
        private IWaitStrategy _strategy3;

        public WaitAllStrategyTest()
        {
            _container = Substitute.For<GenericContainer>();
            _strategy1 = Substitute.For<IWaitStrategy>();
            _strategy2 = Substitute.For<IWaitStrategy>();
            _strategy3 = Substitute.For<IWaitStrategy>();
        }
        /*
     * Dummy-based tests, to check that timeout values are propagated correctly, without involving actual timing-sensitive code
     */
        [Fact]
        public void ParentTimeoutApplies()
        {

            var child1 = new DummyStrategy(TimeSpan.FromMilliseconds(10));
            child1.WithStartupTimeout(TimeSpan.FromMilliseconds(20));

            Assert.Equal(20L, child1._startupTimeout.TotalMilliseconds); //"withStartupTimeout directly sets the timeout"

            new WaitAllStrategy()
                .WithStrategy(child1)
                .WithStartupTimeout(TimeSpan.FromMilliseconds(30));

            Assert.Equal(30L, child1._startupTimeout.TotalMilliseconds);//"WaitAllStrategy overrides a child's timeout"
        }

        [Fact]
        public void ParentTimeoutAppliesToAdditionalChildren()
        {

            var defaultInnerWait = TimeSpan.FromMilliseconds(2);
            var outerWait = TimeSpan.FromMilliseconds(20);

            DummyStrategy child1 = new DummyStrategy(defaultInnerWait);
            DummyStrategy child2 = new DummyStrategy(defaultInnerWait);

            new WaitAllStrategy()
                .WithStrategy(child1)
                .WithStartupTimeout(outerWait)
                .WithStrategy(child2);

            Assert.Equal(20L, child1._startupTimeout.TotalMilliseconds);//"WaitAllStrategy overrides a child's timeout (1st)"
            Assert.Equal(20L, child2._startupTimeout.TotalMilliseconds);//"WaitAllStrategy overrides a child's timeout (2nd, additional)"
        }

        [Fact]
        public async Task ChildExecutionTest()
        {

            var underTest = new WaitAllStrategy()
                .WithStrategy(_strategy1)
                .WithStrategy(_strategy2);

            //doNothing().when(strategy1).waitUntilReady(eq(container));
            //doNothing().when(strategy2).waitUntilReady(eq(container));

            await underTest.WaitUntilReady(_container);

            Received.InOrder(() =>
            {
                _strategy1.WaitUntilReady(Arg.Any<IWaitStrategyTarget>(), Arg.Any<CancellationToken>());
                _strategy2.WaitUntilReady(Arg.Any<IWaitStrategyTarget>(), Arg.Any<CancellationToken>());
            });
        }

        [Fact]
        public async Task WithoutOuterTimeoutShouldRelyOnInnerStrategies()
        {

            var underTest = new WaitAllStrategy(WaitAllStrategy.Mode.WITH_INDIVIDUAL_TIMEOUTS_ONLY)
                .WithStrategy(_strategy1)
                .WithStrategy(_strategy2)
                .WithStrategy(_strategy3);

            //_strategy1.WaitUntilReady(_container, Arg.Any<CancellationToken>());

            _strategy1.WaitUntilReady(_container, Arg.Any<CancellationToken>()).Returns(x => throw new TimeoutException());

            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await underTest.WaitUntilReady(_container);
            });//"The outer strategy timeout applies"

            Received.InOrder(() =>
            {
                _strategy1.WaitUntilReady(Arg.Any<IWaitStrategyTarget>(), Arg.Any<CancellationToken>());
                _strategy2.WaitUntilReady(Arg.Any<IWaitStrategyTarget>(), Arg.Any<CancellationToken>());
            });
            _  = _strategy3.DidNotReceiveWithAnyArgs().WaitUntilReady(Arg.Any<IWaitStrategyTarget>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public void TimeoutChangeShouldNotBePossibleWithIndividualTimeoutMode()
        {

            var underTest = new WaitAllStrategy(WaitAllStrategy.Mode.WITH_INDIVIDUAL_TIMEOUTS_ONLY);

            Assert.Throws<IllegalStateException>(() =>
            {
                underTest.WithStartupTimeout(TimeSpan.FromSeconds(42));
            });//("Cannot change timeout for individual timeouts"
        }

        [Fact]
        public void ShouldNotMessWithIndividualTimeouts()
        {

            new WaitAllStrategy(WaitAllStrategy.Mode.WITH_INDIVIDUAL_TIMEOUTS_ONLY)
                .WithStrategy(_strategy1)
                .WithStrategy(_strategy2);

            _strategy1.DidNotReceiveWithAnyArgs().WithStartupTimeout(Arg.Any<TimeSpan>());
            _strategy2.DidNotReceiveWithAnyArgs().WithStartupTimeout(Arg.Any<TimeSpan>());
        }

        [Fact]
        public void ShouldOverwriteIndividualTimeouts()
        {

            var someSeconds = TimeSpan.FromSeconds(23);
            new WaitAllStrategy()
                .WithStartupTimeout(someSeconds)
                .WithStrategy(_strategy1)
                .WithStrategy(_strategy2);

            _strategy1.Received().WithStartupTimeout(someSeconds);
            _strategy2.Received().WithStartupTimeout(someSeconds);
        }

        private class DummyStrategy : AbstractWaitStrategy
        {
            public DummyStrategy(TimeSpan defaultInnerWait)
            {
                base._startupTimeout = defaultInnerWait;
            }


            protected override Task WaitUntilReady(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
