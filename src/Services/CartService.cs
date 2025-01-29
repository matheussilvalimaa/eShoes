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
                throw new Exception("No user logged in");

            var cart = _context.Carts
                .Include(c => c.Items)
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

        // Add a product in the user's cart. If already exists, updates the quantity.
        public void AddProductCart(Product product, int quantity)
        {
            _logger.LogInformation($"Adding product ID={product.Id}, Quantity={quantity} to cart.");
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
                    TotalPrice = product.Price * quantity,
                    Cart = cart
                };
                cart.Items.Add(newItem);
                _logger.LogInformation($"Added new cart item for product ID={product.Id}.");
            }

            _context.SaveChanges();
        }

        // Remove an item from the user's cart.
        public void RemoveProductFromCart(long productId)
        {
             _logger.LogInformation($"Removing product ID={productId} from cart.");
            var cart = GetUserCart();
            cart.Items.RemoveAll(item => item.ProductId == productId);
            _context.SaveChanges();
        }

        // Get all items from the user's cart
        public List<CartItem> GetCartItems()
        {
            _logger.LogInformation("Fetching cart items for current user.");
            return GetUserCart().Items;
        }

        // Clear the cart
        public void ClearCart()
        {
            _logger.LogInformation("Clearing the cart of current user.");
            var cart = GetUserCart();
            cart.Items.Clear();
            _context.SaveChanges();
        }
    }
}
