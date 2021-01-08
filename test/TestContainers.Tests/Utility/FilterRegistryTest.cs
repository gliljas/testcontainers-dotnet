using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using static TestContainers.Utility.ResourceReaper;

namespace TestContainers.Tests.Utility
{
    public class FilterRegistryTest
    {
        private static readonly List<KeyValuePair<string, string>> FILTERS = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string,string>("key1!", "value2?"), new KeyValuePair<string,string>("key2#", "value2%")
        };
        private static readonly string URL_ENCODED_FILTERS = "key1%21=value2%3F&key2%23=value2%25";
        private static readonly byte[] ACKNOWLEDGEMENT = Encoding.UTF8.GetBytes(FilterRegistry.ACKNOWLEDGMENT);
        private static readonly byte[] NO_ACKNOWLEDGEMENT = Encoding.UTF8.GetBytes("");
        private static readonly string NEW_LINE = "\n";

        [Fact]
        public void RegisterReturnsTrueIfAcknowledgementIsReadFromInputStream()
        {
            FilterRegistry registry = new FilterRegistry(InputStream(ACKNOWLEDGEMENT), AnyOutputStream());

            var successful = registry.Register(FILTERS);

            successful.Should().BeTrue();
        }

        [Fact]
        public void RegisterReturnsFalseIfNoAcknowledgementIsReadFromInputStream()
        {
            FilterRegistry registry = new FilterRegistry(InputStream(NO_ACKNOWLEDGEMENT), AnyOutputStream());

            var successful = registry.Register(FILTERS);

            successful.Should().BeFalse();
        }

        [Fact]
        public void RegisterWritesUrlEncodedFiltersAndNewlineToOutputStream()
        {
            var outputStream = new MemoryStream();
            var registry = new FilterRegistry(AnyInputStream(), outputStream);

            registry.Register(FILTERS);

            Encoding.UTF8.GetString(outputStream.ToArray()).Should().Be(URL_ENCODED_FILTERS + NEW_LINE);
        }

        private static MemoryStream InputStream(byte[] bytes)
        {
            return new MemoryStream(bytes);
        }

        private static MemoryStream AnyInputStream()
        {
            return new MemoryStream(ACKNOWLEDGEMENT);
        }

        private static MemoryStream AnyOutputStream()
        {
            return new MemoryStream();
        }
    }
}
