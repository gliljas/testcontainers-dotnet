using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class EntryPointStatementTraitExtensions
    {

        public static T EntryPoint<T>(this T trait, string command) where T : IEntryPointStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new SingleArgumentStatement("ENTRYPOINT", command));
        }
        public static T EntryPoint<T>(this T trait, params string[] commandParts) where T : IEntryPointStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new MultiArgsStatement("ENTRYPOINT", commandParts));
        }
    }

}
