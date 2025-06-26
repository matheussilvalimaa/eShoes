using eShoes.Services;
using eShoes.DTO;
using Microsoft.AspNetCore.Mvc;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("api/cart")]
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
        [HttpGet("items")]
        public IActionResult GetCartItems()
        {
            try
            {
                 _logger.LogInformation("GET /api/cart -> GetCartItems called.");
                var cartDTO = _cartService.GetCartItems();
                return Ok(cartDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Add a product in the cart
        [HttpPost("add/{productId}")]
        public IActionResult AddToCart(int productId, [FromBody] AddToCartRequest request)
        {
            try
            {
                _logger.LogInformation($"POST /api/cart/add/{productId} -> quantity={request.Quantity}");
                var product = _productService.GetProductById(productId);
                _cartService.AddProductCart(product, request.Quantity, request.Size);

                return Ok("Product added to cart");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Get a ID from the cart
        [HttpGet("{itemId}")]
        public IActionResult GetCartItemById(int itemId)
        {
            try
            {
                _logger.LogInformation($"GET /api/cart/items/{itemId} -> GetCartItemById called for itemId: {itemId}");
                var cartItemDTO = _cartService.GetCartItemById(itemId);
                if (cartItemDTO == null)
                {
                    return NotFound(); 
                }
                return Ok(cartItemDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Remove the product from the cart
        [HttpDelete("{itemId}")]
        public IActionResult RemoveFromCart(int itemId)
        {
            try
            {
                _logger.LogInformation($"DELETE /api/cart/remove/{itemId}");
                _cartService.RemoveProductFromCart(itemId);
                return Ok("Product removed from cart");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Update Quantity
        [HttpPatch("{itemId}")]
        public IActionResult UpdateQuantity(int itemId, [FromBody] QuantityUpdateRequest request)
        {
            try
            {
                _cartService.UpdateQuantity(itemId, request.Delta);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}