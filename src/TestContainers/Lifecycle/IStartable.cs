using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Lifecycle
{
    public interface IStartable
    {
        ISet<IStartable> Dependencies { get; }

        Task Start(CancellationToken cancellationToken = default);
        Task Stop();
    }

    public static class Startables
    {
        public static Task DeepStart(IEnumerable<IStartable> startables) => DeepStart(new Dictionary<IStartable, Task>(), startables);
        private static async Task DeepStart(Dictionary<IStartable, Task> started, IEnumerable<IStartable> startables)
        {
            var futures = startables.Select(async it =>
            {
                var subStarted = new Dictionary<IStartable, Task>();
                Task future;
                if (!started.ContainsKey(it))
                {
                    future = DeepStart(subStarted, it.Dependencies).ContinueWith(async x => await it.Start());
                    started[it] = future;
                }
                foreach (var entry in subStarted)
                {
                    started.Add(entry.Key, entry.Value);
                }
            });
            await Task.WhenAll(futures);
        }


    }

}
