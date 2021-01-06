using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Polly;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Core.Containers;
using TestContainers.Images.Builder;
using Xunit;

namespace TestContainers.Tests.Containers.Wait.Internal
{
    public class InternalCommandPortListeningCheckTest
    {
        private GenericContainer _container;

        public InternalCommandPortListeningCheckTest(string dockerfile)
        {
            _container = new ContainerBuilder<GenericContainer>(new ImageFromDockerfile()
                .WithFileFromClasspath("Dockerfile", dockerfile)
                .WithFileFromClasspath("nginx.conf", "internal-port-check-dockerfile/nginx.conf")
                .GetTask()
            ).Build();
        }

        [Fact]
        public async Task SingleListening()
        {
            var check = new InternalCommandPortListeningCheck(_container, ImmutableHashSet.Create(8080));


            var p = Policy.TimeoutAsync(5).WrapAsync(
         Policy.HandleResult(false).WaitAndRetryForeverAsync(x => TimeSpan.FromSeconds(1))); //Unreliables.retryUntilTrue(5, TimeUnit.SECONDS, check);

            await p.ExecuteAsync(() => check.Invoke());

            var result = await check.Invoke();

            Assert.True(result); //"InternalCommandPortListeningCheck identifies a single listening port"
        }

        [Fact]
        public async Task NonListening()
        {
            var check = new InternalCommandPortListeningCheck(_container, ImmutableHashSet.Create(8080, 1234));

            var p = Policy.TimeoutAsync(5).WrapAsync(
         Policy.HandleResult(false).WaitAndRetryForeverAsync(x => TimeSpan.FromSeconds(1))); //Unreliables.retryUntilTrue(5, TimeUnit.SECONDS, check);

            try
            {
                await p.ExecuteAsync(() => check.Invoke());
            }
            catch (TimeoutException e)
            {
            }

            var result = await check.Invoke();

            Assert.False(result);// "InternalCommandPortListeningCheck detects a non-listening port among many"
        }

        [Fact]
        public async Task LowAndHighPortListening()
        {
            var check = new InternalCommandPortListeningCheck(_container, ImmutableHashSet.Create(100, 8080));

            var p = Policy.TimeoutAsync(5).WrapAsync(
         Policy.HandleResult(false).WaitAndRetryForeverAsync(x => TimeSpan.FromSeconds(1))); //Unreliables.retryUntilTrue(5, TimeUnit.SECONDS, check);


            await p.ExecuteAsync(() => check.Invoke());

            var result = await check.Invoke();

            Assert.True(result);// "InternalCommandPortListeningCheck identifies a low and a high port", result);
        }
    }
}
