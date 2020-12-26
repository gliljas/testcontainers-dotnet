using System;
using Docker.DotNet.Models;

namespace TestContainers.Containers.StartupStrategies
{
    public static class ContainerStateExtensions
    {
        private static readonly string DOCKER_TIMESTAMP_ZERO = "0001-01-01T00:00:00Z";

        public static bool IsContainerRunning(this ContainerState state, TimeSpan? minimumRunningDuration, DateTimeOffset now)
        {
            if (state.Running)
            {
                if (minimumRunningDuration == null)
                {
                    return true;
                }

                var startedAt = DateTimeOffset.Parse(state.StartedAt);

                if (startedAt < now - minimumRunningDuration.Value)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsContainerStopped(this ContainerState state)
        {
            // get some preconditions out of the way
            if (state.Running || state.Paused)
            {
                return false;
            }

            // if the finished timestamp is non-empty, that means the container started and finished.
            bool hasStarted = IsDockerTimestampNonEmpty(state.StartedAt);
            bool hasFinished = IsDockerTimestampNonEmpty(state.FinishedAt);
            return hasStarted && hasFinished;
        }

        private static bool IsDockerTimestampNonEmpty(string dockerTimestamp)
        {
            // This is a defensive approach. Current versions of Docker use the DOCKER_TIMESTAMP_ZERO value, but
            // that could change.
            return dockerTimestamp != null
                    && !string.IsNullOrWhiteSpace(dockerTimestamp)
                    && !dockerTimestamp.Equals(DOCKER_TIMESTAMP_ZERO)
                    && DateTimeOffset.TryParse(dockerTimestamp, out var ts) && ts.ToUnixTimeSeconds() >= 0L;
        }

        public static bool IsContainerExitCodeSuccess(this ContainerState state) => state.ExitCode == 0;
    }
}
