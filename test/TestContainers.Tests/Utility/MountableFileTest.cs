using System;
using System.IO;
using TestContainers.Utility;
using Xunit;
using FluentAssertions;

namespace TestContainers.Tests.Utility
{
    public class MountableFileTest
    {

        private static readonly int TEST_FILE_MODE = 0532;
        private static readonly int BASE_FILE_MODE = 0100000;
        private static readonly int BASE_DIR_MODE = 0040000;

        //[Fact]
        //public void ForClasspathResource()
        //{
        //    var mountableFile = MountableFile.ForClasspathResource("mappable-resource/test-resource.txt");

        //    PerformChecks(mountableFile);
        //}

        //[Fact]
        //public void ForClasspathResourceWithAbsolutePath()
        //{
        //    var mountableFile = MountableFile.ForClasspathResource("/mappable-resource/test-resource.txt");

        //    PerformChecks(mountableFile);
        //}

        //[Fact]
        //public void ForClasspathResourceFromJar()
        //{
        //    var mountableFile = MountableFile.ForClasspathResource("META-INF/dummy_unique_name.txt");

        //    PerformChecks(mountableFile);
        //}

        //[Fact]
        //public void ForClasspathResourceFromJarWithAbsolutePath()
        //{
        //    var mountableFile = MountableFile.ForClasspathResource("/META-INF/dummy_unique_name.txt");

        //    PerformChecks(mountableFile);
        //}

        [Fact]
        public void ForHostPath()
        {
            var file = CreateTempFile("somepath");
            var mountableFile = MountableFile.ForHostPath(file.FullName);

            PerformChecks(mountableFile);
        }

        [Fact]
        public void ForHostPathWithSpaces()
        {
            var file = CreateTempFile("some path");
            var mountableFile = MountableFile.ForHostPath(file.FullName);

            PerformChecks(mountableFile);

            mountableFile.ResolvedPath.Should().Contain(" ", "The resolved path contains the original space");
            mountableFile.ResolvedPath.Should().NotContain("\\ ", "The resolved path does not contain an escaped space");
        }

        [Fact]
        public void ForHostPathWithPlus()
        {
            var file = CreateTempFile("some+path");
            var mountableFile = MountableFile.ForHostPath(file.FullName);

            PerformChecks(mountableFile);

            mountableFile.ResolvedPath.Should().Contain("+", "The resolved path contains the original space");
            mountableFile.ResolvedPath.Should().NotContain(" ", "The resolved path does not contain an escaped space");
        }

        //[Fact]
        //public void ForClasspathResourceWithPermission()
        //{
        //    var mountableFile = MountableFile.ForClasspathResource("mappable-resource/test-resource.txt",
        //        TEST_FILE_MODE);

        //    PerformChecks(mountableFile);
        //    mountableFile.FileMode.Should().Be(BASE_FILE_MODE | TEST_FILE_MODE, "the file mode should be valid");
        //}

        [Fact]
        public void ForHostFilePathWithPermission()
        {
            var file = CreateTempFile("somepath");
            var mountableFile = MountableFile.ForHostPath(file.FullName, TEST_FILE_MODE);
            PerformChecks(mountableFile);
            mountableFile.FileMode.Should().Be(BASE_FILE_MODE | TEST_FILE_MODE, "the file mode should be valid");
        }

        [Fact]
        public void ForHostDirPathWithPermission()
        {
            var dir = CreateTempDir();
            var mountableFile = MountableFile.ForHostPath(dir.FullName, TEST_FILE_MODE);
            PerformChecks(mountableFile);
            mountableFile.FileMode.Should().Be(BASE_DIR_MODE | TEST_FILE_MODE, "the file mode should be valid");
        }

        //[Fact]
        //public void NoTrailingSlashesInTarEntryNames()
        //{
        //    var mountableFile = MountableFile.ForClasspathResource("mappable-resource/test-resource.txt");

        //    using (var TarArchiveInputStream tais = IntoTarArchive((taos)=> {
        //        mountableFile.TransferTo(taos, "/some/path.txt");
        //        mountableFile.TransferTo(taos, "/path.txt");
        //        mountableFile.TransferTo(taos, "path.txt");
        //    });

        //    ArchiveEntry entry;
        //    while ((entry = tais.getNextEntry()) != null)
        //    {
        //        assertFalse("no entries should have a trailing slash", entry.getName().endsWith("/"));
        //    }
        //}

        //private TarArchiveInputStream IntoTarArchive(Action<TarArchiveOutputStream> consumer)
        //{
        //    using var baos = new ByteArrayOutputStream();
        //    using var taos = new TarArchiveOutputStream(baos);
        //    consumer(taos);
        //    taos.close();

        //    return new TarArchiveInputStream(new ByteArrayInputStream(baos.toByteArray()));
        //}

        //@SuppressWarnings("ResultOfMethodCallIgnored")
        //@NotNull
        private FileInfo CreateTempFile(string name)
        {
            var tempParentDir = FileUtil.CreateTempFile("testcontainers", "");
            tempParentDir.Delete();
            var file = new FileInfo(Path.Combine(tempParentDir.FullName, name));
            Directory.CreateDirectory(tempParentDir.FullName);
            File.Copy("Resources/mappable-resource/test-resource.txt", file.FullName, true);
            return file;
        }

        //@NotNull
        private DirectoryInfo CreateTempDir()
        {
            return FileUtil.CreateTempDirectory("testcontainers");
        }

        private void PerformChecks(MountableFile mountableFile)
        {
            var mountablePath = mountableFile.ResolvedPath;
            (new DirectoryInfo(mountablePath).Exists || new FileInfo(mountablePath).Exists).Should().BeTrue("the filesystem path '" + mountablePath + "' should exist");
            mountablePath.Should().NotContain("%20", "The filesystem path '" + mountablePath + "' does not contain any URL escaping");
        }

    }

    internal class FileUtil
    {
        internal static DirectoryInfo CreateTempDirectory(string prefix)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{prefix}{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            return new DirectoryInfo(path);
        }

        internal static FileInfo CreateTempFile(string prefix, string suffix)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{prefix}{Guid.NewGuid():N}{suffix}");

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllText(path, "");

            return new FileInfo(path);
        }
    }

}
