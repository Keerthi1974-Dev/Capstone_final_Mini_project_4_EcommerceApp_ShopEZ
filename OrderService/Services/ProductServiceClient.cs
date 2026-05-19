using System.Net.Http.Headers;
using OrderService.DTOs;

namespace OrderService.Services
{
    /// <summary>
    /// Calls the ProductService microservice to fetch product info during order creation.
    /// </summary>
    public class ProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(
            HttpClient httpClient,
            ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public virtual async Task<ProductInfoDTO?> GetProductAsync(
            int productId,
            string? bearerToken = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", bearerToken);
                }

                var response =
                    await _httpClient.GetAsync($"/api/products/{productId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Product {ProductId} not found in ProductService",
                        productId);

                    return null;
                }

                return await response.Content
                    .ReadFromJsonAsync<ProductInfoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error calling ProductService for product {ProductId}",
                    productId);

                return null;
            }
        }
    }
}