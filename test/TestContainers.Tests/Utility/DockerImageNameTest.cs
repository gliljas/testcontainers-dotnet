using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using TestContainers.Images;
using Xunit;

namespace TestContainers.Tests.Utility
{
    public class DockerImageNameTest
    {
        [Theory]
        [InlineData("myname:latest")]
        [InlineData("repo/my-name:1.0")]
        [InlineData("registry.foo.com:1234/my-name:1.0")]
        [InlineData("registry.foo.com/my-name:1.0")]
        [InlineData("registry.foo.com:1234/repo_here/my-name:1.0")]
        [InlineData("registry.foo.com:1234/repo-here/my-name@sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("registry.foo.com:1234/my-name@sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("1.2.3.4/my-name:1.0")]
        [InlineData("1.2.3.4:1234/my-name:1.0")]
        [InlineData("1.2.3.4/repo-here/my-name:1.0")]
        [InlineData("1.2.3.4:1234/repo-here/my-name:1.0")]
        public void TestValidNameAccepted(string imageName)
        {
            Action act = () => DockerImageName.Parse(imageName).AssertValid();

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(":latest")]
        [InlineData("/myname:latest")]
        [InlineData("/myname@sha256:latest")]
        [InlineData("/myname@sha256:gggggggggggggggggggggggggggggggg")]
        [InlineData("repo:notaport/myname:latest")]
        public void TestInvalidNameRejected(string imageName)
        {
            Action act = () => DockerImageName.Parse(imageName).AssertValid();

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("", "", "myname", ":", null)]
        [InlineData("", "", "myname", ":", "latest")]
        [InlineData("", "", "repo/myname", ":", null)]
        [InlineData("", "", "repo/myname", ":", "latest")]
        [InlineData("registry.foo.com:1234", "/", "my-name", ":", null)]
        [InlineData("registry.foo.com:1234", "/", "my-name", ":", "1.0")]
        [InlineData("registry.foo.com", "/", "my-name", ":", "1.0")]
        [InlineData("registry.foo.com:1234", "/", "repo_here/my-name", ":", null)]
        [InlineData("registry.foo.com:1234", "/", "repo_here/my-name", ":", "1.0")]
        [InlineData("1.2.3.4:1234", "/", "repo_here/my-name", ":", null)]
        [InlineData("1.2.3.4:1234", "/", "repo_here/my-name", ":", "1.0")]
        [InlineData("1.2.3.4:1234", "/", "my-name", ":", null)]
        [InlineData("1.2.3.4:1234", "/", "my-name", ":", "1.0")]
        [InlineData("", "", "myname", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("", "", "repo/myname", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("registry.foo.com:1234", "/", "repo-here/my-name", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("registry.foo.com:1234", "/", "my-name", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("1.2.3.4", "/", "my-name", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("1.2.3.4:1234", "/", "my-name", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("1.2.3.4", "/", "my-name", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("1.2.3.4:1234", "/", "my-name", "@", "sha256:1234abcd1234abcd1234abcd1234abcd")]
        public void TestParsing(string registry, string registrySeparator, string repo, string versionSeparator, string version)
        {
            var unversionedPart = registry + registrySeparator + repo;

            String combined;
            String canonicalName;
            if (version != null)
            {
                combined = unversionedPart + versionSeparator + version;
                canonicalName = unversionedPart + versionSeparator + version;
            }
            else
            {
                combined = unversionedPart;
                canonicalName = unversionedPart + ":latest";
            }

            //VisibleAssertions.context("For " + combined);
            //VisibleAssertions.context("Using single-arg constructor:", 2);

            var imageName = DockerImageName.Parse(combined);
            imageName.Registry.Should().Be(registry, combined + " should have registry address: " + registry);
            imageName.UnversionedPart.Should().Be(unversionedPart, combined + " should have unversioned part: " + unversionedPart);
            if (version != null)
            {
                imageName.VersionPart.Should().Be(version, combined + " should have version part: " + version);
            }
            else
            {
                imageName.VersionPart.Should().Be("latest", combined + " should have automatic 'latest' version specified");
            }
            imageName.AsCanonicalNameString().Should().Be(canonicalName, combined + " should have canonical name: " + canonicalName);

            //if (version != null)
            //{
            //    //VisibleAssertions.context("Using two-arg constructor:", 2);

            //    var imageNameFromSecondaryConstructor = new DockerImageName(unversionedPart, version);
            //    imageNameFromSecondaryConstructor.Registry.Should().Be(registry, combined + " should have registry address: " + registry);
            //    imageNameFromSecondaryConstructor.UnversionedPart.Should().Be(combined + " should have unversioned part: " + unversionedPart);
            //    imageNameFromSecondaryConstructor.VersionPart.Should().Be(version,combined + " should have version part: " + version);
            //}
        }
    }
}

