namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IRunStatementTrait<T> where T : IRunStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
