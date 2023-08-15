using Microsoft.AspNetCore.Mvc;
using Store.Web.Models;

namespace Store.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBookRepository bookRepository;
        private readonly IOrderRepository orderRepository;
        public OrderController(IBookRepository bookRepository,
                              IOrderRepository orderRepository)
        {
            this.bookRepository = bookRepository;
            this.orderRepository = orderRepository;
        }
        public IActionResult Index()
        {
            Cart cart;
            if (HttpContext.Session.TryGetCart(out cart))
            {
                var order = orderRepository.GetById(cart.OrderId);
                OrderModel model = Map(order);
                return View(model);
            }
            return View("Empty");
        }
        public IActionResult AddItem(int id)
        {
            Order order;
            Cart cart;
            if (HttpContext.Session.TryGetCart(out cart))
            {
                order = orderRepository.GetById(cart.OrderId);
            }
            else
            {
                order = orderRepository.Create();
                cart = new Cart(order.Id);
            }

            var book = bookRepository.GetById(id);
            order.AddItem(book, 1);
            orderRepository.Update(order);

            cart.TotalCount = order.TotalCount;
            cart.TotalPrice = order.TotalPrice;


            HttpContext.Session.Set(cart);

            return RedirectToAction("Index", "Book", new { id = id });
        }

        private OrderModel Map(Order order)
        {
            var bookIds = order.Items.Select(i => i.BookId);
            var books = bookRepository.GetAllByIds(bookIds);
            var itemModels = from item in order.Items
                             join book in books on item.BookId equals book.Id
                             select new OrderItemModel
                             {
                                 BookId = book.Id,
                                 Author = book.Author,
                                 Title = book.Title,
                                 Count = item.Count,
                                 Price = item.Price,
                             };
            return new OrderModel
            {
                Id = order.Id,
                Items = itemModels.ToArray(),
                TotalCount = order.TotalCount,
                TotalPrice = order.TotalPrice
            };
        }
    }
}

