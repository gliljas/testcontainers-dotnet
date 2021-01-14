namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IFromStatementTrait<T> where T : IFromStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
