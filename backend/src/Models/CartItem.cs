using System.ComponentModel.DataAnnotations.Schema;

namespace eShoes.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Size { get; set; }
        
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        public Product Product { get; set; }
    }
}
