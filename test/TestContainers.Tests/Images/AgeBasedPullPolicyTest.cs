using System;
using System.Collections.Generic;
using System.Text;
using TestContainers.Images;
using Xunit;

namespace TestContainers.Tests.Images
{
    public class AgeBasedPullPolicyTest
    {
        private readonly DockerImageName _imageName = DockerImageName.Parse(Guid.NewGuid().ToString("N"));

        [Fact]
        public void ShouldPull()
        {
            var imageData = ImageData.Builder
                .CreatedAt(DateTimeOffset.Now.AddHours(-2))
                .Build();

            var oneHour = new AgeBasedPullPolicy(TimeSpan.FromHours(1));

            Assert.True(oneHour.ShouldPullCached(_imageName, imageData));

            var fiveHours = new AgeBasedPullPolicy(TimeSpan.FromHours(5));
            Assert.False(fiveHours.ShouldPullCached(_imageName, imageData));
        }
    }
}
