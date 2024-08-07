using Azure.Core;
using ForFIll.Data;
using ForFIll.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;





namespace ForFIll.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        //private static List<Product> Products = new List<Product>();

        private readonly ApplicationDbContext _context;
        private static List<Product> Products = new List<Product>
        {
            //new Product { Id = 1, Name = "Product 1", Price = 10.99M, Category = "Category 1" },
            //new Product { Id = 2, Name = "Product 2", Price = 20.99M, Category = "Category 2" },
            //new Product { Id = 3, Name = "Product 3", Price = 21.99M, Category = "Category 3" },
            //new Product { Id = 4, Name = "Product 4", Price = 40.99M, Category = "Category 4" },

        };
        public ProductsController( ApplicationDbContext context)
        {
            _context = context;

       
        }
        
        [HttpGet]
        public IEnumerable<Product> GetProducts()
        {
            try
            {
                var request  = _context.Products.Where(x => x.IsDeleted == false).ToListAsync();
                Products  = request.Result;
                Console.WriteLine("GetProducts");
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
            try
            {
                ActionResult<Product> request = _context.Products.Where(x => x.IsDeleted == false && x.Id == id).FirstOrDefault();

                if (request == null)
                {
                    return NotFound();
                }
                Console.WriteLine("GetProductsById");
                return request;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
        }

        [HttpPost]
 
        public async Task<DataBaseRequest> PostProduct(Product createProduct)
        {
            Product product = new Product
            {
                Name = createProduct.Name,
                Price = createProduct.Price,
                Category = createProduct.Category,
                IsDeleted = createProduct.IsDeleted,

            };
            _context.Products.Add(product);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return new DataBaseRequest
                {
                    Message = "Product Added Successfully",
                    Success = true
                };
            }
            else
            {
                return new DataBaseRequest
                {
                    Message = "Error occurred while adding new product",
                    Success = false
                };
            }

        }

        [HttpPut("{id}")]

        public async Task<DataBaseRequest> PutProduct(int id, Product createProduct)
        {
            var request = _context.Products.Where(x => x.IsDeleted == false && x.Id == id).FirstOrDefault();
          
            if (request == null || request.IsDeleted)

            {
                
                return new DataBaseRequest { Message = ($"Product with ID {id} not found or get Deleted."), Success = false };
            }
            request.Name = createProduct.Name;
            request.Price = createProduct.Price;
            request.Category = createProduct.Category;
            request.IsDeleted = createProduct.IsDeleted;
            try
            {
                _context.Products.Update(request);
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    return new DataBaseRequest
                    {
                        Message = $"Product wtih Name :  {request.Name} And  ID  : {request.Id} Updated Successfully",
                        Success = true
                    };
                }
                else
                {
                    return new DataBaseRequest
                    {
                        Message = $"an error occurred while Updated {request.Name} ",
                        Success = false
                    };
                }
            }
            catch (Exception)
            {
               
                return new DataBaseRequest
                {
                    Message = "يوجد سلة لهذا المنتج"
                    ,
                    Success = false
                };
            }

        }
        [HttpDelete("{id}")]
    
        public async Task<DataBaseRequest> DeleteProduct(int id)
        {
    
            var request = _context.Products.Where(x => x.IsDeleted == false && x.Id == id).FirstOrDefault();
            try
            {
                _context.Products.Remove(request);

                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    return new DataBaseRequest
                    {
                        Message = $"Product {request.Name} with Id {request.Id} Deleted Successfully",
                        Success = true
                    };
                }
                else
                {
                    return new DataBaseRequest
                    {
                        Message = $"an error occurred while Deleting {request.Name} ",
                        Success = false
                    };
                }
            }
            catch (Exception)
            {
                
                return new DataBaseRequest
                {
                    Message = "هذا المنتج غير متوفر"
                    ,
                    Success = false
                };
            }

        }
       
    }
}

