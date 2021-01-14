using System.Linq;
using System.Text;

namespace TestContainers.Images.Builder.Dockerfile.Statements
{
    public class SingleArgumentStatement : Statement
    {
        private readonly string _argument;

        public SingleArgumentStatement(string type, string argument) : base(type)
        {
            _argument = argument;
        }

        public override void AppendArguments(StringBuilder dockerfileStringBuilder)
        {
            dockerfileStringBuilder.Append(_argument.Replace("\n", "\\\n"));
        }
    }
}
