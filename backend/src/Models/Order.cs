namespace eShoes.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalProducts { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalPrice => TotalProducts + ShippingCost;
        public string Status { get; set; } = "Pending";
        public string StripePaymentIntentId { get; set; }
        public string PaymentMethodType { get; set; }
        public string Last4 { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}