using System;
using Microsoft.Extensions.Logging;

namespace TestContainers.Utility
{
    public static class DockerLoggerFactory
    {
        static ILoggerFactory _loggerFactory;
        public static ILogger GetLogger(String dockerImageName)
        {

            string abbreviatedName;
            if (dockerImageName.Contains("@sha256"))
            {
                abbreviatedName = dockerImageName.Substring(0, dockerImageName.IndexOf("@sha256") + 14) + "...";
            }
            else
            {
                abbreviatedName = dockerImageName;
            }

            //if ("UTF-8".Equals(Environment. System.getProperty("file.encoding")))
            //{
                return StaticLoggerFactory.CreateLogger("\uD83D\uDC33 [" + abbreviatedName + "]");
            //}
            //else
            //{
            //    return LoggerFactory.getLogger("docker[" + abbreviatedName + "]");
            //}
        }
    }
}
