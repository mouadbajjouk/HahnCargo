using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Models;
using CargoSim.Infrastructure.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CargoSim.Infrastructure.Clients;

public class HahnCargoSimClient(HttpClient httpClient, JwtService jwtService) : IHahnCargoSimClient
{
    public async Task<Grid> GetGrid()
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var gridString = await httpClient.GetStringAsync("Grid/Get");

        return JsonSerializer.Deserialize<Grid>(gridString)!;
    }

    public async Task<int> GetCoinAmount()
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var coinAmountString = await httpClient.GetStringAsync("User/CoinAmount");

        if (int.TryParse(coinAmountString, out int coinAmount))
            return coinAmount;

        throw new InvalidOperationException("Can't get coin amount!");
    }
}