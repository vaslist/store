using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Memory
{
    public class OrderRepository : IOrderRepository
    {
        private readonly List<Order> orders = new List<Order>();
        public Order Create()
        {
            int nextId = orders.Count + 1;
            Order order = new Order(nextId, new OrderItem[0]);
            orders.Add(order);
            return order;
        }

        public Order GetById(int id)
        {
            // single выбрасывает исключение если не будет такого объекта
            // или их будет больше 1-го
            return orders.Single(o => o.Id == id);
        }

        public void Update(Order order)
        {
            ;
        }
    }
}
