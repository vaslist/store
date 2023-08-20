using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PhoneNumbers;
using Store.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Web.App
{
    public class OrderService
    {
        private readonly IBookRepository bookRepository;
        private readonly IOrderRepository orderRepository;
        private readonly INotificationService notificationService;
        private readonly IHttpContextAccessor httpContextAccessor;
        protected ISession session => httpContextAccessor.HttpContext.Session;

        public OrderService(IBookRepository bookRepository,
                           IOrderRepository orderRepository,
                            INotificationService notificationService,
                           IHttpContextAccessor httpContextAccessor)
        {
            this.bookRepository = bookRepository;
            this.orderRepository = orderRepository;
            this.notificationService = notificationService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool TryGetModel(out OrderModel model)
        {
            if (TryGetOrder(out Order order))
            {
                model = Map(order);
                return true;
            }
            model = null;
            return false;

        }

        private OrderModel Map(Order order)
        {
            var books = GetBooks(order);
            var items = from item in order.Items
                        join book in books on item.BookId equals book.Id
                        select new OrderItemModel
                        {
                            BookId = book.Id,
                            Author = book.Author,
                            Title = book.Title,
                            Price = item.Price,
                            Count = item.Count,
                        };
            return new OrderModel
            {
                Id = order.Id,
                Items = items.ToArray(),
                TotalCount = order.TotalCount,
                TotalPrice = order.TotalPrice,
                CellPhone = order.CellPhone,
                DeliveryDescription = order.Delivery?.Description,
                PaymentDescription = order.Payment?.Description,
            };
        }

        internal IEnumerable<Book> GetBooks(Order order)
        {
            var bookIds = order.Items.Select(x => x.BookId).ToArray();
            return bookRepository.GetAllByIds(bookIds);
        }

        public OrderModel AddBook(int bookId, int count)
        {
            if (count < 1)
                throw new InvalidOperationException("Too few books to add");
            
            if (!TryGetOrder(out Order order))
                order = orderRepository.Create();

            AddOrUpdateBook(order, bookId, count);
            UpdateSession(order);

            return Map(order);
        }

        private void UpdateSession(Order order)
        {
            var cart = new Cart(order.Id, order.TotalCount, order.TotalPrice);
            session.Set(cart);
        }

        public OrderModel UpdateBook(int bookId, int count)
        {
            Order order = GetOrder();
            order.Items.Get(bookId).Count = count;

            orderRepository.Update(order);
            UpdateSession(order);

            return Map(order);
        }

        public OrderModel RemoveBook(int bookId)
        {
            Order order = GetOrder();
            order.Items.Remove(bookId);
            UpdateSession(order);
            return Map(order);
        }
        public Order GetOrder()
        {
            if (TryGetOrder(out Order order))
                return order;

            throw new InvalidOperationException("Empty session");
        }

        public OrderModel SendConfirmation(string cellPhone)
        {
            var order = GetOrder();
            var model = Map(order);

            if (TryFormatPhone(cellPhone, out string formatedPhone))
            {
                var confirmationCode = 1111;
                model.CellPhone = formatedPhone;
                session.SetInt32(formatedPhone, confirmationCode);
                notificationService.SendConfirmationCode(formatedPhone, confirmationCode);
            }
            else
                model.Errors["cellPhone"] = "Номер телефона не соответствует";

            return model;
        }

        private readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
        private bool TryFormatPhone(string cellPhone, out string formatedPhone)
        {
            try
            {
                var phoneNumber = phoneNumberUtil.Parse(cellPhone, "ru");
                formatedPhone = phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.INTERNATIONAL);
                return true;
            }
            catch (NumberParseException)
            {
                formatedPhone = null;
                return false;
            }
        }

        public OrderModel SetDelivery(OrderDelivery delivery)
        {
            var order = GetOrder();
            order.Delivery = delivery;
            orderRepository.Update(order);

            return Map(order);
        }

        public OrderModel SetPayment(OrderPayment payment)
        {
            var order = GetOrder();
            order.Payment = payment;
            orderRepository.Update(order);

            session.RemoveCart();

            return Map(order);
        }


        public OrderModel ConfirmCellPhone(string cellPhone, int confirmationCode)
        {
            int? storeCode = session.GetInt32(cellPhone);
            var model = new OrderModel();
            if (storeCode == null)
            {
                model.Errors["celPhone"] = "Чтото случилось. Попробуйте получить код еще раз";
                return model;
            }
            if (storeCode != confirmationCode)
            {
                model.Errors["confirmationCode"] = "Неверный код.Проверьте и попробуйте еще раз";
                return model;
            }
            var order = GetOrder();
            order.CellPhone = cellPhone;
            orderRepository.Update(order);

            session.Remove(cellPhone);
            return Map(order);
        }

        private void AddOrUpdateBook(Order order, int bookId, int count)
        {
            var book = bookRepository.GetById(bookId);
            if (order.Items.TryGet(bookId, out OrderItem orderItem))
                orderItem.Count += count;

            else
                order.Items.Add(book.Id, book.Price, count);

        }

        internal bool TryGetOrder(out Order order)
        {
            if (session.TryGetCart(out Cart cart))
            {
                order = orderRepository.GetById(cart.OrderId);
                return true;
            }

            order = null;
            return false;
        }
    }
}
