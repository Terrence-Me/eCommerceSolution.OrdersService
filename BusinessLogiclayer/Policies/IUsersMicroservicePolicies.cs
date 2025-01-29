using Polly;

namespace BusinessLogiclayer.Policies;
public interface IUsersMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
}
