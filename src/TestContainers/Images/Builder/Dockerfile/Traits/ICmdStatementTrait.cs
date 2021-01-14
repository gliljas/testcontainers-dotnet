namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface ICmdStatementTrait<T> where T : ICmdStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
