using Polly;

namespace BusinessLogiclayer.Policies;
public interface IProductsMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();

    IAsyncPolicy<HttpResponseMessage> GetBuilkheadPolicy();

    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}
