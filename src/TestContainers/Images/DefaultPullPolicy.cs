namespace TestContainers.Images
{
    internal class DefaultPullPolicy : AbstractImagePullPolicy
    {
        protected override bool ShouldPullCached(DockerImageName imageName, ImageData localImageData)
        {
            throw new System.NotImplementedException();
        }
    }
}
