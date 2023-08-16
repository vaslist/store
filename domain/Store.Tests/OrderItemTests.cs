using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Tests
{
    public class OrderItemTests
    {
        [Fact]
        public void OrderItem_WithZeroCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                int count = 0;
                new OrderItem(1, count, 1m);
            });
        }

        [Fact]
        public void OrderItem_WithNegativCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                int count = -1;
                new OrderItem(1, count, 1m);
            });
        }
        [Fact]
        public void OrderItem_WithPositivCount_SetssCount()
        {
            var orderItem = new OrderItem(1, 2, 3m);
            Assert.Equal(1, orderItem.BookId);
            Assert.Equal(2, orderItem.Count);
            Assert.Equal(3m, orderItem.Price);
        }

        [Fact]
        public void Count_WithNegativValue_ThrowsArgumentOutOfRangeException()
        {
            var orderItem = new OrderItem(1, 2, 5m);

            Assert.Throws<ArgumentOutOfRangeException>(() => orderItem.Count = -1);
        }
        [Fact]
        public void Count_WithZeroValue_ThrowsArgumentOutOfRangeException()
        {
            var orderItem = new OrderItem(1, 2, 5m);

            Assert.Throws<ArgumentOutOfRangeException>(() => orderItem.Count = 0);
        }
        [Fact]
        public void Count_WithPositivValue_SetsValue()
        {
            var orderItem = new OrderItem(1, 2, 5m);
            orderItem.Count = 10;
            Assert.Equal(10, orderItem.Count);
        }
    }
}
