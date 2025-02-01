using BusinessLogiclayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net;
using System.Net.Http.Json;
using HttpClientAlias = System.Net.Http.HttpClient;

namespace BusinessLogiclayer.HttpClient;
public class ProductsMicroserviceClient
{
    private readonly HttpClientAlias _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    public ProductsMicroserviceClient(HttpClientAlias httpClient, ILogger<ProductsMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productID)
    {
        ArgumentNullException.ThrowIfNull(productID);
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/productId/{productID}");
            if (!response.IsSuccessStatusCode)
            {
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
