using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class FromStatementTraitExtensions
    {

        public static T From<T>(this T trait, string dockerImageName) where T : IFromStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            DockerImageName.Parse(dockerImageName).AssertValid();
            return (T) trait.WithStatement(new SingleArgumentStatement("FROM", dockerImageName));
        }
    }

}
