using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace BusinessLogiclayer.Policies;
public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger) : IUsersMicroservicePolicies
{
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(5, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            // Log the outcome of the retry
            logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
        });

        return retryPolicy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> retryPolicy =
       Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
       .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromMinutes(2), onBreak: (outcome, timespan) =>
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
}
