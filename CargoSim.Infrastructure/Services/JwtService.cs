using CargoSim.Application.Abstractions.Services;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CargoSim.Infrastructure.Services;

public class JwtService(HttpClient httpClient, IStateService stateService)
{
    public async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(stateService.JwtToken))
        {
            return stateService.JwtToken;
        }

        var loginData = new { Username = "Mouad", Password = "Hahn" };

        var json = JsonSerializer.Serialize(loginData);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync("User/Login", content);

        response.EnsureSuccessStatusCode();

        string jwtToken = (await response.Content.ReadFromJsonAsync<TokenResponse>())!.Token;

        stateService.JwtToken = jwtToken;

        return jwtToken;
    }

}

public record TokenResponse(string Username, string Token);
