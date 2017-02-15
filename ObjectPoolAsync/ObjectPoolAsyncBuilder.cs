using System;
using ObjectPool;

namespace ObjectPoolAsync
{
    public class ObjectPoolAsyncBuilder
    {
        public static LimitedObjectPoolAsync<T> CreateLimited<T>(
            Func<T> creator, int totalLimit, TimeSpan periodicLimit)
                => CreateLimitedInner(creator, totalLimit, periodicLimit);

        public static LimitedObjectPoolAsync<T> CreateLimited<T>(
            Func<T> creator, int totalLimit)
                => CreateLimitedInner(creator, totalLimit);

        public static LimitedObjectPoolAsync<T> CreateLimited<T>(
            Func<T> creator, TimeSpan periodicLimit)
                => CreateLimitedInner(creator, periodicLimit: periodicLimit);

        private static LimitedObjectPoolAsync<T> CreateLimitedInner<T>(
            Func<T> creatorFunc, int? totalLimit = null, TimeSpan? periodicLimit = null)
        {
            if (totalLimit.HasValue && totalLimit <= 0)
            {
                throw new ArgumentException("total limit must be greater than zero");
            }

            var creator = new LimitedObjectCreator<T>(creatorFunc, totalLimit, periodicLimit);
            return new LimitedObjectPoolAsync<T>(() => creator.TryCreate());
        }
    }
}
