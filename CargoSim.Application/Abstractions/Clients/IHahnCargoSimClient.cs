using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Clients;

public interface IHahnCargoSimClient
{
    Task<int> BuyTransporter();
    Task<int> GetCoinAmount();
    Task<Grid> GetGrid();
    Task<Transporter?> GetTransporter(int id);
}
