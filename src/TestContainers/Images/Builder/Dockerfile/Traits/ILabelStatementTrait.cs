namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface ILabelStatementTrait<T> where T : ILabelStatementTrait<T>, IDockerfileBuilderTrait<T>
    {
    }

}
