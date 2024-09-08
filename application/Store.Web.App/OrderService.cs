using Microsoft.AspNetCore.Http;
using PhoneNumbers;
using Store.Messages;

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

        public async Task<(bool hasValue, OrderModel? model)> TryGetModelAsync()
        {
            var (hasValue, order) = await TryGetOrderAsync();
            if (hasValue)
            {
                return (true, await MapAsync(order));
            }
            return (false, null);
        }

        internal async Task<(bool hasValue, Order? order)> TryGetOrderAsync()
        {
            if (session.TryGetCart(out Cart cart))
            {
                var order = await orderRepository.GetByIdAsync(cart.OrderId);
                return (true, order);
            }

            return (false, null);
        }

        private async Task<OrderModel> MapAsync(Order order)
        {
            var books = await GetBooksAsync(order);
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

        internal async Task<IEnumerable<Book>> GetBooksAsync(Order order)
        {
            var bookIds = order.Items.Select(x => x.BookId).ToArray();
            return await bookRepository.GetAllByIdsAsync(bookIds);
        }

        public async Task<OrderModel> AddBookAsync(int bookId, int count)
        {
            if (count < 1)
                throw new InvalidOperationException("Too few books to add");
            var (hasValue, order) = await TryGetOrderAsync();
            if (!hasValue)
                order = await orderRepository.CreateAsync();

            await AddOrUpdateBookAsync(order, bookId, count);
            UpdateSession(order);

            return await MapAsync(order);
        }

        private void UpdateSession(Order order)
        {
            var cart = new Cart(order.Id, order.TotalCount, order.TotalPrice);
            session.Set(cart);
        }

        public async Task<OrderModel> UpdateBookAsync(int bookId, int count)
        {
            Order order = await GetOrderAsync();
            order.Items.Get(bookId).Count = count;

            await orderRepository.UpdateAsync(order);
            UpdateSession(order);

            return await MapAsync(order);
        }

        public async Task<OrderModel> RemoveBookAsync(int bookId)
        {
            Order order = await GetOrderAsync();
            order.Items.Remove(bookId);

            await orderRepository.UpdateAsync(order);
            UpdateSession(order);
            return await MapAsync(order);
        }

        public async Task<Order> GetOrderAsync()
        {
            var (hasValue, order) = await TryGetOrderAsync();
            if (hasValue)
                return order;

            throw new InvalidOperationException("Empty session");
        }

        public async Task<OrderModel> SendConfirmationAsync(string cellPhone)
        {
            var order = await GetOrderAsync();
            var model = await MapAsync(order);

            if (TryFormatPhone(cellPhone, out string formatedPhone))
            {
                var confirmationCode = 1111;
                model.CellPhone = formatedPhone;
                session.SetInt32(formatedPhone, confirmationCode);
                await notificationService.SendConfirmationCodeAsync(formatedPhone, confirmationCode);
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

        public async Task<OrderModel> SetDeliveryAsync(OrderDelivery delivery)
        {
            var order = await GetOrderAsync();
            order.Delivery = delivery;
            await orderRepository.UpdateAsync(order);

            return await MapAsync(order);
        }
        
        public async Task<OrderModel> SetPaymentAsync(OrderPayment payment)
        {
            var order = await GetOrderAsync();
            order.Payment = payment;
            await orderRepository.UpdateAsync(order);

            session.RemoveCart();

            await notificationService.StartProcessAsync(order);

            return await MapAsync(order);
        }

        public async Task<OrderModel> ConfirmCellPhoneAsync(string cellPhone, int confirmationCode)
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
            var order = await GetOrderAsync();
            order.CellPhone = cellPhone;
            await orderRepository.UpdateAsync(order);

            session.Remove(cellPhone);
            return await MapAsync(order);
        }

        internal async Task AddOrUpdateBookAsync(Order order, int bookId, int count)
        {
            var book = await bookRepository.GetByIdAsync(bookId);

            if (order.Items.TryGet(bookId, out OrderItem orderItem))
                orderItem.Count += count;

            else
                order.Items.Add(book.Id, book.Price, count);

            await orderRepository.UpdateAsync(order);
        }


    }
}
