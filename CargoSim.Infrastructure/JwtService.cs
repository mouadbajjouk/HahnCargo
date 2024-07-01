using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CargoSim.Infrastructure;

public class JwtService(HttpClient httpClient)
{
    public async Task<string> GetAccessTokenAsync()
    {
        var loginData = new { Username = "Mouad", Password = "Hahn" };

        var json = JsonSerializer.Serialize(loginData);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync("User/Login", content);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!.Token;
    }

}

public record TokenResponse(string Username, string Token);
