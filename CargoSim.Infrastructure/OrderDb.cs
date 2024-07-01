using CargoSim.Application.Abstractions;
using CargoSim.Application.Models;

namespace CargoSim.Infrastructure;

public class OrderDb : IOrderDb
{
    public static readonly object padlock = new();

    private static OrderDb _instance = default!;

    private List<OrderMessage> _orders = [];

    public IReadOnlyList<OrderMessage> Orders => _orders;

    public static OrderDb Instance
    {
        get
        {
            lock (padlock)
            {
                _instance ??= new OrderDb();

                return _instance;
            }
        }
    }

    public void AddOrder(OrderMessage order)
    {
        _orders.Add(order);
    }

    public void AddOrders(List<OrderMessage> orders)
    {
        _orders.AddRange(orders);
    }

    public List<OrderMessage> GetOrders()
    {
        return _orders.ToList();
    }
}
