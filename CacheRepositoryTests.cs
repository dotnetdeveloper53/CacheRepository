using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using System.Threading;

namespace CacheRepository.UnitTests
{
    public class Order : IClone<Order>
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public Order DeepClone()
        {
            //doesn't actually deep clone but there are currently no referenced classes in the graph!
            return (Order)this.MemberwiseClone();
        }
    }
    
    public class MockOrderRepository : IRepository<Order, int>
    {
        public int GetCounter = 0;

        public Order Get(int key)
        {
            GetCounter++;

            if (key == 1)
                return new Order()
                {
                    OrderId = 1,
                    OrderDate = DateTime.Today.AddDays(-1)
                };
            else if (key == 2)
                return new Order()
                {
                    OrderId = 2,
                    OrderDate = DateTime.Today.AddDays(-2)
                };
            else if (key == 3)
                return new Order()
                {
                    OrderId = 3,
                    OrderDate = DateTime.Today.AddDays(-3)
                };
            else
                return null;
        }

        public void Save(int key, Order entity)
        {
            //gracefully do nothing
        }
    }

    [TestClass]
    public class CacheRepositoryTests
    {
        CacheRepository<Order, int> _cacheRep;
        MockOrderRepository _mockOrderRep;

        [TestInitialize]
        public void TestSetup()
        {
            _mockOrderRep = new MockOrderRepository();
            _cacheRep = new CacheRepository<Order, int>(_mockOrderRep, new TimeSpan(0, 0, 5));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullRepository_ExpectsException()
        {
            new CacheRepository<Order, int>(null);
        }

        [TestMethod]
        public void Get_WithValidKey_ReturnsEntity()
        {
            var entity = _cacheRep.Get(1);

            Assert.IsNotNull(entity);
            Assert.AreEqual(1, entity.OrderId);
        }

        [TestMethod]
        public void Get_WithInvalidKey_ReturnsNull()
        {
            var entity = _cacheRep.Get(-1);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public void Get_EntityNotInCache_CallsUnderlyingRepositoryOnce()
        {
            var entity = _cacheRep.Get(1);
            Assert.AreEqual(1, _mockOrderRep.GetCounter);
        }

        [TestMethod]
        public void Get_EntityInCache_UnderlyingRepositoryNotCalledSubsequentTimes()
        {
            //get the entity so that it's loaded in the cache
            var entity = _cacheRep.Get(1);
            entity = _cacheRep.Get(1);
            Assert.AreEqual(1, _mockOrderRep.GetCounter);
        }

        [TestMethod]
        public void Get_EntityExpires_CallsUnderlyingRepositorySubsequently()
        {
            var entity = _cacheRep.Get(1);

            //wait 6 seconds for the entity to expire
            Thread.Sleep(6000);
            entity = _cacheRep.Get(1);

            Assert.AreEqual(2, _mockOrderRep.GetCounter);
        }
    }
}
