using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using TestContainers.Containers;
using TestContainers.Images;
using Xunit;

namespace TestContainers.Tests.Utility
{
    public class DockerImageNameCompatibilityTest
    {
        [Fact]
        public void TestPlainImage()
        {
            var subject = DockerImageName.Parse("foo");

            subject.IsCompatibleWith(DockerImageName.Parse("bar")).Should().BeFalse("image name foo != bar");
        }
        [Fact]
        public void TestNoTagTreatedAsWildcard()
        {
            var subject = DockerImageName.Parse("foo:4.5.6");
            /*
            foo:1.2.3 != foo:4.5.6
            foo:1.2.3 ~= foo

            The test is effectively making sure that 'no tag' is treated as a wildcard
             */
            subject.IsCompatibleWith(DockerImageName.Parse("foo:1.2.3")).Should().BeFalse("foo:4.5.6 != foo:1.2.3");
            subject.IsCompatibleWith(DockerImageName.Parse("foo")).Should().BeTrue("foo:4.5.6 ~= foo");
        }

        [Fact]
        public void TestImageWithAutomaticCompatibilityForFullPath()
        {
            var subject = DockerImageName.Parse("repo/foo:1.2.3");

            subject.IsCompatibleWith(DockerImageName.Parse("repo/foo")).Should().BeTrue("repo/foo:1.2.3 ~= repo/foo");
        }

        [Fact]
        public void TestImageWithClaimedCompatibility()
        {
            var subject = DockerImageName.Parse("foo").AsCompatibleSubstituteFor("bar");

            subject.IsCompatibleWith(DockerImageName.Parse("bar")).Should().BeTrue("foo(bar) ~= bar");
            subject.IsCompatibleWith(DockerImageName.Parse("fizz")).Should().BeFalse("foo(bar) != fizz");
        }

        [Fact]
        public void TestImageWithClaimedCompatibilityAndVersion()
        {
            var subject = DockerImageName.Parse("foo:1.2.3").AsCompatibleSubstituteFor("bar");

            subject.IsCompatibleWith(DockerImageName.Parse("bar")).Should().BeTrue("foo:1.2.3(bar) ~= bar");
        }

        [Fact]
        public void TestImageWithClaimedCompatibilityForFullPath()
        {
            var subject = DockerImageName.Parse("foo").AsCompatibleSubstituteFor("registry/repo/bar");

            subject.IsCompatibleWith(DockerImageName.Parse("registry/repo/bar")).Should().BeTrue("foo(registry/repo/bar) ~= registry/repo/bar");
            subject.IsCompatibleWith(DockerImageName.Parse("repo/bar")).Should().BeFalse("foo(registry/repo/bar) != repo/bar");
            subject.IsCompatibleWith(DockerImageName.Parse("bar")).Should().BeFalse("foo(registry/repo/bar) != bar");
        }

        [Fact]
        public void TestImageWithClaimedCompatibilityForVersion()
        {
            var subject = DockerImageName.Parse("foo").AsCompatibleSubstituteFor("bar:1.2.3");

            subject.IsCompatibleWith(DockerImageName.Parse("bar")).Should().BeTrue("foo(bar:1.2.3) ~= bar");
            subject.IsCompatibleWith(DockerImageName.Parse("bar:1.2.3")).Should().BeTrue("foo(bar:1.2.3) ~= bar:1.2.3");
            subject.IsCompatibleWith(DockerImageName.Parse("bar:0.0.1")).Should().BeFalse("foo(bar:1.2.3) != bar:0.0.1");
            subject.IsCompatibleWith(DockerImageName.Parse("bar:2.0.0")).Should().BeFalse("foo(bar:1.2.3) != bar:2.0.0");
            subject.IsCompatibleWith(DockerImageName.Parse("bar:1.2.4")).Should().BeFalse("foo(bar:1.2.3) != bar:1.2.4");
        }

        [Fact]
        public void TestAssertMethodAcceptsCompatible()
        {
            var subject = DockerImageName.Parse("foo").AsCompatibleSubstituteFor("bar");
            subject.AssertCompatibleWith(DockerImageName.Parse("bar"));
        }

        [Fact]
        public void TestAssertMethodRejectsIncompatible()
        {
            var subject = DockerImageName.Parse("foo");
            subject.Invoking(x => x.AssertCompatibleWith(DockerImageName.Parse("bar")))
                    .Should().Throw<IllegalStateException>()
                    .And.Message.Should().Contain("Failed to verify that image 'foo' is a compatible substitute for 'bar'");
        }
    }
}
