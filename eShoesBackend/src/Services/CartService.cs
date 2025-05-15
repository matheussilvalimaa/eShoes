using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using eShoes.Context;
using eShoes.Models;

namespace eShoes.Services
{
    public class CartService
    {
        private readonly eShoesDbContext _context;
        private readonly UserService _userService;
        private readonly ILogger<CartService> _logger;

        public CartService(eShoesDbContext context, UserService userService, ILogger<CartService> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        // Get the current cart. If not exist, create a new cart
        public Cart GetUserCart()
        {
            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
                throw new UnauthorizedAccessException("No user logged in");

            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)  
                .FirstOrDefault(c => c.User.Id == currentUser.Id);

            if (cart == null)
            {
                 _logger.LogInformation($"Creating a new cart for user '{currentUser.Username}'.");
                cart = new Cart(currentUser);
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }
            else
            {
                _logger.LogDebug($"Cart found (ID={cart.Id}) for user '{currentUser.Username}'.");
            }

            return cart;
        }

        //Get the specific item's id
        public CartItemDTO GetCartItemById(int itemId)
        {
            _logger.LogInformation($"Fetching cart item with ID: {itemId} for current user.");
            var cart = GetUserCart(); 
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                _logger.LogWarning($"Cart item with ID: {itemId} not found in user's cart.");
                return null;
            }

            return new CartItemDTO
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Name = item.ProductName,
                Price = item.ProductPrice,
                Image = item.Product.ImagePath ?? "/images/default-product.png",
                Quantity = item.Quantity,
                Size = item.Size,
                TotalPrice = item.TotalPrice
            };
        }

        // Add a product in the user's cart. If already exists, updates the quantity.
        public void AddProductCart(Product product, int quantity, string size)
        {
            _logger.LogInformation($"Adding product ID={product.Id}, Quantity={quantity}, Size={size} to cart.");
            if (quantity <= 0)
                throw new Exception("Quantity must be greater than 0");
                
            var cart = GetUserCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.ProductPrice * existingItem.Quantity;
                _logger.LogInformation($"Updated cart item for product ID={product.Id}, new Quantity={existingItem.Quantity}.");
            }
            else
            {
                var newItem = new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductPrice = product.Price,
                    Quantity = quantity,
                    Size = size,
                    TotalPrice = product.Price * quantity,
                    Cart = cart
                };
                cart.Items.Add(newItem);
                _logger.LogInformation($"Added new cart item for product ID={product.Id}.");
            }

            _context.SaveChanges();
        }

        // Remove an item from the user's cart.
        public void RemoveProductFromCart(int itemId)
        {
            _logger.LogInformation($"Removing product ID={itemId} from cart.");
            var item = _context.CartItems
                .AsNoTracking()
                .FirstOrDefault(i => i.Id == itemId);
    
            var itemToRemove = new CartItem { Id = itemId };
            _context.CartItems.Attach(itemToRemove);
            _context.CartItems.Remove(itemToRemove);
            _context.SaveChanges();
        }

        // Get all items from the user's cart
        public CartDTO GetCartItems()
        {
            _logger.LogInformation("Fetching cart items for current user.");
            var cart = GetUserCart();
    
            return new CartDTO
            {
                Items = cart.Items.Select(item => new CartItemDTO
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    Price = item.ProductPrice,
                    Image = item.Product.ImagePath ?? "/images/default-product.png",
                    Quantity = item.Quantity,
                    Size = item.Size,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };
        }

        //Update the quantity of the cart items
        public void UpdateQuantity(int itemId, int delta)
        {
            var item = _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefault(i => i.Id == itemId);

            if (item == null)
                throw new Exception("Item não encontrado no carrinho");

            item.Quantity += delta;
            
            if (item.Quantity <= 0)
                _context.CartItems.Remove(item);
            else
                item.TotalPrice = item.ProductPrice * item.Quantity;

            _context.SaveChanges();
        }

        //Clear the cart
        public void ClearCart()
        {
            _logger.LogInformation("Clearing the cart of current user.");
            var cart = GetUserCart();
            cart.Items.Clear();
            _context.SaveChanges();
        }
    }
}

public class CartItemDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; }
    public int Quantity { get; set; }
    public string Size { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CartDTO
{
    public List<CartItemDTO> Items { get; set; } = new();
}
