using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

namespace CacheRepository.UnitTests
{
    [TestClass]
    public class CloneCacheRepositoryTests
    {
        CloneCacheRepository<Order, int> _mockRep;

        [TestInitialize]
        public void TestSetup()
        {
            _mockRep = new CloneCacheRepository<Order, int>(new MockOrderRepository());
        }

        [TestMethod]
        public void Get_SameEntityTwice_ReturnsDifferentObjectReferences()
        {
            var entity1 = _mockRep.Get(1);
            var entity2 = _mockRep.Get(1);

            Assert.IsNotNull(entity1);
            Assert.IsNotNull(entity2);

            Assert.AreEqual(entity1.OrderId, entity2.OrderId);

            ObjectIDGenerator g = new ObjectIDGenerator();
            bool firsttime;
            long id1 = g.GetId(entity1, out firsttime);
            long id2 = g.GetId(entity2, out firsttime);
            Assert.AreNotEqual(id1, id2);
        }
    }
}
