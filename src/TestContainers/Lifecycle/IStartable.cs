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
            var futures = new List<Task>();
            foreach (var startable in startables)
            {
                if (!started.ContainsKey(startable))
                {
                    await DeepStart(started, startable.Dependencies);
                    if (!started.ContainsKey(startable))
                    {
                        started[startable] = startable.Start();
                    }
                }           
            }
            
            await Task.WhenAll(futures);
        }


    }

}
