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
    public class ObjectPoolAsyncUnitTests
    {
        [TestMethod]
        public void ObjectPoolAsync_GetFromUnlimited_ReturnsResource()
        {
            // Arrange
            const int range = 1000;
            var pool = new LimitedObjectPoolAsync<Guid>(() => Optional<Guid>.Some(Guid.NewGuid()));

            // Act
            var optionalGuids = Enumerable.Range(0, range).Select(i => pool.TryGet()).ToList();
            var guids = optionalGuids.Where(o => o.HasValue).Select(o => o.Value.Resource);
            var uniqueGuids = new HashSet<Guid>(guids);

            // Assert
            Assert.AreEqual(range, uniqueGuids.Count);
        }

        [TestMethod]
        public async Task ObjectPoolAsync_GetFromLimited_ReturnsPooledResource()
        {
            // Arrange
            const int range = 1000;
            var pool = ObjectPoolAsyncBuilder.CreateLimited(
                () => Optional<Guid>.Some(Guid.NewGuid()), range);

            // Act
            var tasks = Enumerable.Range(0, range).Select(i => pool.GetAsync()).ToArray();
            await Task.WhenAll(tasks);
            var optionalGuids = tasks.Select(t => t.Result.Resource).ToArray();
            var guids = optionalGuids.FilterValues().ToArray();
            var uniqueGuids = new HashSet<Guid>(guids);

            // Assert
            Assert.AreEqual(range, guids.Length);
            Assert.AreEqual(range, uniqueGuids.Count);
        }
    }
}
