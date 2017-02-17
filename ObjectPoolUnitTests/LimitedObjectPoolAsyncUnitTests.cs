using ObjectPool;
using ObjectPool.Misc;
using ObjectPoolAsync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ObjectPoolUnitTests
{
    public class LimitedObjectPoolAsyncUnitTests
    {
        private const int Range = 1000;

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ObjectPoolAsync_GetFromUnlimited_ReturnsPooledResource(bool wait)
        {
            // Arrange
            var pool = new LimitedObjectPoolAsync<Guid>(() => Optional<Guid>.Some(Guid.NewGuid()));

            var tasks = Enumerable.Range(0, Range).Select(i => AcquireAsync(pool, wait)).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);
            var guids = tasks.Select(t => t.Result).FilterValues().ToArray();
            var uniqueGuids = new HashSet<Guid>(guids);

            // Assert
            Assert.Equal(Range, guids.Length);
            Assert.InRange(uniqueGuids.Count, 1, Range);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ObjectPoolAsync_GetFromLimited_ReturnsPooledResource(bool wait)
        {
            // Arrange
            const int totalUniqueGuids = 50;
            var pool = ObjectPoolAsyncBuilder.CreateLimited(Guid.NewGuid, totalLimit: totalUniqueGuids);

            // Act
            var tasks = Enumerable.Range(0, Range).Select(i => AcquireAsync(pool, wait)).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);
            var guids = tasks.Select(t => t.Result).FilterValues().ToArray();
            var uniqueGuids = new HashSet<Guid>(guids);

            // Assert
            Assert.Equal(Range, guids.Length);
            Assert.InRange(uniqueGuids.Count, 1, totalUniqueGuids);
        }

        private static async Task<Optional<Guid>> AcquireAsync(LimitedObjectPoolAsync<Guid> pool, bool wait)
        {
            async Task<Guid> Unwrap(Pooled<Guid> pooled)
            {
                using (pooled)
                {
                    if (wait)
                    {
                        await Task.Delay(1);
                    }
                    return pooled.Resource;
                }
            }

            var result = await pool.TryGetAsync().ConfigureAwait(false);

            return await result.TransformAsync<Guid>(Unwrap);
        }
    }
}
