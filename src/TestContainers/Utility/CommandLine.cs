using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestContainers.Utility
{
    class CommandLine
    {
        internal static bool ExecutableExists(string executable)
        {
            // First check if we've been given the full path already
            var directFile = new FileInfo(executable);
            if (directFile.Exists)// && directFile.canExecute())
            {
                return true;
            }

            foreach (var pathString in Environment.GetEnvironmentVariable("PATH").Split(';'))
            {
                if (Directory.Exists(pathString) && File.Exists(Path.Combine(pathString, executable)))// && Files.isExecutable(path.resolve(executable)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
