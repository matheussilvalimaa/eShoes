using System.Collections.Generic;

namespace eShoes.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public User User { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public Cart() { }

        public Cart(User user) => User = user;
    }
}
