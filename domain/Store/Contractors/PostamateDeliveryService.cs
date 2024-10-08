﻿namespace Store.Contractors
{
    public class PostamateDeliveryService : IDeliveryService
    {
        private static readonly IReadOnlyDictionary<string, string> cities = new Dictionary<string, string>
        {
            {"1","Москва" },
            {"2","Ессентуки" }
        };

        private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> postamates = new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            {
                "1",
                new Dictionary<string, string>
                {
                    { "1", "Казанский вокзал" },
                    { "2", "Охотный ряд" },
                    { "3", "Савёловский рынок" },
                }
            },
            {
                "2",
                new Dictionary<string, string>
                {
                    { "4", "Московский вокзал" },
                    { "5", "Гостиный двор" },
                    { "6", "Вершина" },
                }
            }
        };
        public string Name => "Postamate";

        public string Title => "Доставка через постоматы";

        public Form FirstForm(Order order)
        {
            return Form.CreateFirst(Name)
                       .AddParameter("orderId", order.Id.ToString())
                       .AddField(new SelectionField("Город", "city", "1", cities));
        }
        public Form NextForm(int step, IReadOnlyDictionary<string, string> value)
        {
            if (step == 1)
            {
                if (value["city"] == "1")
                {
                    return Form.CreateNext(Name, 2, value)
                               .AddField(new SelectionField("Постамат", "postamate", "1", postamates["1"]));
                }
                else if (value["city"] == "2")
                {
                    return Form.CreateNext(Name, 2, value)
                               .AddField(new SelectionField("Постамат", "postamate", "4", postamates["2"]));
                }
                else
                {
                    throw new InvalidOperationException("Invalid postamate city.");
                }
            }
            else if (step == 2)
            {
                return Form.CreateLast(Name, 3, value);
            }
            else
            {
                throw new InvalidOperationException("Invalid postamate step.");
            }
        }

        public OrderDelivery GetDelivery(Form form)
        {
            if (form.ServiceName != Name || !form.IsFinal)
                throw new InvalidOperationException("Invalid form.");

            var cityId = form.Parameters["city"];
            var cityName = cities[cityId];
            var postamateId = form.Parameters["postamate"];
            var postamateName = postamates[cityId][postamateId];

            var parameters = new Dictionary<string, string>
            {
                { nameof(cityId), cityId },
                { nameof(cityName), cityName },
                { nameof(postamateId), postamateId },
                { nameof(postamateName), postamateName },
            };

            var description = $"Город: {cityName}\nПостамат: {postamateName}";

            return new OrderDelivery(Name, description, 150m, parameters);
        }
    }
}
