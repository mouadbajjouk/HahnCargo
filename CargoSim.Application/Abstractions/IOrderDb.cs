using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions;

public interface IOrderDb
{
    List<OrderMessage> GetOrders();
}
