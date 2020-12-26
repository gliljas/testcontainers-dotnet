namespace TestContainers.Images
{
    public interface IImagePullPolicy
    {
        bool ShouldPull(DockerImageName dockerImageName);
    }
}
