namespace eShoes.DTO
{
    public class CheckoutRequest
    {
        public AddressInputDto Address { get; set; }
        public PaymentInputDto Payment  { get; set; }
    }
}