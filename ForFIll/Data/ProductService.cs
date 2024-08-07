using Azure.Core;
using ForFIll.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;



using ForFIll.Data;

using ForFIll.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ForFIll.Data
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public ProductService(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;

        }

        //start crud opreation from sql

        public async Task<DataBaseRequest<IEnumerable<Product>>> GetProductsApi()
        {
            try
            {
                var request = await _context.Products.Where(x=>x.IsDeleted == false).ToListAsync();
                return new DataBaseRequest<IEnumerable<Product>>
                {
                    Data = request,
                    Message = "Product retrieved successfully",
                    Success = true

                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

        }
        public async Task<DataBaseRequest<IEnumerable<Product>>> GetProductsApiByid(int id)
        {
          
            try
            {
                //var request = await _context.Products.Where(p  => p.Id == id).ToListAsync();
                var request = await _context.Products.Where(p=> p.Id ==id && p.IsDeleted == false ).ToListAsync();


                return new DataBaseRequest<IEnumerable<Product>>
                {
                    Data = request,
                    Message = "Product retrieved successfully",
                    Success = true

                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

        }
        public async Task<DataBaseRequest> DeleteProductAsyncsql(int id)
        {
            var request = await GetProductByIdAsync(id);
            var product = request.Success ? request.Data : null;

            try
            {
                _context.Products.Remove(product);

                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    return new DataBaseRequest
                    {
                        Message = $"Product {product.Name} With Id {product.Id} Deleted  Successfully",
                        Success = true
                    };
                }
                else
                {
                    return new DataBaseRequest
                    {
                        Message = $"an error occurred while Deleting {product.Name} ",
                        Success = false
                    };
                }
            }
            catch (Exception)
            {
              
                return new DataBaseRequest
                {
                    Message = "يوجد مشكلة"
                    ,
                    Success = false
                };
            }

        }
        public async Task<DataBaseRequest> CreateProductAsync(Product createProduct)
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

        public async Task<DataBaseRequest<Product>> GetProductByIdAsync(int id)
        {

            var request = await _context.Products.Where(p => p.Id == id).FirstOrDefaultAsync(p=>p.Id == id);
            
            if (request != null)
            {
                return new DataBaseRequest<Product>
                {
                    Data = request,
                    Message = "Product Found!",
                    Success = true,
                };
            }
            else
            {
                return new DataBaseRequest<Product>
                {
                    Data = new Product(),
                    Message = $"The Product with {id} not Found"
                };

            }
        }
     
        public async Task<DataBaseRequest> UpdateProductAsync(int id, Product createProduct)
        {
            var request = await GetProductByIdAsync(id);
            var product = request.Success ? request.Data : null;

            Console.WriteLine(createProduct.IsDeleted);
            product.Name = createProduct.Name;
            product.Price = createProduct.Price;
            product.Category = createProduct.Category;
            product.IsDeleted = createProduct.IsDeleted;

            if (product == null || product.IsDeleted)
            {
                return new DataBaseRequest { Message = ($"Product with ID {id} not found or already deleted."), Success = false };
            }
            try
            {
                _context.Products.Update(product);
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    return new DataBaseRequest
                    {
                        Message = $"Product {product.Name} Deleted Successfully",
                        Success = true
                    };
                }
                else
                {
                    return new DataBaseRequest
                    {
                        Message = $"an error occurred while Deleting {product.Name} ",
                        Success = false
                    };
                }
            }
            catch (Exception)
            {
             
                return new DataBaseRequest
                {
                    Message = "يوجد مشكلة"
                    ,
                    Success = false
                };
            }

        }
        //end crud opreation from sql
        public async Task<List<Product>> GetProducts()
        {
            

            try
            {
                return await _httpClient.GetFromJsonAsync<List<Product>>("api/products");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<Product> GetProduct(int id)
        {
     
            return await _httpClient.GetFromJsonAsync<Product>($"api/products/{id}");
        }

        public async Task<HttpResponseMessage> CreateProduct(Product product)
        {
            Console.WriteLine("create product");
            
            return await _httpClient.PostAsJsonAsync("api/products", product);
        }

        public async Task<HttpResponseMessage> UpdateProduct(int id, Product product)
        {

            return await _httpClient.PutAsJsonAsync($"api/products/{id}", product);
        }

        public async Task<HttpResponseMessage> DeleteProduct(int id)
        {
            return await _httpClient.DeleteAsync($"api/products/{id}");
        }

    }

}
