using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace TestContainers.Utility
{
    public class MountableFile : ITransferable
    {
        private readonly string _path;

        private MountableFile(string path)
        {
            _path = path;
        }
        public int FileMode => throw new NotImplementedException();

        public long Size => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string FileSystemPath { get; internal set; }

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
            //return ForHostPath(Paths.get(path), mode);
            return null;
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

        private static string UnencodeResourceURIToFilePath(string resource)
        {

            // Convert any url-encoded characters (e.g. spaces) back into unencoded form
            return WebUtility.UrlDecode(resource.Replace("+", "%2B"))
                    .ReplaceFirst("jar:", "")
                    .ReplaceFirst("file:", "")
                    .Replace("!.*", "");

        }

        public byte[] GetBytes() => File.ReadAllBytes(_path);
        
    }
}
