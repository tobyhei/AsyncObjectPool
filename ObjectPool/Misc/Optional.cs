using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObjectPool.Misc
{
    public static class OptionalExtensions
    {
        public static IEnumerable<T> FilterValues<T>(this IEnumerable<Optional<T>> optionals)
            => optionals.Where(o => o.HasValue).Select(o => o.Value);
    }

    public struct Optional<T>
    {
        private readonly T _value;

        public bool HasValue { get; }

        public T Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException("Can't access value of Optional of type none");
                }
                return _value;
            }
        }

        private Optional(bool hasValue, T value = default(T))
        {
            HasValue = hasValue;
            _value = value;
        }

        public static Optional<T> Some(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new Optional<T>(true, value);
        }

        public static Optional<T> None()
            => new Optional<T>(false);

        public T Unwrap()
        {
            if (!HasValue)
            {
                throw new InvalidOperationException();
            }

            return Value;
        }

        // TODO can't remember what chaining/transforming nones is called, rename to its proper name
        public Optional<TOther> Transform<TOther>(Func<T, TOther> createFunc)
            => HasValue ? Optional<TOther>.Some(createFunc(Value)) : Optional<TOther>.None();

        public async Task<Optional<TOther>> TransformAsync<TOther>(Func<T, Task<TOther>> createFunc)
            => HasValue ? Optional<TOther>.Some(await createFunc(Value).ConfigureAwait(false)) : Optional<TOther>.None();
    }
}
