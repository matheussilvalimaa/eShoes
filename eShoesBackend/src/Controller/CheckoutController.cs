using eShoes.Services;
using eShoes.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace eShoes.Controller
{
    [ApiController]
    [Route("api/checkout")]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly CheckoutService _checkoutService;
        public CheckoutController(CheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CheckoutRequest rq)
        {
            try
            {
                var result = await _checkoutService.ProcessCheckout(rq);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("shipping/{state}")]
        public IActionResult GetShippingCost(string state)
        {
            try
            {
                var cost = _checkoutService.CalculateShippingCost(state);
                return Ok(new { shippingCost = cost });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("payments/{paymentId}/status")]
        public async Task<ActionResult> GetPixStatus(string paymentId)
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentId); 
            
            return Ok(new {
                status = paymentIntent.Status 
            });
        }
    }
}