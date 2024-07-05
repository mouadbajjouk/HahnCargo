using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Clients;

public interface IHahnCargoSimClient
{
    Task AcceptOrder(int orderId);
    Task<int> BuyTransporter(int atPosition);
    Task<int> GetCoinAmount();
    Task<Grid> GetGrid();
    Task<Transporter?> GetTransporter(int id);
    Task Move(int transporterId, int targetNodeId);
    Task StartSimulation();
    Task StopSimulation();
}
