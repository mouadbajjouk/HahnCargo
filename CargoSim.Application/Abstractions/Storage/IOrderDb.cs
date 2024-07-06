using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Storage;

public interface IOrderDb
{
    void Delete(OrderMessage order);
    List<OrderMessage> GetOrders();
}
