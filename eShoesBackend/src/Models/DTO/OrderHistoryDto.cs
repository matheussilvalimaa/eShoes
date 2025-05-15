namespace eShoes.DTO
{
    public class OrderHistoryDto
    {
        public int Id { get; set; }
        public DateTime OrderDate    { get; set; }
        public decimal TotalProducts { get; set; }
        public decimal ShippingCost  { get; set; }
        public decimal TotalPrice    { get; set; }
        public string Status         { get; set; }
        public string PaymentMethodType { get; set; }
        public string Last4 { get; set; }
        
        public List<OrderItemDto> Items { get; set; }
    }
}