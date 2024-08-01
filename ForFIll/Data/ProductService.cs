using ForFIll.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;




namespace ForFIll.Data
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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
