using Polly;

namespace BusinessLogiclayer.Policies;
public interface IProductsMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
}
