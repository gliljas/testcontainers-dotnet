using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class CopyStatementTraitExtensions
    {
        public static T Copy<T>(this T trait, string source, string destination) where T : ICopyStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T)trait.WithStatement(new MultiArgsStatement("COPY", source, destination));
        }
    }

}
