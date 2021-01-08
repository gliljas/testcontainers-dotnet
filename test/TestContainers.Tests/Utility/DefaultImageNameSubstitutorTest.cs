using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TestContainers.Core.Containers;
using TestContainers.Images;
using TestContainers.Utility;
using Xunit;

namespace TestContainers.Tests.Utility
{
    public class DefaultImageNameSubstitutorTest
    {
        public static readonly DockerImageName ORIGINAL_IMAGE = DockerImageName.Parse("foo");
        public static readonly DockerImageName SUBSTITUTE_IMAGE = DockerImageName.Parse("bar");
        private readonly TestContainersConfiguration _mockConfiguration;
        private readonly ConfigurationFileImageNameSubstitutor _underTest;

        public DefaultImageNameSubstitutorTest()
        {
            _mockConfiguration = Substitute.For<TestContainersConfiguration>();
            _underTest = new ConfigurationFileImageNameSubstitutor(_mockConfiguration);
        }

        [Fact]
        public async Task TestConfigurationLookup()
        {
            _mockConfiguration.GetConfiguredSubstituteImage(ORIGINAL_IMAGE).Returns(SUBSTITUTE_IMAGE);

            var substitute = await _underTest.Apply(ORIGINAL_IMAGE);

            substitute.Should().Be(SUBSTITUTE_IMAGE, "a match should be found");
            substitute.IsCompatibleWith(ORIGINAL_IMAGE).Should().BeTrue();
            //assertEquals("match is found", SUBSTITUTE_IMAGE, substitute);
            //assertTrue("compatibility is automatically set", substitute.isCompatibleWith(ORIGINAL_IMAGE));
        }
    }
}
