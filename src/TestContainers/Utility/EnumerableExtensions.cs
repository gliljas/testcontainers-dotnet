using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers.Utility
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Peek<T>(this IEnumerable<T> enumerable, Action<T> peekAction) => new PeekableEnumerable<T>(enumerable, peekAction);
        
    }
}
