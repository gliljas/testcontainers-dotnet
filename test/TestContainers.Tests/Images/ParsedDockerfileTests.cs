using System;
using System.Collections.Generic;
using System.Text;
using TestContainers.Containers;
using Xunit;

namespace TestContainers.Tests.Images
{
    public class ParsedDockerfileTests
    {
        [Fact]
        public void DoesSimpleParsing()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "FROM someimage", "RUN something" });
            Assert.Equal(new HashSet<string> { "someimage" }, parsedDockerfile.DependencyImageNames);//, "extracts a single image name"
        }

        [Fact]
        public void IsCaseInsensitive()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "from someimage", "RUN something" });
            Assert.Equal(new HashSet<string> { "someimage"}, parsedDockerfile.DependencyImageNames); //"extracts a single image name", 
        }

        [Fact]
        public void HandlesTags()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "FROM someimage:tag", "RUN something" });
            Assert.Equal(new HashSet<string> { "someimage:tag"}, parsedDockerfile.DependencyImageNames); //"retains tags in image names", 
        }

        [Fact]
        public void HandlesDigests()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "FROM someimage@sha256:abc123", "RUN something" });
            Assert.Equal(new HashSet<string> { "someimage@sha256:abc123"}, parsedDockerfile.DependencyImageNames); //"retains digests in image names", 
        }

        [Fact]
        public void IgnoringCommentedFromLines()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "FROM someimage", "#FROM somethingelse" });
            Assert.Equal(new HashSet<string> { "someimage"}, parsedDockerfile.DependencyImageNames); //"ignores commented from lines", 
        }

        [Fact]
        public void IgnoringBuildStageNames()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "FROM someimage --as=base", "RUN something", "FROM nextimage", "RUN something" });
            Assert.Equal(new HashSet<string> { "someimage", "nextimage"}, parsedDockerfile.DependencyImageNames); //"ignores build stage names and allows multiple images to be extracted"
        }

        [Fact]
        public void IgnoringPlatformArgs()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "FROM --platform=linux/amd64 someimage", "RUN something" });
            Assert.Equal(new HashSet<string> { "someimage"}, parsedDockerfile.DependencyImageNames);//"ignores platform args", 
        }

        [Fact]
        public void IgnoringExtraPlatformArgs()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "FROM --platform=linux/amd64 --somethingelse=value someimage", "RUN something" });
            Assert.Equal(new HashSet<string> { "someimage"}, parsedDockerfile.DependencyImageNames); //"ignores platform args", 
        }

        [Fact]
        public void HandlesGracefullyIfNoFromLine()
        {
            var parsedDockerfile = new ParsedDockerfile(new[] { "RUN something", "# is this even a valid Dockerfile?" });
            Assert.Equal(new HashSet<string> { }, parsedDockerfile.DependencyImageNames); //"handles invalid Dockerfiles gracefully", 
        }

        [Fact]
        public void HandlesGracefullyIfDockerfileNotFound()
        {
            var parsedDockerfile = new ParsedDockerfile("nonexistent.Dockerfile");
            Assert.Equal(new HashSet<string> { }, parsedDockerfile.DependencyImageNames); //"handles missing Dockerfiles gracefully", 
        }
    }
}
