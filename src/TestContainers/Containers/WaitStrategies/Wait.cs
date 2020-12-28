using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public static class Wait
    {
        /**
     * Convenience method to return the default WaitStrategy.
     *
     * @return a WaitStrategy
     */
        public static IWaitStrategy DefaultWaitStrategy()
        {
            return ForListeningPort();
        }

        /**
         * Convenience method to return a WaitStrategy for an exposed or mapped port.
         *
         * @return the WaitStrategy
         * @see HostPortWaitStrategy
         */
        public static HostPortWaitStrategy ForListeningPort()
        {
            return new HostPortWaitStrategy();
        }

        /**
         * Convenience method to return a WaitStrategy for an HTTP endpoint.
         *
         * @param path the path to check
         * @return the WaitStrategy
         * @see HttpWaitStrategy
         */
        public static HttpWaitStrategy ForHttp(String path)
        {
            return new HttpWaitStrategy()
                    .ForPath(path);
        }

        /**
         * Convenience method to return a WaitStrategy for an HTTPS endpoint.
         *
         * @param path the path to check
         * @return the WaitStrategy
         * @see HttpWaitStrategy
         */
        public static HttpWaitStrategy ForHttps(String path)
        {
            return ForHttp(path)
                    .UsingTls();
        }

        /**
         * Convenience method to return a WaitStrategy for log messages.
         *
         * @param regex the regex pattern to check for
         * @param times the number of times the pattern is expected
         * @return LogMessageWaitStrategy
         */
        public static LogMessageWaitStrategy ForLogMessage(string regex, int times)
        {
            return new LogMessageWaitStrategy().WithRegEx(regex).WithTimes(times);
        }

        /**
         * Convenience method to return a WaitStrategy leveraging Docker's built-in healthcheck.
         *
         * @return DockerHealthcheckWaitStrategy
         */
        public static DockerHealthcheckWaitStrategy ForHealthcheck()
        {
            return new DockerHealthcheckWaitStrategy();
        }

        
    }

    public interface IRateLimiter
    {
        Task<T> GetWhenReady<T>(Func<Task<T>> func);
    }
}
