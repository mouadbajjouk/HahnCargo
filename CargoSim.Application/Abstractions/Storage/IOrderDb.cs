using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Storage;

public interface IOrderDb
{
    List<OrderMessage> GetOrders();
}
