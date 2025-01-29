namespace eShoes.Models
{
    public class Order
    {   
        public int Id { get; set;}
        public DateTime CreationDate { get; private set;}
        public List<OrderItem> Items { get; private set;} = new List<OrderItem>();
        public decimal TotalPrice { get; private set;}

        public int UserId { get; set; }
        public User User { get; set;}

        public Order()
        {

        }

        public Order(List<OrderItem> items)
        {
            CreationDate = DateTime.Now;
            Items = items ?? throw new ArgumentNullException(nameof(items));
            TotalPrice = items.Sum(i => i.TotalPrice);
        }

    }
}