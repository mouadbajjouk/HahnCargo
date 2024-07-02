using CargoSim.Application.Abstractions.Storage;

namespace CargoSim.Application.Services;

public class SimService(IOrderDb orderDb, DijkstraService dijkstraService)
{
    public async Task Func()
    {
        var orders = orderDb.GetOrders();

        foreach (var order in orders)
        {
            var (shortestPath, enoughCoins) = await dijkstraService.FindShortestPath(order);

            if (!enoughCoins)
            {
                break;
            }

            if (shortestPath.Count == 0)
            {
                Console.WriteLine($"Rejected order {order.Id}");
                continue;
            }

            Console.Write($"Accepted order {order.Id} : {order.OriginNodeId} -> {order.TargetNodeId}, it's shortest path is: ");

            shortestPath.ForEach(i => Console.Write($"{i}->"));

            Console.WriteLine();
        }
    }
}
