using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Store
{
    public class Order
    {
        public int Id { get; }
       
        public OrderItemCollection Items { get; }

        public string CellPhone { get; set; }

        public OrderDelivery Delivery { get; set; }
        public OrderPayment Payment { get; set; }
        public int TotalCount => Items.Sum(o => o.Count);

        public decimal TotalPrice => Items.Sum(o => o.Count * o.Price)
                                            +(Delivery?.Amount ?? 0m);

        public Order(int id, IEnumerable<OrderItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            this.Id = id;
            this.Items = new OrderItemCollection(items);
        }
    }
}
