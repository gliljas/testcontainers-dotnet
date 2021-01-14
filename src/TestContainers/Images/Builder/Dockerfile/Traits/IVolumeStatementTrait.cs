namespace TestContainers.Images.Builder.Dockerfile.Traits
{
    public interface IVolumeStatementTrait<T> where T : IVolumeStatementTrait<T>, IDockerfileBuilderTrait<T>
    {

    }

}
