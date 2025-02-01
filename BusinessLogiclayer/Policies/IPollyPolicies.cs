using Polly;

namespace BusinessLogiclayer.Policies;
public interface IPollyPolicies
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount);

    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handleEventsAllowedbeforeBreaking, TimeSpan durationOfBreak);

    IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout);

}
