using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TestContainers.Images.Builder.Dockerfile.Statements
{
    public class KeyValuesStatement : Statement
    {
        private readonly Dictionary<string, string> _entries;

        public KeyValuesStatement(string type, Dictionary<string,string> entries) : base(type)
        {
            _entries = entries;
        }

        public override void AppendArguments(StringBuilder dockerfileStringBuilder)
        {
            dockerfileStringBuilder.Append(
                string.Join(" \\\n\t", _entries.Select(e => $"{JsonConvert.SerializeObject(e.Key)}={JsonConvert.SerializeObject(e.Value)}"))
            );
            //while (iterator.hasNext())
            //{
            //    Map.Entry<String, String> entry = iterator.next();

            //    try
            //    {
            //        dockerfileStringBuilder.append(objectMapper.writeValueAsString(entry.getKey()));
            //        dockerfileStringBuilder.append("=");
            //        dockerfileStringBuilder.append(objectMapper.writeValueAsString(entry.getValue()));
            //    }
            //    catch (JsonProcessingException e)
            //    {
            //        throw new RuntimeException("Can't serialize entry: " + entry, e);
            //    }

            //    if (iterator.hasNext())
            //    {
            //        dockerfileStringBuilder.append(" \\\n\t");
            //    }
            //}
        }
    }
}
