using CargoSim.Application.Abstractions;

namespace CargoSim.Application.Services;

public class SimService(IOrderDb orderDb, DijkstraService dijkstraService)
{
    public async Task Func()
    {
        var orders = orderDb.GetOrders();

        foreach (var order in orders)
        {
            var shortestPath = dijkstraService.FindShortestPath(order);

            if (shortestPath.Count == 0)
                Console.WriteLine($"Rejected order {order.Id}");

            Console.Write($"Accepted order {order.Id} going from {order.OriginNodeId} to {order.TargetNodeId}, it's shortest path is: ");

            shortestPath.ForEach(i=>Console.Write($"{i}, "));

            Console.WriteLine();
        }
    }
}
