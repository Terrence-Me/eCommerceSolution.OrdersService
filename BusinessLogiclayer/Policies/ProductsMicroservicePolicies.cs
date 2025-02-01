﻿using BusinessLogiclayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BusinessLogiclayer.Policies;
public class ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger) : IProductsMicroservicePolicies
{
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

                   HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                   {
                       Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                   };
                   return response;
               });
        return policy;
    }
}
