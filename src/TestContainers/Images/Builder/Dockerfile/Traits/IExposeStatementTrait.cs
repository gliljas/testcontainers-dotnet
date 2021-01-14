namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IExposeStatementTrait<T> where T : IExposeStatementTrait<T>, IDockerfileBuilderTrait<T>
    {

    }

}
