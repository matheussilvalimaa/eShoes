namespace eShoes.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public int Rating { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public string AvailableSizes { get; set; }
    }
}
