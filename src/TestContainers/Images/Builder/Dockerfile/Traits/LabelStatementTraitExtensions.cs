using System.Collections.Generic;
using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class LabelStatementTraitExtensions
    {

        public static T Label<T>(this T trait, string key, string value) where T : ILabelStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return trait.Label(new Dictionary<string, string> { { key, value } });
        }
        public static T Label<T>(this T trait, Dictionary<string,string> entries) where T : ILabelStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new KeyValuesStatement("LABEL", entries));
        }
    }

}
