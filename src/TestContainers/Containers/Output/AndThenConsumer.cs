namespace TestContainers.Containers.Output
{
    internal class AndThenConsumer<T> : IConsumer<T>
    {
        private readonly IConsumer<T> _instance;
        private readonly IConsumer<T> _secondInstance;

        public AndThenConsumer(IConsumer<T> instance, IConsumer<T> secondInstance)
        {
            _instance = instance ?? throw new System.ArgumentNullException(nameof(instance));
            _secondInstance = secondInstance ?? throw new System.ArgumentNullException(nameof(secondInstance));
        }

        public void Accept(T value)
        {
            _instance.Accept(value);
            _secondInstance.Accept(value);
        }
    }
}
