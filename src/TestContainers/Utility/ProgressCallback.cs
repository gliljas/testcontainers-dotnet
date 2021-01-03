using System;
using System.Collections.Generic;

namespace TestContainers.Utility
{
    internal class ProgressCallback<T> : IProgress<T>
    {
        private readonly List<IProgress<T>> _consumers;

        public ProgressCallback()
        {
            _consumers = new List<IProgress<T>>();
        }

        public void Report(T value)
        {
            foreach (var consumer in _consumers)
            {
                consumer.Report(value);
            }
        }

        internal void AddConsumer(OutputType type, IProgress<T> consumer)
        {
            _consumers.Add(consumer);
        }
    }
}
