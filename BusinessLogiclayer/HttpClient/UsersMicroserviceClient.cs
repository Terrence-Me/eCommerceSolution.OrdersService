using BusinessLogiclayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http.Json;
using HttpClientAlias = System.Net.Http.HttpClient;

namespace BusinessLogiclayer.HttpClient;
public class UsersMicroserviceClient
{
    private readonly HttpClientAlias _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;

    public UsersMicroserviceClient(HttpClientAlias httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userId)
    {

        ArgumentNullException.ThrowIfNull(userId);
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/Users/{userId}");
            //HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userId}");
            if (!response.IsSuccessStatusCode)
            {
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
