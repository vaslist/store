using System.Diagnostics;

namespace Store.Messages
{
    public class DebugNotificationService : INotificationService
    {
        public void SendConfirmationCode(string cellPhone, int code)
        {
            Debug.WriteLine("Cell phone: {0}, code: {1:0000}.", cellPhone, code);
        }

        public Task SendConfirmationCodeAsync(string cellPhone, int code)
        {
            Debug.WriteLine("Cell phone: {0}, code: {1:0000}.", cellPhone, code);

            return Task.CompletedTask;
        }

        public void StartProcess(Order order)
        {
            // отправка письма
            //using(var client = new SmtpClient())
            //{
            //    var message = new MailMessage("from@at.domain","to@at.domain");
            //    message.Subject= "Заказ # " +order.Id;

            //    var builder = new StringBuilder();
            //    foreach (var item in order.Items)
            //    {
            //        builder.Append("{0}, {1}", item.BookId, item.Count);
            //        builder.AppendLine();   
            //    }
            //    message.Body = builder.ToString();
            //    message.BodyEncoding = Encoding.UTF8;
            //    client.Send(message);
            //}

            Debug.WriteLine("Order ID {0}", order.Id);
            Debug.WriteLine("Delivery: {0}", (object)order.Delivery.Description);
            Debug.WriteLine("Payment: {0}", (object)order.Payment.Description);

        }

        public Task StartProcessAsync(Order order)
        {
            StartProcess(order);
            return Task.CompletedTask;
        }
    }
}