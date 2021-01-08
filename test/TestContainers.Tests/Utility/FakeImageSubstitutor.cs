using System.Threading.Tasks;
using TestContainers.Images;
using TestContainers.Utility;

namespace TestContainers.Tests.Utility
{
    public class FakeImageSubstitutor : ImageNameSubstitutor
    {
        protected override string Description => "test implementation";

        public override Task<DockerImageName> Apply(DockerImageName original)=> Task.FromResult(DockerImageName.Parse("transformed-" + original.AsCanonicalNameString()));
    }
}
