namespace TestContainers.Containers.Output
{
    public static class ComposingExtensions
    {
        public static IConsumer<T> AndThen<T>(this IConsumer<T> instance, IConsumer<T> secondInstance)
        {
            return new AndThenConsumer<T>(instance, secondInstance);
        }
    }
}
