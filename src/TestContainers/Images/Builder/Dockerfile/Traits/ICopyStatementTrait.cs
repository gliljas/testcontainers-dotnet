using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestContainers.Images.Builder.Dockerfile.Statements;

namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface ICopyStatementTrait<T> where T : ICopyStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
