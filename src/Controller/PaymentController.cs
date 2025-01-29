using System.Threading.RateLimiting;
using eShoes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(CartService cartService, ILogger<PaymentController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        //Creates a PaymentIntent using Stripe's API.
        [HttpPost("create-payment-intent")]
        [Authorize]
        public IActionResult CreatePaymentIntent()
        {
            try
            {
                _logger.LogInformation("POST /api/payment/create-payment-intent -> Creating PaymentIntent.");
                var cart = _cartService.GetUserCart();
                if (cart == null)
                    return BadRequest("Cart is empty");
                
                var totalAmount = cart.Items.Sum(i => i.TotalPrice);
                var amountInCents = (long)(totalAmount * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = "BRL",
                };
                var service = new PaymentIntentService();
                var paymentIntent = service.Create(options);

                _logger.LogInformation($"PaymentIntent created: ID={paymentIntent.Id}");
                return Ok(new { clientSecret = paymentIntent.ClientSecret});
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}