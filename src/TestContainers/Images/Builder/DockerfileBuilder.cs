using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using TestContainers.Images.Builder.Dockerfile.Statements;
using TestContainers.Images.Builder.Dockerfile.Traits;

namespace TestContainers.Images.Builder
{
    internal class DockerfileBuilder :
        IDockerfileBuilderTrait<DockerfileBuilder>,
        IFromStatementTrait<DockerfileBuilder>,
        IAddStatementTrait<DockerfileBuilder>,
        ICopyStatementTrait<DockerfileBuilder>,
        IRunStatementTrait<DockerfileBuilder>,
        ICmdStatementTrait<DockerfileBuilder>,
        IWorkdirStatementTrait<DockerfileBuilder>,
        IEnvStatementTrait<DockerfileBuilder>,
        ILabelStatementTrait<DockerfileBuilder>,
        IExposeStatementTrait<DockerfileBuilder>,
        IEntryPointStatementTrait<DockerfileBuilder>,
        //IVolumeStatementTrait<DockerfileBuilder>,
        IUserStatementTrait<DockerfileBuilder>
    {
        private List<Statement> _statements = new List<Statement>();
        private ILogger _logger = StaticLoggerFactory.CreateLogger<DockerfileBuilder>();
        List<Statement> IDockerfileBuilderTrait<DockerfileBuilder>.Statements => _statements;

        internal DockerfileBuilder From(string baseImage)
        {
            throw new NotImplementedException();
        }

        internal string Build()
        {
            var builder = new StringBuilder();

            foreach (var statement in _statements)
            {
                builder.Append(statement.Type);
                builder.Append(" ");
                statement.AppendArguments(builder);
                builder.Append("\n");
            }

            var result = builder.ToString();

            _logger.LogDebug("Returning Dockerfile:\n{}", result);

            return result;
        }
    }
}
