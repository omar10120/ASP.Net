using ForFIll.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
namespace ForFIll.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProductsController : ControllerBase
    {
        private static  List<Product> Products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.99M, Category = "Category 1" },
            new Product { Id = 2, Name = "Product 2", Price = 20.99M, Category = "Category 2" },
            new Product { Id = 3, Name = "Product 3", Price = 21.99M, Category = "Category 3" },
            new Product { Id = 4, Name = "Product 4", Price = 40.99M, Category = "Category 4" },
        };

        [HttpGet]
        public IEnumerable<Product> GetProducts()
        {
            try
            {
                return Products;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            Console.WriteLine("Product");
            try
            {

                var product = Products.FirstOrDefault(p => p.Id == id);
                if (product == null)
                {
                    return NotFound();
                }
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [HttpPost]
        public ActionResult<Product> PostProduct(Product product)
        {
            product.Id = Products.Max(p => p.Id) + 1;
            Products.Add(product);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult PutProduct(int id, Product product)
        {
            var existingProduct = Products.FirstOrDefault(p => p.Id == id);
            if (existingProduct == null)
            {
                return NotFound();
            }
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            Console.WriteLine("delete");
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            Products.Remove(product);
            return NoContent();
        }
        
    }
}

