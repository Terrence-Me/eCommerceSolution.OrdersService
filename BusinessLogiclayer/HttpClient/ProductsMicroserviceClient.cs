using BusinessLogiclayer.DTO;
using System.Net;
using System.Net.Http.Json;
using HttpClientAlias = System.Net.Http.HttpClient;

namespace BusinessLogiclayer.HttpClient;
public class ProductsMicroserviceClient
{
    private readonly HttpClientAlias _httpClient;
    public ProductsMicroserviceClient(HttpClientAlias httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productID)
    {
        ArgumentNullException.ThrowIfNull(productID);

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
}
