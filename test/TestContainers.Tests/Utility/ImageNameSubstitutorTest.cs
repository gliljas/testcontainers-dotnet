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
    public class ImageNameSubstitutorTest  : IDisposable
    {
        //    @Rule
        //public MockTestcontainersConfigurationRule config = new MockTestcontainersConfigurationRule();
        private ImageNameSubstitutor _originalInstance;
        private TestContainersConfiguration _originalConfiguration;
        private TestContainersConfiguration _configuration;
        private readonly ImageNameSubstitutor _originalDefaultImplementation;

        //    private ImageNameSubstitutor originalDefaultImplementation;

        public ImageNameSubstitutorTest()
        {
            _originalInstance = ImageNameSubstitutor.Instance;
            _originalConfiguration = TestContainersConfiguration.Instance;
            _configuration = Substitute.For<TestContainersConfiguration>();
            _originalDefaultImplementation = ImageNameSubstitutor._defaultImplementation;
            TestContainersConfiguration.Instance = _configuration;
            ImageNameSubstitutor._instance = null;
            ImageNameSubstitutor._defaultImplementation = Substitute.For<ImageNameSubstitutor>();

            ImageNameSubstitutor._defaultImplementation
                .Apply(DockerImageName.Parse("original"))
                .Returns(DockerImageName.Parse("substituted-image"));

       //     ImageNameSubstitutor._defaultImplementation
       //         .Description
       //         .Returns("default implementation");
        }

        //private class Bobo : ImageNameSubstitutor
        //{
        //    protected override string Description => GetDescription();

        //    public override Task<DockerImageName> Apply(DockerImageName original)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public void Dispose()
        {
            TestContainersConfiguration.Instance = _originalConfiguration;
            ImageNameSubstitutor._instance = _originalInstance;
            ImageNameSubstitutor._defaultImplementation = _originalDefaultImplementation;
        }

        [Fact]
        public async Task SimpleConfigurationTest()
        {
            _configuration.ImageSubstitutorClassName.Returns(typeof(FakeImageSubstitutor).AssemblyQualifiedName);
           
            var imageNameSubstitutor = ImageNameSubstitutor.Instance;

            var result = await imageNameSubstitutor.Apply(DockerImageName.Parse("original"));
            result.AsCanonicalNameString().Should().Be(
                "transformed-substituted-image:latest",
                "the image has been substituted by default then configured implementations"
            );
        }

        [Fact]
        public async Task TestWorksWithoutConfiguredImplementation()
        {
            //Mockito
            //    .doReturn(null)
            //    .when(TestcontainersConfiguration.getInstance())
            //    .getImageSubstitutorClassName();

            var imageNameSubstitutor = ImageNameSubstitutor.Instance;

            var result = await imageNameSubstitutor.Apply(DockerImageName.Parse("original"));
            result.AsCanonicalNameString().Should().Be(
                 "substituted-image:latest",
                "the image has been substituted by default then configured implementations"
            );
        }
    }
}
