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
    public class PrefixingImageNameSubstitutorTest
    {
        private TestContainersConfiguration _mockConfiguration;
        private PrefixingImageNameSubstitutor _underTest;

        public PrefixingImageNameSubstitutorTest()
        {
            _mockConfiguration = Substitute.For<TestContainersConfiguration>();
            _underTest = new PrefixingImageNameSubstitutor(_mockConfiguration);
        }

        [Fact]
        public async Task TestHappyPath()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com/our-mirror/");

            var result = await _underTest.Apply(DockerImageName.Parse("some/image:tag"));

            result.AsCanonicalNameString().Should().Be("someregistry.com/our-mirror/some/image:tag", "the prefix should be applied");
        }

        [Fact]
        public async Task HubIoRegistryIsNotChanged()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com/our-mirror/");

            var result = await _underTest.Apply(DockerImageName.Parse("docker.io/some/image:tag"));

            result.AsCanonicalNameString().Should().Be("docker.io/some/image:tag", "the prefix should be applied");
        }

        [Fact]
        public async Task HubComRegistryIsNotChanged()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com/our-mirror/");

            var result = await _underTest.Apply(DockerImageName.Parse("registry.hub.docker.com/some/image:tag"));

            result.AsCanonicalNameString().Should().Be("registry.hub.docker.com/some/image:tag", "the prefix should be applied");
        }

        [Fact]
        public async Task ThirdPartyRegistriesNotAffected()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com/our-mirror/");

            var result = await _underTest.Apply(DockerImageName.Parse("gcr.io/something/image:tag"));

            result.AsCanonicalNameString().Should().Be("gcr.io/something/image:tag", "the prefix should not be not applied if a third party registry is used");
        }

        [Fact]
        public async Task TestNoDoublePrefixing()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com/our-mirror/");

            var result = await _underTest.Apply(DockerImageName.Parse("someregistry.com/some/image:tag"));

            result.AsCanonicalNameString().Should().Be("someregistry.com/some/image:tag", "the prefix should not be applied if already present");
        }

        [Fact]
        public async Task TestHandlesEmptyValue()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("");

            var result = await _underTest.Apply(DockerImageName.Parse("some/image:tag"));

            result.AsCanonicalNameString().Should().Be("some/image:tag", "the prefix should not be applied if the env var is not set");
        }

        [Fact]
        public async Task TestHandlesRegistryOnlyWithTrailingSlash()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com/");

            var result = await _underTest.Apply(DockerImageName.Parse("some/image:tag"));

            result.AsCanonicalNameString().Should().Be("someregistry.com/some/image:tag", "the prefix should be applied");
        }

        [Fact]
        public async Task TestCombinesLiterallyForRegistryOnlyWithoutTrailingSlash()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com");

            var result = await _underTest.Apply(DockerImageName.Parse("some/image:tag"));

            result.AsCanonicalNameString().Should().Be("someregistry.comsome/image:tag", "the prefix should be treated literally, for predictability");
        }

        [Fact]
        public async Task TestCombinesLiterallyForBothPartsWithoutTrailingSlash()
        {
            _mockConfiguration.GetEnvVarOrProperty(PrefixingImageNameSubstitutor.PREFIX_PROPERTY_KEY, Arg.Any<string>()).Returns("someregistry.com/our-mirror");

            var result = await _underTest.Apply(DockerImageName.Parse("some/image:tag"));

            result.AsCanonicalNameString().Should().Be("someregistry.com/our-mirrorsome/image:tag", "the prefix should be treated literally, for predictability");
        }
    }
}
