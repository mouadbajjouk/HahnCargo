using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Services;

public interface ISimService
{
    Task Stop();
    Task Start();
    Task Move(bool firstTime);
    Task CreateOrders();
    Task<Transporter?> GetCargo();
    Graph GetGraph();
}
