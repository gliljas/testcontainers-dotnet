//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Runtime.InteropServices;
//using System.Text;
//using JetBrains.Annotations;

//namespace TestContainers.Utility
//{
//    public class MountableFile : Transferable
//    {
//        //private MountableFile() //: base()
//        //{

//        //}
//        /**
//     * Obtains a {@link MountableFile} corresponding to a file on the docker host filesystem.
//     *
//     * @param path the path to the resource
//     * @return a {@link MountableFile} that may be used to obtain a mountable path
//     */
//        public static MountableFile ForHostPath(string path)
//        {
//            return ForHostPath(path, null);
//        }

//        /**
//     * Obtains a {@link MountableFile} corresponding to a file on the docker host filesystem.
//     *
//     * @param path the path to the resource
//     * @param mode octal value of posix file mode (000..777)
//     * @return a {@link MountableFile} that may be used to obtain a mountable path
//     */
//        public static MountableFile ForHostPath(string path, int mode)
//        {
//            //return ForHostPath(Paths.get(path), mode);
//            return null;
//        }

//        /**
//   * Obtain a path that the Docker daemon should be able to use to volume mount a file/resource
//   * into a container. If this is a classpath resource residing in a JAR, it will be extracted to
//   * a temporary location so that the Docker daemon is able to access it.
//   *
//   * @return a volume-mountable path.
//   */
//        public string ResolvePath()
//        {
//            string result = GetResourcePath();

//            // Special case for Windows
//            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && result.StartsWith("/"))
//            {
//                // Remove leading /
//                result = result.Substring(1);
//            }

//            return result;
//        }

//        private string GetResourcePath()
//        {
//            //if (path.contains(".jar!"))
//            //{
//            //    resourcePath = extractClassPathResourceToTempLocation(this.path);
//            //}
//            //else
//            //  {
//            var resourcePath = UnencodeResourceURIToFilePath(path);
//            // }
//            return resourcePath;
//        }

//        private static string UnencodeResourceURIToFilePath(string resource)
//        {

//            // Convert any url-encoded characters (e.g. spaces) back into unencoded form
//            return WebUtility.UrlDecode(resource.ReplaceAll("+", "%2B"))
//                    .ReplaceFirst("jar:", "")
//                    .ReplaceFirst("file:", "")
//                    .ReplaceAll("!.*", "");

//        }
//    }
//}
