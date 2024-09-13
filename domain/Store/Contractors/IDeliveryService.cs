namespace Store.Contractors
{
    public interface IDeliveryService
    {
        string Name { get; }
        string Title { get; }
        Form FirstForm(Order order);
        Form NextForm(int step, IReadOnlyDictionary<string, string> value);
        OrderDelivery GetDelivery(Form form);
    }
}
