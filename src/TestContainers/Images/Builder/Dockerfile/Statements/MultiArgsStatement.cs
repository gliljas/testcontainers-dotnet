using System.Text;
using Newtonsoft.Json;

namespace TestContainers.Images.Builder.Dockerfile.Statements
{
    public class MultiArgsStatement : Statement
    {
        private readonly string[] _args;

        public MultiArgsStatement(string type, params string[] args) : base(type)
        {
            _args = args;
        }

        public override void AppendArguments(StringBuilder dockerfileStringBuilder)
        {
            try
            {
                dockerfileStringBuilder.Append(JsonConvert.SerializeObject(_args));
            }
            catch (JsonSerializationException e)
            {
              //  throw new RuntimeException("Can't serialize arguments: " + Arrays.toString(args), e);
            }
        }
    }
}
