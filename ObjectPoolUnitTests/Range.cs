using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPoolUnitTests
{
    public class Range<T> where T : IComparable<T>
    {
        public T Start { get; }
        public T End { get; }

        public Range(T start, T end)
        {
            if (start.CompareTo(end) > 1)
            {
                throw new InvalidOperationException("Start cannot be greater than end");
            }

            Start = start;
            End = end;
        }
    }

    public static class RangeExtensions
    {
        public static IEnumerable<int> GenerateAll(this Range<int> range) => Enumerable.Range(range.Start, range.End);
    }
}
