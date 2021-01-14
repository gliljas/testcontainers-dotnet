using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class DockerfileBuilderTraitExtensions
    {
        public static T WithStatement<T>(this T trait, Statement statement) where T : IDockerfileBuilderTrait<T>
        {
            trait.Statements.Add(statement);
            return trait;
        }
    }

}
