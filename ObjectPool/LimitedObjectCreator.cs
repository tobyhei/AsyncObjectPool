using System;
using ObjectPool.Misc;

namespace ObjectPool
{
    /// <summary>
    /// Creates a maximum of _totalLimit objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedObjectCreator<T>
    {
        private Func<T> _objectCreaterFunc;
        private readonly int? _totalLimit;
        private readonly TimeSpan? _periodicLimit;
        private int _current;
        private DateTime _nextCreationInstant;

        public LimitedObjectCreator(
            Func<T> objectCreaterFunc, int? totalLimit, TimeSpan? periodicLimit)
        {
            if (totalLimit.HasValue && totalLimit.Value < 1)
            {
                throw new ArgumentException($"{nameof(totalLimit)} must be a positive integer");
            }

            _totalLimit = totalLimit;
            _objectCreaterFunc = objectCreaterFunc;
            _periodicLimit = periodicLimit;

            _current = 0;
            _nextCreationInstant = DateTime.MinValue;
        }

        public Optional<T> TryCreate()
        {
            if ((_totalLimit.HasValue && _current >= _totalLimit) ||
                (_periodicLimit.HasValue && DateTime.UtcNow < _nextCreationInstant))
            {
                return Optional<T>.None();
            }

            var resource = Optional<T>.Some(_objectCreaterFunc());
            _current++;
            if (_periodicLimit.HasValue)
            {
                _nextCreationInstant = DateTime.UtcNow + _periodicLimit.Value;
            }

            if (_current == _totalLimit)
            {
                // Once we are finished creating we can clear the creator func
                // to allow gc of any potentially expensive types involved
                _objectCreaterFunc = null;
            }

            return resource;
        }
    }
}