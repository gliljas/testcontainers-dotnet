using System.Text;

namespace TestContainers.Images.Builder.Dockerfile.Statements
{
    public abstract class Statement
    {
        protected Statement(string type)
        {
            Type = type;
        }

        public string Type { get;  }

        public abstract void AppendArguments(StringBuilder builder);
    }
}
