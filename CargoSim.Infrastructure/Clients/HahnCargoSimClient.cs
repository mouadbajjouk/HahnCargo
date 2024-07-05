using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Models;
using CargoSim.Infrastructure.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    public async Task<int> BuyTransporter(int atPosition)
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsync($"CargoTransporter/Buy?positionNodeId={atPosition}", null); // TODO: position node isn't always 0 !!!

        response.EnsureSuccessStatusCode();

        var transporterIdString = await response.Content.ReadAsStringAsync();

        if (int.TryParse(transporterIdString, out int transporterId))
            return transporterId;

        throw new InvalidOperationException("Can't get transporter ID!");
    }

    public async Task<Transporter?> GetTransporter(int id)
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await httpClient.GetFromJsonAsync<Transporter>($"CargoTransporter/Get?transporterId={id}");
    }

    public async Task StartSimulation()
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsync("Sim/Start", null); // TODO: position node isn't always 0 !!!

        response.EnsureSuccessStatusCode();
    }

    public async Task StopSimulation()
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsync("Sim/Stop", null); // TODO: position node isn't always 0 !!!

        response.EnsureSuccessStatusCode();
    }

    public async Task Move(int transporterId, int targetNodeId)
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PutAsync($"CargoTransporter/Move?transporterId={transporterId}&targetNodeId={targetNodeId}", null); // TODO: position node isn't always 0 !!!

        response.EnsureSuccessStatusCode();
    }

    public async Task AcceptOrder(int orderId)
    {
        var token = await jwtService.GetAccessTokenAsync(); // TODO: centralize jwt token

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsync($"Order/Accept?orderId={orderId}", null);

        response.EnsureSuccessStatusCode();
    }
}