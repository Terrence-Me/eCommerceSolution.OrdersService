using BusinessLogiclayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HttpClientAlias = System.Net.Http.HttpClient;

namespace BusinessLogiclayer.HttpClient;
public class UsersMicroserviceClient
{
    private readonly HttpClientAlias _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _cache;

    public UsersMicroserviceClient(HttpClientAlias httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userId)
    {

        ArgumentNullException.ThrowIfNull(userId);
        try
        {
            string cacheKeyToRead = $"user:{userId}";
            string? cachedUser = await _cache.GetStringAsync(cacheKeyToRead);
            if (cachedUser != null)
            {
                UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);
                return userFromCache;
            }


            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/Users/{userId}");
            //HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? userFromFallbackPolicy = await response.Content.ReadFromJsonAsync<UserDTO>();

                    if (userFromFallbackPolicy == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented");
                    }
                    return userFromFallbackPolicy;

                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                }
                else
                {
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");

                    // this is situational depenend, if you actually want to return dummy data. 
                    return new UserDTO(

                        PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        UserId: Guid.Empty,
                        Gender: "Temporarily Unavailable"
                        );



                }

            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            var cacheWriteKey = $"user:{userId}";
            string userJson = JsonSerializer.Serialize(user);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));
            await _cache.SetStringAsync(cacheWriteKey, userJson, options);


            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because circuit breaker opened. Returning dummy data");
            return new UserDTO(

                         PersonName: "Temporarily Unavailable(Circuit breaker)",
                         Email: "Temporarily Unavailable",
                         UserId: Guid.Empty,
                         Gender: "Temporarily Unavailable"
                         );
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout exception occured. Returning dummy data");

            return new UserDTO(
                PersonName: "Temporarily Unavailable (Timeout)",
                Email: "Temporarily Unavailable",
                UserId: Guid.Empty,
                Gender: "Temporarily Unavailable"
                );
        }
    }
}
