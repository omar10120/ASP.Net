using Azure;
using Azure.Core;
using ForFIll.Data;
using ForFIll.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;  // Add this to use IHttpContextAccessor






namespace ForFIll.Controllers
{
    [Route("api/[controller]")]
    [ApiController]



    public class ProductsController : ControllerBase
    {
        //private static List<Product> Products = new List<Product>();
        //private readonly ProductService _passwordService; // Assuming you have a service for password verification

        private readonly ApplicationDbContext _context;
        private static List<Product> Products = new List<Product>();
        private static List<User> Users = new List<User>();
        private readonly HttpClient _httpClient;
        private string _token;
        private readonly IHttpContextAccessor _httpContextAccessor;



        private readonly PasswordHasher<User> _passwordHasher;

        public ProductsController( ApplicationDbContext context , HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _context = context; 
            _passwordHasher = new PasswordHasher<User>();
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

        }



        [Authorize]
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


        [Authorize]
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
        [Authorize]
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
        [HttpPost("Register")]

        public async Task<DataBaseRequest> CreateUserAsync(User createuser)
        {

            var hashedPassword = HashPassword(createuser, createuser.Password);
            createuser.Password = hashedPassword;
            createuser.Password2 = hashedPassword;


            var request = await _context.User.Where(p => p.Username == createuser.Username).FirstOrDefaultAsync();
            if (request != null)
            {

                return new DataBaseRequest
                {
                    Message = "Username Allready Exists ",
                    Success = false
                };
            }

            var requestEmail = await _context.User.Where(p => p.Email == createuser.Email).FirstOrDefaultAsync();
            if (requestEmail != null)
            {

                return new DataBaseRequest
                {
                    Message = "Email allready exist",
                    Success = false
                };
            }

            _context.User.Add(createuser);
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
        [Authorize]
        [HttpPut("{id}/updateUser")]


        public async Task<DataBaseRequest> UpdateUserAsync(int id, User createUser)
        {

            var hashedPassword = HashPassword(createUser, createUser.Password);
            createUser.Password = hashedPassword;

            var request = await GetUserByIdAsync(id);
            var user = request.Success ? request.Data : null;

            user.Username = createUser.Username;
            user.Password = createUser.Password;
            user.Password2 = createUser.Password;
            user.Token = createUser.Token;
            user.Email = createUser.Email;
            user.AllowAdd = createUser.AllowAdd;
            user.AllowEditUser = createUser.AllowEditUser;
            user.AllowEdit = createUser.AllowEdit;


            if (user == null)
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

        [HttpPost("login")]  // This will map to POST /api/auth/login
        public async Task<IActionResult> Login([FromBody] LoginModel loginRequest)
        {

            Console.WriteLine(loginRequest.Username);
            Console.WriteLine(loginRequest.Password);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if the user exists
                var user = await _context.User
                    .FirstOrDefaultAsync(x => x.Username == loginRequest.Username || x.Email == loginRequest.Username);



                if (user == null)
                {
                    return Unauthorized("Invalid username or password");
                }

                // Verify the password

                var isValidPassword = VerifyPassword(user, user.Password, loginRequest.Password);
                if (!isValidPassword)
                {
                    return Unauthorized("Invalid username or password");
                }
                //start add
                // Generate a JWT Token for API-based clients like Postman
                var token = GenerateToken(user);  // This method should generate a valid JWT

                // Create claims for the user (needed for cookie-based auth)
                var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Email, user.Email),
                            // Add other claims here if needed (e.g., roles, permissions)
                        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Sign the user in using cookie-based authentication (for browser clients)
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // If you want the cookie to persist across browser sessions
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // Set expiration time for the cookie
                };

                // This will issue a cookie and sign the user in
                //await HttpContextAccessor.HttpContext.SignInAsync(
                //    CookieAuthenticationDefaults.AuthenticationScheme,
                //    new ClaimsPrincipal(claimsIdentity),
                //    authProperties
                //);

                //// Notify the app that the user is authenticated (used for Blazor Server apps)
                //((CustomAuthenticationStateProvider)AuthenticationStateProvider)
                //    .NotifyUserAuthentication(new ClaimsPrincipal(claimsIdentity));

                //var userIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
                //var userName = _httpContextAccessor.HttpContext.User.Identity.Name;


                //end add
                // Return token and any user information needed

                return Ok(new
                {
                    Token = token,
                    Username = user.Username,
                    AllowEdit = user.AllowEdit,
                    AllowAdd = user.AllowAdd,
                    AllowEditUser = user.AllowEditUser,
                    HttpMessage = "Login Done Successfully"
                });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }
        private string GenerateToken(User user)
        {

            // Implement your JWT token generation logic here.
            // Use libraries like System.IdentityModel.Tokens.Jwt for this purpose.

            // Here’s a simplified example (you should replace this with actual JWT generation):
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("securitykey16with256bayite2323512"); // Replace with a secure key

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            //edit token
            // Implement your JWT token generation logic here.
            // This is just a placeholder.
            return ".AspNetCore.Cookies=CfDJ8APoXJoiMH5LksVPQVo7OVUJKqM7eUqjgrMEGCFNx9W17gRta3inqqDGvYyfGFaSivpaxsZdeenjQDW4paN8JIMsxXGkkAgGeBldivZCxORrjxUZKrzxsq81nknj9O0Stxnwg_6P-dX4cdZoOvmxZDdw7vmVaw5_9440Voz42xKi0y1Cjvz_q-g0JuwuW9Q8hbrF_h9Cu6eC9tucozRTGvVWYEGUMQC5E3NDixnshjvHTjOMXSMhqSOMa5IFqtPl3wSvY2QDpyU51eabzVeryBS8eeAHTMQUicSh3wj4F7TwgkpI1HmEsS0sYlJDHsBZ4wVnELvhU9ExG9KTeyJGlp5sANPgSH7lfRFg7FIzS4lSAW3ZqJfwBCw5-WfM8GyxnU_YvSO3Vgt9940x3mXI2AhhQesFNv5QRGflSAGG6od2lT7PB9BKSOXv1d3jbwb9XQ"; // Replace with actual token generation logic
        }

        




        [Authorize]
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
        [Authorize]
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

        //start check VerifyPassword 
        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }
        public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
        //end check VerifyPassword 


    }
}

