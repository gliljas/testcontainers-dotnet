using System;
using System.Collections;
using System.Collections.Generic;

namespace TestContainers.Utility
{
    internal class PeekableEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private readonly Action<T> _peekAction;

        public PeekableEnumerable(IEnumerable<T> enumerable, Action<T> peekAction)
        {
            _enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            _peekAction = peekAction ?? throw new ArgumentNullException(nameof(peekAction));
        }

        public IEnumerator<T> GetEnumerator() => new PeekableEnumerator<T>(_enumerable.GetEnumerator(), _peekAction);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class PeekableEnumerator<T> : IEnumerator<T>
        {
            private IEnumerator<T> _enumerator;
            private Action<T> _peekAction;

            public PeekableEnumerator(IEnumerator<T> enumerator, Action<T> peekAction)
            {
                _enumerator = enumerator;
                _peekAction = peekAction;
            }

            public T Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (_enumerator.MoveNext())
                {
                    _peekAction(_enumerator.Current);
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}
