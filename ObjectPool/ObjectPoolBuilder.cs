using ObjectPool.Misc;
using System;

namespace ObjectPool
{
    public class ObjectPoolBuilder
    {
        public static LimitedObjectPool<T> CreateLimited<T>(
            Func<T> creator, int totalLimit, TimeSpan periodicLimit)
                => CreateLimitedInner(creator, totalLimit, periodicLimit);

        public static LimitedObjectPool<T> CreateLimited<T>(
            Func<T> creator, int totalLimit)
                => CreateLimitedInner(creator, totalLimit);

        public static LimitedObjectPool<T> CreateLimited<T>(
            Func<T> creator, TimeSpan periodicLimit)
                => CreateLimitedInner(creator, periodicLimit: periodicLimit);

        public static LimitedObjectPool<T> CreateLimitedObjectPool<T>(Func<Optional<T>> creatorFunc)
            => new LimitedObjectPool<T>(creatorFunc);

        private static LimitedObjectPool<T> CreateLimitedInner<T>(
            Func<T> creatorFunc, int? totalLimit = null, TimeSpan? periodicLimit = null)
        {
            if (totalLimit.HasValue && totalLimit <= 0)
            {
                throw new ArgumentException("total limit must be greater than zero");
            }

            var creator = new LimitedObjectCreator<T>(creatorFunc, totalLimit, periodicLimit);
            return new LimitedObjectPool<T>(() => creator.TryCreate());
        }
    }
}
