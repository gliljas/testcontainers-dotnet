using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TestContainers.Utility;
using Xunit;

namespace TestContainers.Tests
{
    public class MountableFileTest
    {

        private static readonly int TEST_FILE_MODE = 0532;
        private static readonly int BASE_FILE_MODE = 0100000;
        private static readonly int BASE_DIR_MODE = 0040000;

        [Fact]
        public void ForClasspathResource()
        {
            var mountableFile = MountableFile.ForClasspathResource("mappable-resource/test-resource.txt");

            PerformChecks(mountableFile);
        }

        [Fact]
        public void ForClasspathResourceWithAbsolutePath()
        {
            var mountableFile = MountableFile.ForClasspathResource("/mappable-resource/test-resource.txt");

            PerformChecks(mountableFile);
        }

        [Fact]
        public void ForClasspathResourceFromJar()
        {
            var mountableFile = MountableFile.ForClasspathResource("META-INF/dummy_unique_name.txt");

            PerformChecks(mountableFile);
        }

        [Fact]
        public void ForClasspathResourceFromJarWithAbsolutePath()
        {
            var mountableFile = MountableFile.ForClasspathResource("/META-INF/dummy_unique_name.txt");

            PerformChecks(mountableFile);
        }

        [Fact]
        public void ForHostPath()
        {
            var file = CreateTempFile("somepath");
            var mountableFile = MountableFile.ForHostPath(file.toString());

            PerformChecks(mountableFile);
        }

        [Fact]
        public void ForHostPathWithSpaces()
        {
            var file = CreateTempFile("some path");
            var mountableFile = MountableFile.ForHostPath(file.toString());

            PerformChecks(mountableFile);

            assertTrue("The resolved path contains the original space", mountableFile.getResolvedPath().contains(" "));
            assertFalse("The resolved path does not contain an escaped space", mountableFile.getResolvedPath().contains("\\ "));
        }

        [Fact]
        public void ForHostPathWithPlus()
        {
            var file = CreateTempFile("some+path");
            var mountableFile = MountableFile.ForHostPath(file.toString());

            PerformChecks(mountableFile);

            assertTrue("The resolved path contains the original space", mountableFile.getResolvedPath().contains("+"));
            assertFalse("The resolved path does not contain an escaped space", mountableFile.getResolvedPath().contains(" "));
        }

        [Fact]
        public void ForClasspathResourceWithPermission()
        {
            var mountableFile = MountableFile.ForClasspathResource("mappable-resource/test-resource.txt",
                TEST_FILE_MODE);

            performChecks(mountableFile);
            assertEquals("Valid file mode.", BASE_FILE_MODE | TEST_FILE_MODE, mountableFile.getFileMode());
        }

        [Fact]
        public void ForHostFilePathWithPermission()
        {
            var file = CreateTempFile("somepath");
            var mountableFile = MountableFile.ForHostPath(file.toString(), TEST_FILE_MODE);
            performChecks(mountableFile);
            assertEquals("Valid file mode.", BASE_FILE_MODE | TEST_FILE_MODE, mountableFile.getFileMode());
        }

        [Fact]
        public void ForHostDirPathWithPermission()
        {
            var dir = createTempDir();
            var mountableFile = MountableFile.ForHostPath(dir.toString(), TEST_FILE_MODE);
            performChecks(mountableFile);
            assertEquals("Valid dir mode.", BASE_DIR_MODE | TEST_FILE_MODE, mountableFile.getFileMode());
        }

        [Fact]
        public void NoTrailingSlashesInTarEntryNames()
        {
            var mountableFile = MountableFile.ForClasspathResource("mappable-resource/test-resource.txt");

            @Cleanup final TarArchiveInputStream tais = intoTarArchive((taos)-> {
                mountableFile.transferTo(taos, "/some/path.txt");
                mountableFile.transferTo(taos, "/path.txt");
                mountableFile.transferTo(taos, "path.txt");
            });

            ArchiveEntry entry;
            while ((entry = tais.getNextEntry()) != null)
            {
                assertFalse("no entries should have a trailing slash", entry.getName().endsWith("/"));
            }
        }

        private TarArchiveInputStream IntoTarArchive(Consumer<TarArchiveOutputStream> consumer)
        {
            @Cleanup final ByteArrayOutputStream baos = new ByteArrayOutputStream();
            @Cleanup final TarArchiveOutputStream taos = new TarArchiveOutputStream(baos);
            consumer.accept(taos);
            taos.close();

            return new TarArchiveInputStream(new ByteArrayInputStream(baos.toByteArray()));
        }

        //@SuppressWarnings("ResultOfMethodCallIgnored")
        //@NotNull
        private FileInfo CreateTempFile(var name)
        {
            var tempParentDir = File.CreateTempFile("testcontainers", "");
            tempParentDir.delete();
            tempParentDir.mkdirs();
            var file = new FileInfo(Path.Combine(tempParentDir, name));

            File.Copy(MountableFileTest.class.getResourceAsStream("/mappable-resource/test-resource.txt"), file);
return file;
    }

    //@NotNull
    private Path CreateTempDir()
    {
        return Files.createTempDirectory("testcontainers");
    }

    private void PerformChecks(var mountableFile)
    {
        var mountablePath = mountableFile.getResolvedPath();
        assertTrue("The filesystem path '" + mountablePath + "' can be found", new File(mountablePath).exists());
        assertFalse("The filesystem path '" + mountablePath + "' does not contain any URL escaping", mountablePath.contains("%20"));
    }

}

}
