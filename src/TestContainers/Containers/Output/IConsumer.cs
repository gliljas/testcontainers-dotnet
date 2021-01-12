namespace TestContainers.Containers.Output
{
    public interface IConsumer<T>
    {
        void Accept(T value);
    }
}
