using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace TestContainers.Core.Containers
{
    internal class TestContainersConfiguration
    {
        private readonly Dictionary<string, string> _environment;
        public static TestContainersConfiguration Instance { get; internal set; }
        public bool EnvironmentSupportsReuse { get; internal set; }
        public string ImageSubstitutorClassName { get; internal set; }
        public string DockerClientStrategyClassName { get; internal set; }
        public string TransportType => GetEnvVarOrProperty("transport.type", "okhttp");

        [ContractAnnotation("_, !null -> !null")]
        public string GetEnvVarOrProperty(string propertyName, string defaultValue) => GetConfigurable(propertyName, defaultValue); //, _userProperties, _classpathProperties

        private string GetConfigurable(string propertyName, string defaultValue)//, params Properties[] propertiesSources
        {
            var envVarName = propertyName.Replace("\\.", "_").ToUpper();
            if (!envVarName.StartsWith("TESTCONTAINERS_"))
            {
                envVarName = "TESTCONTAINERS_" + envVarName;
            }

            if (_environment.TryGetValue(envVarName, out var value))
            {
                return value;
            }

            //foreach (var properties in propertiesSources)
            //{
            //    if (properties.get(propertyName) != null)
            //    {
            //        return (String) properties.get(propertyName);
            //    }
            //}

            return defaultValue;
        }
        internal void UpdateGlobalConfig(string v, string assemblyQualifiedName)
        {
            throw new NotImplementedException();
        }

        internal static void SetInstance(TestContainersConfiguration configInstance)
        {
            throw new NotImplementedException();
        }
    }
}
