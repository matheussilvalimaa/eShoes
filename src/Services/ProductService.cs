using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using eShoes.Models;
using eShoes.Context;

namespace eShoes.Services
{
    public class ProductService
    {
        private readonly eShoesDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(eShoesDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Return all the products
        public List<Product> GetAllProducts()
        {
            var products = _context.Products.ToList();
            _logger.LogInformation($"Returning {products.Count} products");
            return products;
        }

        //Get the product by the id, throwing an exception if not finding one.
        public Product GetProductById(int id)
        {
            _logger.LogInformation($"Searching product with the ID {id}");
            var product = _context.Products.Find(id);
            if (product == null)
                throw new Exception("Product not found");

            return product;
        }

        //Creates a new product and put in the database.
        public Product CreateProduct(Product product)
        {
            _logger.LogInformation($"Creating product with name = {product.Name} and price = {product.Price}");

            if (product.Price < 0)
                throw new Exception("Price cannot be negative");
            if (product.InventQuantity < 0)
                throw new Exception("Invalid quantity");

            _context.Products.Add(product);
            _context.SaveChanges();
            _logger.LogInformation($"Product created with success!. ID={product.Id}");
            return product;
        }

        //Updates the stock
        public void UpdateStock(int productId, int quantityToRemove)
        {
            var product = GetProductById(productId);
            var newQuantity = product.InventQuantity - quantityToRemove;
            if (newQuantity < 0)
                throw new Exception($"Insufficient stock for product {product.Name}");

            product.InventQuantity = newQuantity;
            _context.SaveChanges();
            _logger.LogInformation($"Stock updated for the product ID = {product.Id}, Name = {product.Name}");
        }

        //Updates the product
        public void UpdateProduct(Product product)
        {
            _logger.LogInformation($"Updating product ID = {product.Id}");
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        //Deletes the product
        public void DeleteProduct(int productId)
        {
            var product = GetProductById(productId);
            _context.Products.Remove(product);
            _context.SaveChanges();
            _logger.LogInformation($"Product ID = {productId} removed from database");
        }
    }
}
