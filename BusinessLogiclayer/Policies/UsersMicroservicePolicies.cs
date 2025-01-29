using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BusinessLogiclayer.Policies;
public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger) : IUsersMicroservicePolicies
{
    //private readonly ILogger<UsersMicroservicePolicies> _logger = logger;

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(5, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2), onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            // Log the outcome of the retry
            logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
        });

        return retryPolicy;
    }
}
