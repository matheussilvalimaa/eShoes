using eShoes.Models;
using eShoes.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderService orderService, CartService cartService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _logger = logger;
        }

        //Get all orders
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            _logger.LogInformation("GET /api/order -> fetching all orders.");
            var orders = _orderService.GetAllOrders();
            return Ok(orders);
        }

        //Get an specific order
        [HttpGet("{id}")]
        public IActionResult GetOrder(long id)
        {
            try
            {
                _logger.LogInformation($"GET /api/order/{id}");
                var order = _orderService.GetOrder(id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        //Create a order
        [HttpPost("create")]
        public IActionResult CreateOrder()
        {
            try
            {
                _logger.LogInformation("POST /api/order/create -> creating order from cart items.");
                var cartItems = _cartService.GetCartItems();
                if (cartItems == null)
                    return BadRequest("Cart is empty");
                
                var orderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.ProductName,
                    ProductPrice = ci.ProductPrice,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.TotalPrice
                }).ToList();

                var order = _orderService.CreateOrder(orderItems);

                _cartService.ClearCart();

                _logger.LogInformation($"Order created successfully. ID={order.Id}");
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}