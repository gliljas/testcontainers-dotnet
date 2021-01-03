using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TestContainers.Utility
{
    public static class PathUtils
    {
        /**
     * Create a MinGW compatible path based on usual Windows path
     *
     * @param path a usual windows path
     * @return a MinGW compatible path
     */
        public static string CreateMinGWPath(string path)
        {
            var mingwPath = path.Replace('\\', '/');
            int driveLetterIndex = 1;
            if (Regex.IsMatch(mingwPath, "^[a-zA-Z]:\\/.*"))
            {
                driveLetterIndex = 0;
            }

            // drive-letter must be lower case
            mingwPath = "//" + mingwPath[driveLetterIndex].ToString().ToLower() +
                    mingwPath.Substring(driveLetterIndex + 1);
            mingwPath = mingwPath.Replace(":", "");
            return mingwPath;
        }
    }
}
