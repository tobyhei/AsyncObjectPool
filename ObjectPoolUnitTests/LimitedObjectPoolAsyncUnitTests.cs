using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectPool;
using ObjectPool.Misc;
using ObjectPoolAsync;

namespace ObjectPoolUnitTests
{
    [TestClass]
    public class LimitedObjectPoolAsyncUnitTests
    {
        [TestMethod]
        public async Task ObjectPoolAsync_GetFromUnlimited_ReturnsPooledResource()
        {
            // Arrange
            const int range = 1000;
            var pool = new LimitedObjectPoolAsync<Guid>(() => Optional<Guid>.Some(Guid.NewGuid()));

            // Act
            var tasks = Enumerable.Range(0, range).AsParallel().Select(async i => await AcquireAsync(pool).ConfigureAwait(false)).ToArray();
            await Task.WhenAll(tasks);
            var guids = tasks.Select(t => t.Result).FilterValues().ToArray();
            var uniqueGuids = new HashSet<Guid>(guids);

            // Assert
            Assert.AreEqual(range, guids.Length);
            Assert.AreEqual(range, uniqueGuids.Count);
        }

        [TestMethod]
        public async Task ObjectPoolAsync_GetFromLimited_ReturnsPooledResource()
        {
            // Arrange
            const int range = 1000;
            const int contentionLevel = 20;
            const int totalUniqueGuids = range / contentionLevel;
            var pool = ObjectPoolAsyncBuilder.CreateLimited(Guid.NewGuid, totalLimit: totalUniqueGuids);

            // Act
            var tasks = Enumerable.Range(0, range).AsParallel().Select(async i => await AcquireAsync(pool).ConfigureAwait(false)).ToArray();
            await Task.WhenAll(tasks);
            var guids = tasks.Select(t => t.Result).FilterValues().ToArray();
            var uniqueGuids = new HashSet<Guid>(guids);

            // Assert
            Assert.AreEqual(range, guids.Length);
            Assert.AreEqual(totalUniqueGuids, uniqueGuids.Count);
        }

        private static Optional<Guid> TryAcquire(LimitedObjectPoolAsync<Guid> pool)
        {
            Func<Pooled<Guid>, Guid> unwrapFunc = pooled =>
            {
                using (pooled)
                {
                    return pooled.Resource;
                }
            };

            return pool.TryGet().Transform(unwrapFunc);
        }

        private static async Task<Optional<Guid>> AcquireAsync(LimitedObjectPoolAsync<Guid> pool)
        {
            Func<Pooled<Guid>, Guid> unwrapFunc = pooled =>
            {
                using (pooled)
                {
                    return pooled.Resource;
                }
            };

            var result = await pool.TryGetAsync().ConfigureAwait(false);

            return result.Transform(unwrapFunc);
        }
    }
}
