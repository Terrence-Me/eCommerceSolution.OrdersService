using BusinessLogiclayer.DTO;
using System.Net;
using System.Net.Http.Json;
using HttpClientAlias = System.Net.Http.HttpClient;

namespace BusinessLogiclayer.HttpClient;
public class UsersMicroserviceClient
{
    private readonly HttpClientAlias _httpClient;

    public UsersMicroserviceClient(HttpClientAlias httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

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
                throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
            }

        }

        UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }


        return user;
    }
}
