using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store
{
    public class Order
    {
        public int Id { get; }
        private List<OrderItem> items;
        public IReadOnlyCollection<OrderItem> Items
        {
            get { return items; }
        }

        public int TotalCount
        {
            get
            {
                return items.Sum(o=>o.Count);
            }
        }

        public decimal TotalPrice
        {
            get { return items.Sum(o => o.Count * o.Price); }
        }

        public Order(int id,IEnumerable<OrderItem> items)
        {
            if(items == null)
                throw new ArgumentNullException(nameof(items));

            this.Id = id;
            this.items = new List<OrderItem>(items);
        }

        public void AddItem(Book book, int count)
        {
            if(book==null)
                throw new ArgumentNullException(nameof(book));

            var item = items.SingleOrDefault(i=>i.BookId == book.Id);
            
            // если книги нет в заказе то мы ее добавляем
            if(item==null)
            {
                items.Add(new OrderItem(book.Id, count, book.Price));
            }
            // если книга есть, то удаляем старую запись
            // и добавляем новую с измененным к-вом 
            else
            {
                items.Remove(item);
                items.Add(new OrderItem(book.Id,item.Count+count, book.Price));
            }
        }
    }
}
