using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Clients;

public interface IHahnCargoSimClient
{
    Task<int> GetCoinAmount();
    Task<Grid> GetGrid();
}
