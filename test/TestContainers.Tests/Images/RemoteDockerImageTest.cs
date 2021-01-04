using System.Threading;
using System.Threading.Tasks;
using TestContainers.Containers;
using TestContainers.Images;
using Xunit;

namespace TestContainers.Tests.Images
{
    public class RemoteDockerImageTest
    {
        [Fact]
        public void ToStringContainsOnlyImageName()
        {
            string imageName = Base58.RandomString(8).ToLower();
            var remoteDockerImage = new RemoteDockerImage(DockerImageName.Parse(imageName));
            Assert.Contains("imageName=" + imageName, remoteDockerImage.ToString());
        }

        [Fact]
        public void ToStringWithExceptionContainsOnlyImageNameFuture()
        {
            var imageNameFuture = new TaskCompletionSource<string>();
            imageNameFuture.SetException(new RuntimeException("arbitrary"));

            RemoteDockerImage remoteDockerImage = new RemoteDockerImage(imageNameFuture.Task);
            Assert.Contains("imageName=java.lang.RuntimeException: arbitrary", remoteDockerImage.ToString());
        }

        [Fact] //(timeout= 5000L)
        public void ToStringDoesntResolveImageNameFuture()
        {
            var imageNameFuture = new TaskCompletionSource<string>();

            // verify that we've set up the test properly
            Assert.False(imageNameFuture.Task.IsCompleted);

            RemoteDockerImage remoteDockerImage = new RemoteDockerImage(imageNameFuture.Task);

            Assert.Contains("imageName=<resolving>", remoteDockerImage.ToString());

            // Make sure the act of calling toString doesn't resolve the imageNameFuture
            Assert.False(imageNameFuture.Task.IsCompleted);

            string imageName = Base58.RandomString(8).ToLower();
            imageNameFuture.SetResult(imageName);
            Assert.Contains("imageName=" + imageName, remoteDockerImage.ToString());
        }

        [Fact]// (timeout= 5000L)
        public void ToStringDoesntResolveLazyFuture() //throws Exception
        {
            var imageName = Base58.RandomString(8).ToLower();
            var resolved = 0;
            var tcs = new TaskCompletionSource<int>();
            var imageNameFutureTask = tcs.Task.ContinueWith(x =>
            {
                Interlocked.Increment(ref resolved);
                return imageName;
            },TaskContinuationOptions.ExecuteSynchronously);

            // verify that we've set up the test properly
            Assert.False(imageNameFutureTask.IsCompleted);

            RemoteDockerImage remoteDockerImage = new RemoteDockerImage(imageNameFutureTask);
            Assert.Contains("imageName=<resolving>", remoteDockerImage.ToString());

            // Make sure the act of calling toString doesn't resolve the imageNameFuture
            Assert.False(imageNameFutureTask.IsCompleted);
            Assert.Equal(0, resolved);

            // Trigger resolve
            tcs.SetResult(1);
            Assert.Equal(1, resolved);
            Assert.Contains("imageName=" + imageName, remoteDockerImage.ToString());
        }
    }
}
