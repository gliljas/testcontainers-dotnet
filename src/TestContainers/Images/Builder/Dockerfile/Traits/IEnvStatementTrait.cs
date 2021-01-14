namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IEnvStatementTrait<T> where T : IEnvStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
