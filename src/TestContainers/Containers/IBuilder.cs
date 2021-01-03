namespace TestContainers.Core.Containers
{
    public interface IBuilder<T>
    {
        T Build();
    }
}
