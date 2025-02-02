using BusinessLogiclayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using Polly.Wrap;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BusinessLogiclayer.Policies;
public class ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger) : IProductsMicroservicePolicies
{
    public IAsyncPolicy<HttpResponseMessage> GetBuilkheadPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 2, maxQueuingActions: 40, onBulkheadRejectedAsync: (context) =>
        {
            logger.LogWarning("Bulkhead policy triggered");
            throw new BulkheadRejectedException("Bulkhead policy triggered");
        });
        return bulkheadPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
               .FallbackAsync(async (context) =>
               {
                   logger.LogWarning("Fallback policy triggered");

                   ProductDTO product = new ProductDTO
                   (
                       ProductID: Guid.Empty,
                       ProductName: "Product not found",
                       Category: "Product not found",
                       UnitPrice: 0,
                       QuantityInStock: 0
                   );

                   HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                   {
                       Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                   };
                   return response;
               });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var bulkheadPolicy = GetBuilkheadPolicy();
        var fallbackPolicy = GetFallbackPolicy();

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(bulkheadPolicy, fallbackPolicy);

        return wrappedPolicy;
    }
}
