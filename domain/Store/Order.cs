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
        private List<OrderItem> items;
        public IReadOnlyCollection<OrderItem> Items
        {
            get { return items; }
        }

        public int TotalCount => items.Sum(o => o.Count);

        public decimal TotalPrice => items.Sum(o => o.Count * o.Price);

        public Order(int id, IEnumerable<OrderItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            this.Id = id;
            this.items = new List<OrderItem>(items);
        }

        public OrderItem GetItem(int bookId)
        {
            int index = items.FindIndex(i=>i.BookId== bookId);

            if (index == -1)
                ThrowBookException("Book not found",bookId);

            return items[index];
        }

        public void AddOrUpdateItem(Book book, int count)
        {
            if (book == null) 
                throw new ArgumentNullException(nameof(book));

            int index = items.FindIndex(i => i.BookId == book.Id);

            if(index == -1)
                items.Add(new OrderItem(book.Id, count, book.Price));
            else
                items[index].Count += count;
        }

        public void RemoveItem(int bookId)
        {
            int index = items.FindIndex(i => i.BookId == bookId);
            if (index == -1)
                ThrowBookException("Order does not contain specifed item.", bookId);

            items.RemoveAt(index);
        }

        private void ThrowBookException(string message, int bookId)
        {
            var exception = new InvalidOperationException(message);

            exception.Data["bookId"] = bookId;

            throw exception;
        }
    }
}
