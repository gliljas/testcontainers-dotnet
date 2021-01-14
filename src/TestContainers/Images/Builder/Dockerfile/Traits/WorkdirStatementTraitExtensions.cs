using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class WorkdirStatementTraitExtensions
    {
        public static T WorkDir<T>(this T trait, string workdir) where T : IWorkdirStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new SingleArgumentStatement("WORKDIR", workdir));
        }
    }

}
