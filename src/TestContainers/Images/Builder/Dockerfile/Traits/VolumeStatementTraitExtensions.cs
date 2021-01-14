using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class VolumeStatementTraitExtensions
    {
        public static T WorkDir<T>(this T trait, params string[] volumes) where T : IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new MultiArgsStatement("VOLUME", volumes));
        }
    }

}
