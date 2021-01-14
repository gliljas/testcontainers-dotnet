using System.Collections.Generic;
using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IDockerfileBuilderTrait<T> where T : IDockerfileBuilderTrait<T>
    {
        internal List<Statement> Statements { get; }
    }

}
