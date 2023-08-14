namespace Store
{
    public interface IOrderRepository
    {
        public Order Create();
        public Order GetById(int id);
        void Update(Order order);
    }
}
