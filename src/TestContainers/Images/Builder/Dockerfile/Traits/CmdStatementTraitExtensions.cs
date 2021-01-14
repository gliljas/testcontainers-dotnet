using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class CmdStatementTraitExtensions
    {

        public static T Cmd<T>(this T trait, string command) where T : ICmdStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new SingleArgumentStatement("CMD", command));
        }
        public static T Cmd<T>(this T trait, params string[] commandParts) where T : ICmdStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new MultiArgsStatement("CMD", commandParts));
        }
    }

}
