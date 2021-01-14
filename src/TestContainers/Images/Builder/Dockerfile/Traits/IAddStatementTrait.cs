namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IAddStatementTrait<T> where T : IAddStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
