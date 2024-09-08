namespace Store
{
    // реализация паттерна ValueObject
    // объект не может изменяться после того как мы его создали
    // у него нет идентификатора
    // но он может хранить идентификаторы других объетов
    public class OrderDelivery
    {
        public string UniqueCode { get; }
        public string Description { get; }
        public decimal Price { get; } // стоимость доставки
        public IReadOnlyDictionary<string, string> Parameters { get; }

        public OrderDelivery(string uniqueCode, 
                             string description, 
                             decimal amount,
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
            this.Price = amount;
            this.Parameters = parameters;
        }
    }
}
