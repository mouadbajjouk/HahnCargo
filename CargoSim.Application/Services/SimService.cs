using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Abstractions.Storage;
using CargoSim.Application.Models;
using Shared;

namespace CargoSim.Application.Services;

public class SimService(IHahnCargoSimClient legacyClient,
                        IOrderDb orderDb,
                        ITransporterDb transporterDb,
                        IStateService stateService,
                        IDijkstraService dijkstraService) : ISimService
{
    public async Task<Transporter?> GetCargo()
    {
        return await legacyClient.GetTransporter(stateService.CurrentTransporter.Id);
    }

    public async Task CreateOrders()
    {
        await legacyClient.CreateOrders();
    }

    public async Task Func(Transporter transporter)
    {
        //int transporterId = 0;

        //if (firstExecution)
        //{
        //    transporterId = await legacyClient.BuyTransporter();

        //    if (transporterId < 0)
        //        throw new InvalidOperationException("Invalid transporter ID!");
        //}

        //var transporter = await legacyClient.GetTransporter(transporterId) ?? throw new InvalidOperationException("NULL transporter");

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

            await legacyClient.AcceptOrder(order.Id);

            availableCoins -= pathCoins;

            transporterLoad += orderLoad;
        }
    }

    public async Task<List<int>> GetPath(OrderMessage order)
    {
        var (shortestPath, n, g) = await dijkstraService.FindShortestPath(order, 1000);

        return shortestPath;
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

    public async Task Start() => await legacyClient.StartSimulation();

    public async Task Stop() => await legacyClient.StopSimulation();

    public async Task Move(bool firstTime)
    {
        stateService.CurrentTransporter = await GetCargo()!; // TODO: null

        var orders = orderDb.GetOrders();

        if (firstTime)
        {
            //var firstOrderPath = await GetPath(orders[0]);

            //stateService.SetCurrentOrder(orders[0]);

            //stateService.SetCurrentPath(firstOrderPath);

            //stateService.SetCurrentPathIndex(stateService.CurrentPathIndex);

            //int firstTransporterPositionNodeId = firstOrderPath[stateService.CurrentPathIndex];

            //var transporterId = await legacyClient.BuyTransporter(firstTransporterPositionNodeId);

            //var newTransporter = await legacyClient.GetTransporter(transporterId) ?? throw new InvalidOperationException("NULL transporter");

            //transporterDb.Add(newTransporter);

            //stateService.SetCurrentTransporter(newTransporter);

            //await legacyClient.AcceptOrder(orders[0].Id); 

            //await legacyClient.Move(stateService.CurrentTransporter.Id, stateService.CurrentTransporterPath[stateService.CurrentPathIndex + 1]);
            await legacyClient.Move(stateService.CurrentTransporter.Id, stateService.CurrentTransporterPath[1]);

            stateService.CurrentTransporterPath.RemoveAt(0);

            return;

            //stateService.SetCurrentPathIndex(stateService.CurrentPathIndex);
        }

        if (stateService.CurrentOrder is null || stateService.CurrentTransporter.InTransit)
        {
            return;
        }

        if (1 < stateService.CurrentTransporterPath.Count)
        {
            if (stateService.CurrentOrder.TargetNodeId == stateService.CurrentTransporterPath[stateService.CurrentPathIndex])
            {
                // order arrived
                orderDb.Delete(orderDb.GetOrders().Find(order => order.Id == stateService.CurrentOrder.Id));

                stateService.SetCurrentOrder(orderDb.GetOrders().First()); // TODO: like a queue, maybe implement a queue instead of list !!!
            }

            await legacyClient.Move(stateService.CurrentTransporter.Id, stateService.CurrentTransporterPath[stateService.CurrentPathIndex + 1]);

            stateService.CurrentTransporterPath.RemoveAt(stateService.CurrentPathIndex);

            //stateService.SetCurrentPathIndex(stateService.CurrentPathIndex + 1);

            //var updatedTransporter = await legacyClient.GetTransporter(stateService.CurrentTransporter.Id) ?? throw new InvalidOperationException("NULL transporter");

            //stateService.SetCurrentTransporter(updatedTransporter);
        }
        else
        {
            // order arrived
            // ++ coins

            orderDb.Delete(orderDb.GetOrders().Find(order => order.Id == stateService.CurrentOrder.Id));

            stateService.SetCurrentOrder(orderDb.GetOrders().First()); // TODO: like a queue, maybe implement a queue instead of list !!!

            //stateService.SetCurrentPath(dijkstraResult.path); // transporter path has the global path

            //stateService.SetCurrentPathIndex(stateService.CurrentPathIndex);
        }


        //stateService.SetCurrentPathIndex(stateService.CurrentPathIndex + 1);
    }
}
