using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Mono.Unix;

namespace TestContainers.Utility
{
    public class MountableFile : AbstractTransferable
    {
        private static readonly string TESTCONTAINERS_TMP_DIR_PREFIX = ".testcontainers-tmp-";
        private static readonly string OS_MAC_TMP_DIR = "/tmp";
        private static readonly int BASE_FILE_MODE = 0100000;
        private static readonly int BASE_DIR_MODE = 0040000;
        private readonly string _path;
        private readonly Lazy<string> _resolvedPath;
        private int? _forcedFileMode;

        internal MountableFile(string path, int mode)
        {
            _path = path;
            _forcedFileMode = mode;
            _resolvedPath = new Lazy<string>(() => ResolvePath());
        }

        public string ResolvedPath => _resolvedPath.Value;
        
        private int GetUnixFileMode(string pathAsString)
        {

            var path = new FileInfo(pathAsString);

            if (_forcedFileMode.HasValue)
            {
                return GetModeValue(path);
            }
            return GetUnixFileMode(path);
        }

        private int GetModeValue(FileInfo path)
        {
            int result = Directory.Exists(path.FullName) ? BASE_DIR_MODE : BASE_FILE_MODE;
            return result | (_forcedFileMode ?? 0);
        }

        private int GetUnixFileMode(FileInfo path)
        {
            //try
            //{
            //    new UnixFileInfo(path.FullName).Protection
            //    var acl = path.;

            //    return 0;
            //}
            //catch (IOException)// | UnsupportedOperationException e) {
            //{    // fallback for non-posix environments
                int mode = AbstractTransferable.DEFAULT_FILE_MODE;

                if (Directory.Exists(path.FullName))
                {
                    mode = DEFAULT_DIR_MODE;
                }
                //else if (Files.isExecutable(path))
                //{
                //    mode |= 0111; // equiv to +x for user/group/others
                //}

                return mode;
           // }
        }

        internal static MountableFile ForClasspathResource(string v)
        {
            throw new NotImplementedException();
        }

        public override string Description => throw new NotImplementedException();

        public string FileSystemPath { get; internal set; }

        public override int FileMode => GetUnixFileMode(ResolvedPath);

        public override long Size => File.Exists(ResolvedPath) ? new FileInfo(ResolvedPath).Length : 0;

        
        //private MountableFile() //: base()
        //{

        //}
        /**
     * Obtains a {@link MountableFile} corresponding to a file on the docker host filesystem.
     *
     * @param path the path to the resource
     * @return a {@link MountableFile} that may be used to obtain a mountable path
     */
        public static MountableFile ForHostPath(string path)
        {
            return ForHostPath(path, 0);
        }

        /**
     * Obtains a {@link MountableFile} corresponding to a file on the docker host filesystem.
     *
     * @param path the path to the resource
     * @param mode octal value of posix file mode (000..777)
     * @return a {@link MountableFile} that may be used to obtain a mountable path
     */
        public static MountableFile ForHostPath(string path, int mode)
        {
            return ForHostPath(new FileInfo(path), mode);
        }

        public static MountableFile ForHostPath(FileInfo fileInfo, int mode)
        {
            return new MountableFile(fileInfo.FullName, mode);
        }

        /**
   * Obtain a path that the Docker daemon should be able to use to volume mount a file/resource
   * into a container. If this is a classpath resource residing in a JAR, it will be extracted to
   * a temporary location so that the Docker daemon is able to access it.
   *
   * @return a volume-mountable path.
   */
        public string ResolvePath()
        {
            string result = GetResourcePath();

            // Special case for Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && result.StartsWith("/"))
            {
                // Remove leading /
                result = result.Substring(1);
            }

            return result;
        }

        private string GetResourcePath()
        {
            //if (path.contains(".jar!"))
            //{
            //    resourcePath = extractClassPathResourceToTempLocation(this.path);
            //}
            //else
            //  {
            var resourcePath = UnencodeResourceURIToFilePath(_path);
            // }
            return resourcePath;
        }

        internal static MountableFile ForClasspathResource(string v, int tEST_FILE_MODE)
        {
            throw new NotImplementedException();
        }

        private static string UnencodeResourceURIToFilePath(string resource)
        {

            // Convert any url-encoded characters (e.g. spaces) back into unencoded form
            return WebUtility.UrlDecode(resource.Replace("+", "%2B"))
                    .ReplaceFirst("jar:", "")
                    .ReplaceFirst("file:", "")
                    .Replace("!.*", "");

        }

        public override byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class AbstractTransferable : ITransferable
    {
        internal static int DEFAULT_FILE_MODE = 0100644;
        internal static int DEFAULT_DIR_MODE = 040755;



        public abstract int FileMode { get; }

        public abstract long Size { get; }

        public abstract string Description { get; }

        public abstract byte[] GetBytes();
    }
}
