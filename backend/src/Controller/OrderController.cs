using eShoes.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eShoes.DTO;
using eShoes.Services;

namespace eShoes.Controllers 
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly eShoesDbContext _db;
        private readonly UserService     _userService;

        public OrderController(eShoesDbContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            var orders = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderHistoryDto {
                    Id             = o.Id,
                    OrderDate      = o.OrderDate,
                    TotalProducts  = o.TotalProducts,
                    ShippingCost   = o.ShippingCost,
                    TotalPrice     = o.TotalPrice,
                    Status         = o.Status,
                    PaymentMethodType = o.PaymentMethodType,
                    Last4 = o.Last4,
                    Items = o.Items.Select(i => new OrderItemDto {
                        ProductName = i.ProductName,
                        Quantity    = i.Quantity,
                        Price       = i.Price
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
    }
}