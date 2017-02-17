using ObjectPool.Misc;
using System;
using System.Linq;
using Xunit;

namespace ObjectPoolUnitTests
{
    public class LimitedObjectPoolUnitTests
    {
        [Fact]
        public void ObjectPool_GetFromEmpty_ReturnsResource()
        {
            // Arrange
            var pool = new ObjectPool.LimitedObjectPool<Guid>(() => Optional<Guid>.Some(Guid.NewGuid()));

            // Act
            var optionalGuids = Enumerable.Range(1, 1000).Select(i => pool.Get()).ToList();

            // Assert
            Assert.True(optionalGuids.All(o => o.Resource != Guid.Empty));
        }
    }
}
