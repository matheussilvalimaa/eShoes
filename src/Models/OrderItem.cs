namespace eShoes.Models
{
    public class OrderItem
    {
        public int Id { get; set;}
        public int ProductId { get; set;}
        public string ProductName { get; set;}
        public decimal ProductPrice { get; set;}
        public int Quantity { get; set;}
        public decimal TotalPrice { get; set;}

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}