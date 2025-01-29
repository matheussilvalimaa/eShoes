using Microsoft.AspNetCore.Mvc;
using eShoes.Services;
using eShoes.Models;
using Microsoft.AspNetCore.Authorization;

namespace eShoes.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize]
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
        [AllowAnonymous]
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

        //Create a new product
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            try
            {
                _logger.LogInformation($"POST /api/product -> creating product '{product.Name}'");
                var createdProduct = _productService.CreateProduct(product);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id}, createdProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Update a product in the stock.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updated)
        {
            try
            {
                _logger.LogInformation($"PUT /api/product/{id} -> updating product");
                var product = _productService.GetProductById(id);

                product.Name = updated.Name;
                product.Description = updated.Description;
                product.Price = updated.Price;
                product.InventQuantity = updated.InventQuantity;

                _productService.UpdateProduct(product);

                return Ok(product);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Delete a product from the stock.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                _logger.LogInformation($"DELETE /api/product/{id}");
                _productService.DeleteProduct(id);
                return Ok($"Product {id} deleted successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}