namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IUserStatementTrait<T> where T : IUserStatementTrait<T>, IDockerfileBuilderTrait<T>
    {

    }

}
