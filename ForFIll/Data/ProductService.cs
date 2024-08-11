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
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace ForFIll.Data
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public ProductService(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
            _passwordHasher = new PasswordHasher<User>();

        }

        //start crud opreation from sql

        public async Task<DataBaseRequest<IEnumerable<Product>>> GetProductsApi()
        {
            try
            {
                var request = await _context.Products.Where(x => x.IsDeleted == false).ToListAsync();
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
        public async Task<DataBaseRequest<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var request = await _context.User.ToListAsync();
                return new DataBaseRequest<IEnumerable<User>>
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

        public async Task<DataBaseRequest> UpdateUserAsync(int id, User createUser)
        {

            var hashedPassword = HashPassword(createUser, createUser.Password);
            createUser.Password = hashedPassword;

            var request = await GetUserByIdAsync(id);
            var user = request.Success ? request.Data : null;

              user.Username = createUser.Username;
              user.Password= createUser.Password;
              user.Password2= createUser.Password;
              user.Token= createUser.Token;
              user.Email= createUser.Email;


            if (user == null )
            {
                return new DataBaseRequest { Message = ($"Product with ID {id} not found or already deleted."), Success = false };
            }
            try
            {
                _context.User.Update(user);
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    return new DataBaseRequest
                    {
                        Message = $"user {user.Username} Updated Successfully",
                        Success = true
                    };
                }
                else
                {
                    return new DataBaseRequest
                    {
                        Message = $"an error occurred while Deleting {user.Username} ",
                        Success = false
                    };
                }
            }
            catch (Exception)
            {

                return new DataBaseRequest
                {
                    Message = "يوجد مشكلة",
                    Success = false
                };
            }

        }


        public async Task<DataBaseRequest<IEnumerable<Product>>> GetProductsApiByid(int id)
        {

            try
            {
                var request = await _context.Products.Where(p => p.Id == id && p.IsDeleted == false).ToListAsync();


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
        public async Task<DataBaseRequest> DeleteFromUserAsync(int id)
        {

            var request = _context.User.Where(x => x.Id== id).FirstOrDefault();
            try
            {
                _context.User.Remove(request);

                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    return new DataBaseRequest
                    {
                        Message = $"Product {request.Username} With Id {request.Password} Deleted  Successfully",
                        Success = true
                    };
                }
                else
                {
                    return new DataBaseRequest
                    {
                        Message = $"an error occurred while Deleting {request.Username} ",
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
        
    
        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }
        public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
        public async Task<DataBaseRequest> CreateUserAsync(User createuser)
        {
          
            var hashedPassword = HashPassword(createuser, createuser.Password);
            createuser.Password = hashedPassword;


            var request = await _context.User.Where(p => p.Username == createuser.Username).FirstOrDefaultAsync();
            if (request != null)
            {

                return new DataBaseRequest
                {
                    Message = "Username allready exist",
                    Success = false
                };
            }

            var requestEmail = await _context.User.Where(p => p.Email== createuser.Email).FirstOrDefaultAsync();
            if (requestEmail != null)
            {

                return new DataBaseRequest
                {
                    Message = "Email allready exist",
                    Success = false
                };
            }


            User user = new User
            {
                Username = createuser.Username,
                Password = createuser.Password,
                Password2 = createuser.Password,
                Email = createuser.Email,
                Token = createuser.Token,
            };
            _context.User.Add(user);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return new DataBaseRequest
                {
                    Message = "User Added Successfully",
                    Success = true
                };
            }
            else
            {
                return new DataBaseRequest
                {
                    Message = "Error occurred while adding new User",
                    Success = false
                };
            }

        }


        public async Task<DataBaseRequest<Product>> GetProductByIdAsync(int id)
        {

            var request = await _context.Products.Where(p => p.Id == id).FirstOrDefaultAsync(p => p.Id == id);

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
        public async Task<DataBaseRequest<User>> GetUserByIdAsync(int id)
        {

            var request = await _context.User.Where(p => p.Id == id).FirstOrDefaultAsync(p => p.Id == id);

            if (request != null)
            {
                return new DataBaseRequest<User>
                {
                    Data = request,
                    Message = "Product Found!",
                    Success = true,
                };
            }
            else
            {
                return new DataBaseRequest<User>
                {
                    Data = new User(),
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
