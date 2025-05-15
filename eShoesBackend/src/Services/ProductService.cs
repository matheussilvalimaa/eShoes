using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using eShoes.Models;
using eShoes.Context;
using Microsoft.AspNetCore.Mvc;

namespace eShoes.Services
{
    public class ProductService
    {
       private readonly eShoesDbContext _context;
       public ProductService(eShoesDbContext context)
       {
            _context = context;
       }

        //Return all the products
        public List<Product> GetAllProducts()
        {
           return _context.Products.ToList();
        }

        //Get the product by the id, throwing an exception if not finding one.
        public Product GetProductById(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new Exception("Product not found");

            return product;
        }

        public List<Product> GetFeaturedProducts()
        {
            return _context.Products
                .Where(p => p.Id == 5 || p.Id == 11 || p.Id == 3 || p.Id == 20)
                .ToList();
        }
    }
}
