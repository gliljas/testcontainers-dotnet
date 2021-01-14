using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class UserStatementTraitExtensions
    {
        public static T User<T>(this T trait, string user) where T : IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new SingleArgumentStatement("USER", user));
        }
    }

}
