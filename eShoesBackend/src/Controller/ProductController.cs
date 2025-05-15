using Microsoft.AspNetCore.Mvc;
using eShoes.Services;
using eShoes.Models;
using Microsoft.AspNetCore.Authorization;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("/api/products")]
    [AllowAnonymous]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;
        public ProductController(ProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        //Return all the products.
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            _logger.LogInformation("GET /api/product -> fetching all products.");
            var products = _productService.GetAllProducts();
            return Ok(products);
        }

        //Return an specific product by the id.
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            try
            {
                _logger.LogInformation($"GET /api/product/{id}");
                var product = _productService.GetProductById(id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        //Get the featured products to show in the index.html
        [HttpGet("featured")]
        public IActionResult GetFeaturedProducts()
        {
            var products = _productService.GetFeaturedProducts();
            return Ok(products);
        }
    }
}