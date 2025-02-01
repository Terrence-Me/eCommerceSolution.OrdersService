using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace BusinessLogiclayer.Policies;
public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger, IPollyPolicies pollyPolicies) : IUsersMicroservicePolicies
{


    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = pollyPolicies.GetRetryPolicy(5);
        var circuitBreakerPolicy = pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
        var timeoutPolicy = pollyPolicies.GetTimeoutPolicy(TimeSpan.FromMinutes(5));

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

        return wrappedPolicy;
    }
}
