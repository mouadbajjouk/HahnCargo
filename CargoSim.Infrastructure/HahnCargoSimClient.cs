using CargoSim.Application.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CargoSim.Infrastructure;

public class HahnCargoSimClient(HttpClient httpClient, JwtService jwtService)
{
    public async Task<Grid> GetGrid()
    {
        var token = await jwtService.GetAccessTokenAsync();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var gridString = await httpClient.GetStringAsync("Grid/Get");

        return JsonSerializer.Deserialize<Grid>(gridString)!;
    }
}