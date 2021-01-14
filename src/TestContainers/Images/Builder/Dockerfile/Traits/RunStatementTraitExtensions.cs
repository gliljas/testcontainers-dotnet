using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class RunStatementTraitExtensions
    {

        public static T Run<T>(this T trait, string command) where T : IRunStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new SingleArgumentStatement("RUN", command));
        }
        public static T Run<T>(this T trait, params string[] commandParts) where T : IRunStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new MultiArgsStatement("RUN", commandParts));
        }
    }

}
