using System.Linq;
using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public static class ExposeStatementTraitExtensions
    {
        public static T Expose<T>(this T trait, params int[] ports) where T : IExposeStatementTrait<T>, IDockerfileBuilderTrait<T>
        {
            return (T) trait.WithStatement(new SingleArgumentStatement("EXPOSE", string.Join(" ", ports.Select(p=>p.ToString()))));
        }
    }

}
