namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IWorkdirStatementTrait<T> where T : IWorkdirStatementTrait<T>, IDockerfileBuilderTrait<T>
    {

    }

}
