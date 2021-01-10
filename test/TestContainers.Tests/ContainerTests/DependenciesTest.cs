using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Core.Containers;
using TestContainers.Lifecycle;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class DependenciesTest
    {
        [Fact]
        public async Task ShouldWorkWithSimpleDependency()
        {
            var startable = Substitute.For<IStartable>();

            await using (
                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .DependsOn(startable)
                    .Build()

            )
            {
                await container.Start();
            }

            _ = startable.ReceivedWithAnyArgs(1).Start();
            _ = startable.DidNotReceiveWithAnyArgs().Stop();
//            VisibleAssertions.assertEquals("Started once", 1, startable.StartInvocationCount
  //          VisibleAssertions.assertEquals("Does not trigger .stop()", 0, startable.getStopInvocationCount().intValue());
        }

        [Fact]
        public async Task ShouldWorkWithMultipleDependencies()
        {
            var startable1 = Substitute.For<IStartable>();
            var startable2 = Substitute.For<IStartable>();

            await using (
                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .DependsOn(startable1, startable2)
                    .Build()
            )
            {
                await container.Start();
            }

            _ = startable1.ReceivedWithAnyArgs(1).Start();
            _ = startable2.ReceivedWithAnyArgs(1).Start();

            //VisibleAssertions.assertEquals("Startable1 started once", 1, startable1.StartInvocationCount
            //VisibleAssertions.assertEquals("Startable2 started once", 1, startable2.StartInvocationCount
        }

        [Fact]
        public async Task ShouldStartEveryTime()
        {
            var startable = Substitute.For<IStartable>();

            await using (
                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .DependsOn(startable)
                    .Build()

            )
            {
                await container.Start();
                await container.Stop();

                await container.Start();
                await container.Stop();

                await container.Start();
            }

            _ = startable.ReceivedWithAnyArgs(3).Start();
            _ = startable.DidNotReceiveWithAnyArgs().Stop();
            //    VisibleAssertions.assertEquals("Started multiple times", 3, startable.StartInvocationCount
            //    VisibleAssertions.assertEquals("Does not trigger .stop()", 0, startable.getStopInvocationCount().intValue());
        }

        [Fact]
        public async Task ShouldStartTransitiveDependencies()
        {
            var transitiveOfTransitiveStartable = Substitute.For<IStartable>();
            var transitiveStartable = Substitute.For<IStartable>();
            transitiveStartable.Dependencies.Returns(new HashSet<IStartable> { transitiveOfTransitiveStartable });

            var startable = Substitute.For<IStartable>();
            startable.Dependencies.Returns(new HashSet<IStartable> { transitiveStartable });

            await using (
                var container = new ContainerBuilder<GenericContainer>(TestImages.TINY_IMAGE)
                    .WithStartupCheckStrategy(new OneShotStartupCheckStrategy())
                    .DependsOn(startable)
                    .Build()
            )
            {
                await container.Start();
                await container.Stop();
            }

            _ = startable.ReceivedWithAnyArgs(1).Start();// VisibleAssertions.assertEquals("Root started", 1, startable.StartInvocationCount
            _ = transitiveStartable.ReceivedWithAnyArgs(1).Start();// VisibleAssertions.assertEquals("Transitive started", 1, transitiveStartable.StartInvocationCount
            _ = transitiveOfTransitiveStartable.ReceivedWithAnyArgs(1).Start();// VisibleAssertions.assertEquals("Transitive of transitive started", 1, transitiveOfTransitiveStartable.StartInvocationCount
        }

        [Fact]
        public async Task ShouldHandleDiamondDependencies()
        {
            var a = Substitute.For<IStartable>();
            var b = Substitute.For<IStartable>();
            var c = Substitute.For<IStartable>();
            var d = Substitute.For<IStartable>();
            //  / b \
            // a     d
            //  \ c /
            b.Dependencies.Returns(new HashSet<IStartable> { a });
            c.Dependencies.Returns(new HashSet<IStartable> { a });

            d.Dependencies.Returns(new HashSet<IStartable> { b, c });

            await Startables.DeepStart(new[] { d });//.get(1, TimeUnit.SECONDS);

            _ = a.ReceivedWithAnyArgs(1).Start();
            _ = b.ReceivedWithAnyArgs(1).Start();
            _ = c.ReceivedWithAnyArgs(1).Start();
            _ = d.ReceivedWithAnyArgs(1).Start();
        }

        [Fact]
        public async Task ShouldHandleParallelStream()
        {
            var startables = Enumerable.Range(0, 10).Select(x => Substitute.For<IStartable>()).ToList();

            startables[0].Dependencies.Returns(new HashSet<IStartable>(startables.Skip(1).ToList()));

            await Startables.DeepStart(startables.AsParallel());//.get(1, TimeUnit.SECONDS);
        }

    }
}
