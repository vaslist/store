using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store
{
    // реализация паттерна ValueObject
    // объект не может изменяться после того как мы его создали
    // у него нет идентификатора
    // но он может хранить идентификаторы других объетов
    public class OrderPayment
    {
        public string UniqueCode { get; }
        public string Description { get; }
        public IReadOnlyDictionary<string, string> Parameters { get; }

        public OrderPayment(string uniqueCode, 
                             string description, 
                             IReadOnlyDictionary<string,string> parameters)
        {
            if(string.IsNullOrEmpty(uniqueCode))
                throw new ArgumentNullException(nameof(uniqueCode));
            if(string.IsNullOrEmpty(description)) 
                throw new ArgumentNullException(nameof(description));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.UniqueCode = uniqueCode;
            this.Description = description;
            this.Parameters = parameters;
        }
    }
}
