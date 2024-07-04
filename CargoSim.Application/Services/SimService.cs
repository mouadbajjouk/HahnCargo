using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Abstractions.Storage;
using CargoSim.Application.Extensions;
using CargoSim.Application.Models;

namespace CargoSim.Application.Services;

public class SimService(IHahnCargoSimClient legacyClient, IOrderDb orderDb, DijkstraService dijkstraService) : ISimService
{
    public async Task Func(bool firstExecution)
    {
        int transporterId = 0;

        if (firstExecution)
        {
            transporterId = await legacyClient.BuyTransporter();

            if (transporterId < 0)
                throw new InvalidOperationException("Invalid transporter ID!");
        }

        var transporter = await legacyClient.GetTransporter(transporterId) ?? throw new InvalidOperationException("NULL transporter");

        int transporterLoad = transporter.Load;

        int availableCoins = await legacyClient.GetCoinAmount();

        var orders = orderDb.GetOrders();

        List<int> transporterPath = [];

        foreach (var order in orders)
        {
            await Console.Out.WriteLineAsync($"Processing order: {order.Id}");


            var orderLoad = order.Load;

            if (transporterLoad + orderLoad > transporter.Capacity)
            {
                continue;
            }

            var (shortestPath, pathCoins, stillHavingEnoughCoinsAfterAcceptingTheOrder) = await dijkstraService.FindShortestPath(order, availableCoins);

            if (shortestPath.Count == 0)
            {
                Console.WriteLine($"Rejected order {order.Id}");
                continue;
            }

            if (!stillHavingEnoughCoinsAfterAcceptingTheOrder)
            {
                await Console.Out.WriteLineAsync($"stillHavingEnoughCoinsAfterAcceptingTheOrder: {stillHavingEnoughCoinsAfterAcceptingTheOrder}");
                break;
            }

            if (!OrderEnRoute(shortestPath, transporterPath))
            {
                Console.Write($"Order {order.Id} not en route!");

                continue;
            }

            transporterPath.AddRange(shortestPath);

            await WriteOrderInfo(transporterLoad, availableCoins, order, orderLoad, shortestPath, pathCoins);

            availableCoins -= pathCoins;

            transporterLoad += orderLoad;
        }
    }

    private static bool OrderEnRoute(List<int> orderPath, List<int> transporterPath)
    {
        // Count = 0 means that the transporter has just started, so return true to process the first order.
        if (transporterPath.Count == 0)
        {
            return true;
        }

        // E.g. TransporterPath = [A,B,C,D,E]
        // Ex1: Order 2: B->C->D => [B,C,D] : ACCEPT IT (it's en route)
        // Ex2: Order 2: E->D->C => [E,D,C] : (because it's origin node = currentOrder.targetNode) ----> transporterPath = [A,B,C,D,E,D,C]
        if (transporterPath[^1] == orderPath[0])
        {
            transporterPath.AddRange(orderPath);

            return true;
        }

        if (transporterPath.ContainsSublist(orderPath))
        {
            return true;
        }

        return false;
    }

    private static async Task WriteOrderInfo(int transporterLoad, int availableCoins, OrderMessage order, int orderLoad, List<int> shortestPath, int pathCoins)
    {
        Console.Write($"Available coins are: {availableCoins}, transporter load is: {transporterLoad} -> Accepted order {order.Id} : {order.OriginNodeId} -> {order.TargetNodeId}, it's load is: {orderLoad}, and it's shortest path is: ");

        shortestPath.ForEach(i => Console.Write($"{i}->"));

        await Console.Out.WriteAsync($" with a total cost of: {pathCoins}");

        await Console.Out.WriteLineAsync();
    }
}
