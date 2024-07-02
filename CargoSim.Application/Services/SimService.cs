using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Storage;

namespace CargoSim.Application.Services;

public class SimService(IHahnCargoSimClient legacyClient, IOrderDb orderDb, DijkstraService dijkstraService)
{
    public async Task Func()
    {
        var transporterId = await legacyClient.BuyTransporter();

        if (transporterId < 0)
            throw new InvalidOperationException("Invalid transporter ID!");

        var transporter = await legacyClient.GetTransporter(transporterId) ?? throw new InvalidOperationException("NULL transporter");

        int transporterLoad = transporter.Load;

        int availableCoins = await legacyClient.GetCoinAmount();

        var orders = orderDb.GetOrders();

        foreach (var order in orders)
        {
            var orderLoad = order.Load;

            if (transporterLoad + orderLoad > transporter.Capacity)
            {
                continue;
            }

            var (shortestPath, pathCoins, enoughCoins) = await dijkstraService.FindShortestPath(order, availableCoins);

            if (!enoughCoins)
            {
                break;
            }

            if (shortestPath.Count == 0)
            {
                Console.WriteLine($"Rejected order {order.Id}");
                continue;
            }

            Console.Write($"Available coins are: {availableCoins}, transporter load is: {transporterLoad} -> Accepted order {order.Id} : {order.OriginNodeId} -> {order.TargetNodeId}, it's load is: {orderLoad}, and it's shortest path is: ");

            shortestPath.ForEach(i => Console.Write($"{i}->"));

            await Console.Out.WriteAsync($" with a total cost of: {pathCoins}");

            await Console.Out.WriteLineAsync();

            availableCoins -= pathCoins;

            transporterLoad += orderLoad;
        }
    }
}
