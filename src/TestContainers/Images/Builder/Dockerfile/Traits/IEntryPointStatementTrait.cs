namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IEntryPointStatementTrait<T> where T : IEntryPointStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
