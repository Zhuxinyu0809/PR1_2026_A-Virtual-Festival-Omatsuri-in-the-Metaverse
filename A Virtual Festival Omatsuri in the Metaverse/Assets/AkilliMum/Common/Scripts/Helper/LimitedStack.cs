using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace AkilliMum
{
    [Serializable]
    public class LimitedStack<T> : IEnumerable<T>
    {
        private T[] _array;
        public LimitedStack(int maxsize)
        {
            if (maxsize == 0)
                throw new Exception("The MaxSize argument must be greater than 0");
            MaxSize = maxsize;
            _array = new T[MaxSize];
            Count = 0;
        }
        public int Count { get; private set; }
        private int MaxSize { get; }
        public T this[int index]
        {
            get
            {
                if (index > MaxSize - 1)
                    throw new Exception($"Index {index} is out of range {MaxSize - 1}");
                return _array[index];
            }
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        public void Clear()
        {
            Array.Clear(_array, 0, MaxSize);
            Count = 0;
        }
        public bool Contains(T item)
        {
            var size = Count;
            var equalityComparer = EqualityComparer<T>.Default;
            while (size-- > 0)
                if (item == null)
                {
                    if (_array[size] == null)
                        return true;
                }
                else if (_array[size] != null && equalityComparer.Equals(_array[size], item))
                {
                    return true;
                }

            return false;
        }
        public void RightShift()
        {
            var TArray = new T[MaxSize];
            Array.Copy(_array, 1, TArray, 0, MaxSize - 1);
            _array = TArray;
        }
        //public T Peek()
        //{
        //    return _array[0];
        //}
        //public T Pop()
        //{
        //    Count--;
        //    var TVal = _array[Count];
        //    _array[Count] = default(T);
        //    return TVal;
        //}
        public void Push(T item)
        {
            //if (Count < MaxSize)
            //{
            //    _array[Count] = item;
            //    Count++;
            //}
            //else
            //{
            //    RightShift();
            //    _array[Count - 1] = item;
            //}
            RightShift();
            _array[0] = item;
        }

        public T At(int index)
        {
            return _array[index];
        }
        [Serializable]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly LimitedStack<T> _stack;
            private int _index;
            public T Current { get; private set; }
            object IEnumerator.Current => Current;
            internal Enumerator(LimitedStack<T> stack)
            {
                _stack = stack;
                _index = -2;
                Current = default(T);
            }
            public void Dispose()
            {
                _index = -1;
            }
            public bool MoveNext()
            {
                if (_index == -2)
                {
                    _index = _stack.Count - 1;
                    if (_index >= 0)
                        Current = _stack._array[_index];
                    return _index >= 0;
                }

                if (_index == -1)
                    return false; 
                _index--;
                Current = !(_index >= 0) ? default(T) : _stack._array[_index];
                return _index >= 0;
            }
            void IEnumerator.Reset()
            {
                _index = -2;
                Current = default(T);
            }
        }
    }
}
