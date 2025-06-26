namespace eShoes.DTO
{
    public class CheckoutResponse
    {
        public string Last4        { get; set; }
        public string PixQrCode    { get; set; }
        public string PixCode      { get; set; }
        public string BoletoUrl    { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount  { get; set; }
    }
}