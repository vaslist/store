using Microsoft.AspNetCore.Mvc;
using Store.Web.Models;
using System.Text.RegularExpressions;
using Store.Messages;
using Store.Contractors;
using Store.Web.Contractors;

namespace Store.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBookRepository bookRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IEnumerable<IDeliveryService> deliveryServices;
        private readonly IEnumerable<IPaymentService> paymentServices;
        private readonly IEnumerable<IWebContractorService> webContractorServices;
        private readonly INotificationService notificationService;
        public OrderController(IBookRepository bookRepository,
                              IOrderRepository orderRepository,
                              IEnumerable<IDeliveryService> deliveryServices,
                              IEnumerable<IPaymentService> paymentServices,
                              IEnumerable<IWebContractorService> webContractorServices,
                              INotificationService notificationService)
        {
            this.bookRepository = bookRepository;
            this.orderRepository = orderRepository;
            this.deliveryServices = deliveryServices;
            this.paymentServices = paymentServices;
            this.webContractorServices = webContractorServices;
            this.notificationService = notificationService;
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
        
        [HttpPost]
        public IActionResult AddItem(int bookId, int count = 1)
        {
            (Order order, Cart cart) = GetOrCreateOrderAndCart();

            var book = bookRepository.GetById(bookId);
            if (order.Items.TryGet(bookId, out OrderItem orderItem))
                orderItem.Count += count;
            else
                order.Items.Add(bookId, book.Price, count);

            SaveOrderAndCart(order, cart);

            return RedirectToAction("Index", "Book", new { id = bookId });
        }

        [HttpPost]
        public IActionResult UpdateItem(int bookId, int count)
        {
            (Order order, Cart cart) = GetOrCreateOrderAndCart();

            order.Items.Get(bookId).Count = count;
            SaveOrderAndCart(order, cart);

            return RedirectToAction("Index", "Order");
        }

        private void SaveOrderAndCart(Order order, Cart cart)
        {
            orderRepository.Update(order);

            cart.TotalCount = order.TotalCount;
            cart.TotalPrice = order.TotalPrice;

            HttpContext.Session.Set(cart);
        }

        private (Order order, Cart cart) GetOrCreateOrderAndCart()
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
            return (order, cart);
        }

        public IActionResult RemoveBook(int id)
        {
            (Order order, Cart cart) = GetOrCreateOrderAndCart();

            order.Items.Get(id).Count--;
            SaveOrderAndCart(order, cart);

            return RedirectToAction("Index", "Book", new { id = id });
        }
        [HttpPost]
        public IActionResult RemoveItem(int bookId)
        {
            (Order order, Cart cart) = GetOrCreateOrderAndCart();


            order.Items.Remove(bookId);
            SaveOrderAndCart(order, cart);

            return RedirectToAction("Index", "Order");
        }

        [HttpPost]
        public IActionResult SendConfirmationCode(int id, string cellPhone)
        {
            var order = orderRepository.GetById(id);
            var model = Map(order);

            if (!IsValidCellPhone(cellPhone))
            {
                model.Errors["cellPhone"] = "Номер телефона не соответствует формату +79876543210";
                return View("Index", model);
            }

            int code = 1111; // random.Next(1000, 10000)
            HttpContext.Session.SetInt32(cellPhone, code);
            notificationService.SendConfirmationCode(cellPhone, code);

            return View("Confirmation",
                        new ConfirmationModel
                        {
                            OrderId = id,
                            CellPhone = cellPhone
                        });
        }

        private bool IsValidCellPhone(string cellPhone)
        {
            if (cellPhone == null)
                return false;

            cellPhone = cellPhone.Replace(" ", "")
                                 .Replace("-", "");

            return Regex.IsMatch(cellPhone, @"^\+?\d{11}$");
        }
        
        [HttpPost]
        public IActionResult Confirmate(int id, string cellPhone, int code)
        {
            int? storedCode = HttpContext.Session.GetInt32(cellPhone);
            if (storedCode == null)
            {
                return View("Confirmation",
                        new ConfirmationModel
                        {
                            OrderId = id,
                            CellPhone = cellPhone,
                            Errors = new Dictionary<string, string>
                            {
                                {"code", "Пустой код, повторите отправку." }
                            }
                        });
            }
            if (storedCode != code)
            {
                return View("Confirmation",
                        new ConfirmationModel
                        {
                            OrderId = id,
                            CellPhone = cellPhone,
                            Errors = new Dictionary<string, string>
                            {
                                {"code", "Неверный код" }
                            }
                        });
            }
            //todo: Сохранить cellPhone
            var order =orderRepository.GetById(id);
            order.CellPhone = cellPhone;
            orderRepository.Update(order);

            HttpContext.Session.Remove(cellPhone);
            var model = new DeliveryModel
            {
                OrderId = id,
                Methods = deliveryServices
                            .ToDictionary(serv => serv.UniqueCode,
                                          serv => serv.Title)
            };

            return View("DeliveryMethod", model);
        }
        
        [HttpPost]
        public IActionResult StartDelivery(int id, string uniqueCode)
        {
            var deliverySevice = deliveryServices
                                 .Single(ds => ds.UniqueCode == uniqueCode);
            var order = orderRepository.GetById(id);
            var form = deliverySevice.CreateForm(order);
            
            return View("DeliveryStep", form);
        }
        
        [HttpPost]
        public IActionResult NextDelivery(int id, string uniqueCode, 
                                          int step,Dictionary<string,string> values) 
        {
            var deliveryService = deliveryServices
                                 .Single(ds => ds.UniqueCode == uniqueCode);
            var form = deliveryService.MoveNextForm(id,step,values);

            if(form.IsFinal)
            {
                var order = orderRepository.GetById(id);
                order.Delivery = deliveryService.GetDelivery(form);
                orderRepository.Update(order);

                var model = new DeliveryModel
                {
                    OrderId = id,
                    Methods = paymentServices
                            .ToDictionary(serv => serv.UniqueCode,
                                          serv => serv.Title)
                };

                return View("PaymentMethod", model);
            }
            return View("DeliveryStep", form);
        }
        [HttpPost]
        public IActionResult StartPayment(int id, string uniqueCode)
        {
            var paymentSevice = paymentServices
                                .Single(ps => ps.UniqueCode == uniqueCode);
            var order = orderRepository.GetById(id);
            var form = paymentSevice.CreateForm(order);

            var webContractorService = webContractorServices
                          .SingleOrDefault(cs => cs.UniqueCode == uniqueCode);
            if(webContractorService != null)
            {
                return Redirect(webContractorService.GetUri);
            }
            return View("PaymentStep", form);
        }

        [HttpPost]
        public IActionResult NextPayment(int id, string uniqueCode,
                                          int step, Dictionary<string, string> values)
        {
            var paymentService = paymentServices
                                 .Single(ds => ds.UniqueCode == uniqueCode);
            var form = paymentService.MoveNextForm(id, step, values);

            if (form.IsFinal)
            {
                var order = orderRepository.GetById(id);
                order.Payment = paymentService.GetPayment(form);
                orderRepository.Update(order);

                return View("Finish");
            }
            return View("PaymentStep", form);
        }

        public IActionResult Finish()
        {
            HttpContext.Session.RemoveCart();
            return View(); 
        }
    }
}

