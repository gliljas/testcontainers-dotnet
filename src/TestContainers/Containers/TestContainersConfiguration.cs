using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using TestContainers.Images;

namespace TestContainers.Core.Containers
{
    public class TestContainersConfiguration
    {
        private readonly Dictionary<string, string> _environment = new Dictionary<string, string>();

        private static string PROPERTIES_FILE_NAME = "testcontainers.properties";

        private static FileInfo USER_CONFIG_FILE = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "." + PROPERTIES_FILE_NAME));

        private static readonly string AMBASSADOR_IMAGE = "richnorth/ambassador";
        private static readonly string SOCAT_IMAGE = "alpine/socat";
        private static readonly string VNC_RECORDER_IMAGE = "testcontainers/vnc-recorder";
        private static readonly string COMPOSE_IMAGE = "docker/compose";
        private static readonly string ALPINE_IMAGE = "alpine";

        //TODO: Public?
        public virtual DockerImageName GetConfiguredSubstituteImage(DockerImageName original)
        {
            foreach (var entry in CONTAINER_MAPPING)
            {
                if (original.IsCompatibleWith(entry.Key))
                {
                    return
                       (DockerImageName.Parse(Convert.ToString(GetEnvVarOrProperty(entry.Value, null)).Trim()) ?? original).AsCompatibleSubstituteFor(original);

                }
            }
            return original;
        }

        private static readonly string RYUK_IMAGE = "testcontainers/ryuk";
        private static readonly string KAFKA_IMAGE = "confluentinc/cp-kafka";
        private static readonly string PULSAR_IMAGE = "apachepulsar/pulsar";
        private static readonly string LOCALSTACK_IMAGE = "localstack/localstack";
        private static readonly string SSHD_IMAGE = "testcontainers/sshd";

        private static readonly ImmutableDictionary<DockerImageName, string> CONTAINER_MAPPING = new Dictionary<DockerImageName, string> {
        {DockerImageName.Parse(AMBASSADOR_IMAGE), "ambassador.container.image" },
        {DockerImageName.Parse(SOCAT_IMAGE), "socat.container.image"},
        {DockerImageName.Parse(VNC_RECORDER_IMAGE), "vncrecorder.container.image"},
        {DockerImageName.Parse(COMPOSE_IMAGE), "compose.container.image"},
        {DockerImageName.Parse(ALPINE_IMAGE), "tinyimage.container.image"},
        {DockerImageName.Parse(RYUK_IMAGE), "ryuk.container.image"},
        {DockerImageName.Parse(KAFKA_IMAGE), "kafka.container.image"},
        {DockerImageName.Parse(PULSAR_IMAGE), "pulsar.container.image"},
        {DockerImageName.Parse(LOCALSTACK_IMAGE), "localstack.container.image"},
        {DockerImageName.Parse(SSHD_IMAGE), "sshd.container.image" }}.ToImmutableDictionary();


        public static TestContainersConfiguration Instance { get; internal set; } = new TestContainersConfiguration();
        public bool EnvironmentSupportsReuse { get; internal set; }
        public string ImageSubstitutorClassName { get; internal set; }
        public string DockerClientStrategyClassName { get; internal set; }
        public string TransportType => GetEnvVarOrProperty("transport.type", "okhttp");

        [ContractAnnotation("_, !null -> !null")]
        public virtual string GetEnvVarOrProperty(string propertyName, string defaultValue) => GetConfigurable(propertyName, defaultValue); //, _userProperties, _classpathProperties

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
        internal virtual void UpdateGlobalConfig(string prop, string value)
        {
            //    throw new NotImplementedException();
        }

        internal static void SetInstance(TestContainersConfiguration configInstance)
        {
            //   throw new NotImplementedException();
        }
    }
}
