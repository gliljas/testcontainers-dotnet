using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class AddStatementTraitExtensions
    {
        public static T Add<T>(this T trait, string source, string destination) where T : IAddStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new MultiArgsStatement("ADD", source, destination));
        }
    }

}
