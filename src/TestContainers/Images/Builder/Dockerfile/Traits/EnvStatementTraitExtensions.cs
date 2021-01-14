using System.Collections.Generic;
using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class EnvStatementTraitExtensions
    {

        public static T Env<T>(this T trait, string key, string value) where T : IEnvStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return trait.Env(new Dictionary<string, string> { { key, value } });
        }
        public static T Env<T>(this T trait, Dictionary<string, string> entries) where T : IEnvStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new KeyValuesStatement("ENV", entries));
        }
    }

}
