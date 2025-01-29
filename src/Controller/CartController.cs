using eShoes.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;
        private readonly ILogger<CartController> _logger;

        public CartController(CartService cartService, ProductService productService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        //Return all items from the current user's cart
        [HttpGet]
        public IActionResult GetCartItems()
        {
            try
            {
                 _logger.LogInformation("GET /api/cart -> GetCartItems called.");
                var items = _cartService.GetCartItems();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Add a product in the cart
        [HttpPost("add/{productId}")]
        public IActionResult AddToCart(int productId, [FromBody] int quantity)
        {
            try
            {
                _logger.LogInformation($"POST /api/cart/add/{productId} -> quantity={quantity}");
                var product = _productService.GetProductById(productId);
                _cartService.AddProductCart(product, quantity);

                return Ok("Product added to cart");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Remove the product from the cart
        [HttpDelete("remove/{productId}")]
        public IActionResult RemoveFromCart(long productId)
        {
            try
            {
                 _logger.LogInformation($"DELETE /api/cart/remove/{productId}");
                _cartService.RemoveProductFromCart(productId);
                return Ok("Product removed from cart");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Clear the cart
        [HttpDelete("clear")]
        public IActionResult ClearCart()
        {
            try
            {
                _logger.LogInformation("DELETE /api/cart/clear -> clearing cart.");
                _cartService.ClearCart();
                return Ok("Cart cleared");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}