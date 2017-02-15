using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectPool.Misc;

namespace ObjectPoolUnitTests
{
    [TestClass]
    public class LimitedObjectPoolUnitTests
    {
        [TestMethod]
        public void ObjectPool_GetFromEmpty_ReturnsResource()
        {
            // Arrange
            var pool = new ObjectPool.LimitedObjectPool<Guid>(() => Optional<Guid>.Some(Guid.NewGuid()));

            // Act
            var optionalGuids = Enumerable.Range(1, 1000).Select(i => pool.Get()).ToList();

            // Assert
            Assert.IsTrue(optionalGuids.All(o => o.Resource != Guid.Empty));
        }
    }
}
