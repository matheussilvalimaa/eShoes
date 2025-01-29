using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using eShoes.Models;
using eShoes.Context;
using Microsoft.AspNetCore.Http.Features;

namespace eShoes.Services
{
    public class OrderService
    {
        private readonly eShoesDbContext _context;
        private readonly UserService _userService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(eShoesDbContext context, UserService userService, ILogger<OrderService> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        //Create an order to send to the database
        public Order CreateOrder(List<OrderItem> items)
        {
            if (items == null || items.Count == 0) 
                throw new Exception("No items to create order.");

            var user = _userService.GetCurrentUser();
            if (user == null)
                throw new Exception("User not logged in");

            var order = new Order(items)
            {
                UserId = user.Id,
                User = user
            };
            
            _logger.LogInformation($"Creating new order for user {user.Username} with {items.Count} item(s).");
            _context.Orders.Add(order);
            _context.SaveChanges();

            _logger.LogInformation($"Order created with ID={order.Id} for user '{user.Username}'.");
            return order;
        }

        //Get the order by the id, throwing an exception if not founding one.
        public Order GetOrder(long id)
        {
            _logger.LogInformation($"Fetching order with ID={id}.");
            var order = _context.Orders
                .Include(o => o.Items) 
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                throw new Exception("Order not found");

            return order;
        }

        //Get all the orders.
        public List<Order> GetAllOrders()
        {
             _logger.LogInformation("Fetching all orders from database.");
            return _context.Orders
                .Include(o => o.Items) 
                .Include(o => o.User)
                .ToList();
        }
    }
}
