using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TestContainers.Containers
{
    internal class AuditLogger
    {
        private static ILogger _logger;
        public static void DoComposeLog(string[] commandParts,
                                    IReadOnlyList<string> env)
        {

            if (!_logger.IsEnabled(LogLevel.Trace))
            {
                return;
            }


//            MDC.put(MDC_PREFIX + ".Action", "COMPOSE");

            if (env != null)
            {
  //              MDC.put(MDC_PREFIX + ".Compose.Env", env.toString());
            }

            var command = string.Join(" ", commandParts);
//            MDC.put(MDC_PREFIX + ".Compose.Command", command);

            _logger.LogTrace("COMPOSE action with command: {command}, env: {env}", command, env);

            //MDC.remove(MDC_PREFIX + ".Action");
            //MDC.remove(MDC_PREFIX + ".Compose.Command");
            //MDC.remove(MDC_PREFIX + ".Compose.Env");
        }
    }
}
