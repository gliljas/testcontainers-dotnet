using System;
using System.Collections.Generic;

namespace TestContainers.Core.Containers
{
    internal class TestContainersConfiguration
    {
        private readonly Dictionary<string, string> environment;
        public static TestContainersConfiguration Instance { get; internal set; }
        public bool EnvironmentSupportsReuse { get; internal set; }
        public string ImageSubstitutorClassName { get; internal set; }
        public string DockerClientStrategyClassName { get; internal set; }
        public string TransportType => GetEnvVarOrProperty("transport.type", "okhttp");

        public string GetEnvVarOrProperty(string propertyName, string defaultValue) => GetConfigurable(propertyName, defaultValue, _userProperties, _classpathProperties);

        private string GetConfigurable(string propertyName, string defaultValue, Properties...propertiesSources)
        {
            var envVarName = propertyName.Replace("\\.", "_").ToUpper();
            if (!envVarName.StartsWith("TESTCONTAINERS_"))
            {
                envVarName = "TESTCONTAINERS_" + envVarName;
            }

            if (environment.containsKey(envVarName))
            {
                return environment.get(envVarName);
            }

            for (final Properties properties : propertiesSources)
            {
                if (properties.get(propertyName) != null)
                {
                    return (String) properties.get(propertyName);
                }
            }

            return defaultValue;
        }
        internal void UpdateGlobalConfig(string v, string assemblyQualifiedName)
        {
            throw new NotImplementedException();
        }
    }
}
