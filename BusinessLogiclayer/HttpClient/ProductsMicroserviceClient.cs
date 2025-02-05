using BusinessLogiclayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HttpClientAlias = System.Net.Http.HttpClient;

namespace BusinessLogiclayer.HttpClient;
public class ProductsMicroserviceClient
{
    private readonly HttpClientAlias _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _cache;
    public ProductsMicroserviceClient(HttpClientAlias httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = distributedCache;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productID)
    {
        ArgumentNullException.ThrowIfNull(productID);
        try
        {
            string cacheKey = $"product:{productID}";
            string? cachedProduct = await _cache.GetStringAsync(cacheKey);

            if (cachedProduct != null)
            {
                ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                return productFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/products/search/productId/{productID}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    ProductDTO? productFromFallbackPolicy = await response.Content.ReadFromJsonAsync<ProductDTO>();

                    if (productFromFallbackPolicy == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented");
                    }
                    return productFromFallbackPolicy;

                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                }

            }


            ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();

            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }

            string productJson = JsonSerializer.Serialize(product);
            DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions()
                 .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                 .SetSlidingExpiration(TimeSpan.FromSeconds(10));

            string cacheKeyToWrite = $"product:{product.ProductID}";

            await _cache.SetStringAsync(cacheKeyToWrite, productJson, cacheEntryOptions);

            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogWarning(ex, "Bulkhead policy triggered");
            return new ProductDTO
            (
              ProductID: Guid.Empty,
              ProductName: "Product not found",
              Category: "Product not found",
              UnitPrice: 0,
              QuantityInStock: 0
            );
        }
    }
}
