using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace BusinessLogiclayer.Policies;
public class PollyPolicies(ILogger<UsersMicroservicePolicies> logger) : IPollyPolicies
{
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(retryCount, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            // Log the outcome of the retry
            logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
        });

        return retryPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handleEventsAllowedbeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> retryPolicy =
       Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
       .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: handleEventsAllowedbeforeBreaking, durationOfBreak: durationOfBreak, onBreak: (outcome, timespan) =>
       {
           // Log the outcome of the retry
           logger.LogInformation($"Circuit breaker opened for {timespan.TotalSeconds} minutes due to consectiev 3 failures. The subsequent requests will be blocked");
       },
       onReset: () =>
       {
           logger.LogInformation("Circuit breaker closed. The subsequent requests will be allowed");
       });

        return retryPolicy;

    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        return timeoutPolicy;
    }

}
