using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Abstractions.Storage;
using CargoSim.Application.Models;
using CargoSim.Application.Services;
using CargoSim.Infrastructure.Storage;
using MassTransit;
using RabbitMQ.Client.Exceptions;
using Shared;
using static MassTransit.Logging.LogCategoryName;

namespace CargoSim.Infrastructure.Consumers;

public class OrderConsumer(IStateService stateService, IDijkstraService dijkstraService, IHahnCargoSimClient legacyClient) : IConsumer<OrderMessage>
{
    public async Task Consume(ConsumeContext<OrderMessage> context)
    {
        try
        {
            if (stateService.CurrentTransporter is null) // TODO: if no transporter,
            {
                var firstOrder = context.Message;

                await Console.Out.WriteLineAsync($"Adding order {firstOrder.Id} : from {firstOrder.OriginNodeId} -> {firstOrder.TargetNodeId}");

                OrderDb.Instance.AddOrder(firstOrder);

                var transporterId = await legacyClient.BuyTransporter(firstOrder.OriginNodeId);

                var newTransporter = await legacyClient.GetTransporter(transporterId) ?? throw new InvalidOperationException("NULL transporter");

                //transporterDb.Add(newTransporter);

                stateService.SetCurrentTransporter(newTransporter);

                await legacyClient.AcceptOrder(firstOrder.Id);

                var dijkstraResult = await dijkstraService.FindShortestPath(firstOrder, await legacyClient.GetCoinAmount());

                stateService.SetCurrentOrder(firstOrder);

                stateService.SetCurrentPath(dijkstraResult.path);

                stateService.SetCurrentPathIndex(stateService.CurrentPathIndex);

                return;
            }

            int transporterLoad = stateService.CurrentTransporter.Load;

            int availableCoins = await legacyClient.GetCoinAmount();

            var order = context.Message;

            if (transporterLoad + order.Load > stateService.CurrentTransporter.Capacity)
            {
                return;
            }

            var(shortestPath, pathCoins, stillHavingEnoughCoinsAfterAcceptingTheOrder) = await dijkstraService.FindShortestPath(order, availableCoins);

            if (shortestPath.Count == 0)
            {
                Console.WriteLine($"Rejected order {order.Id}");

                return;
            }

            if (!stillHavingEnoughCoinsAfterAcceptingTheOrder)
            {
                await Console.Out.WriteLineAsync($"stillHavingEnoughCoinsAfterAcceptingTheOrder: {stillHavingEnoughCoinsAfterAcceptingTheOrder}");

                return;
            }

            if (!OrderEnRoute(shortestPath, stateService.CurrentTransporterPath))
            {
                Console.Write($"Order {order.Id} not en route!");

                return;
            }

            await Console.Out.WriteLineAsync($"Adding order {order.Id} : from {order.OriginNodeId} -> {order.TargetNodeId}");

            OrderDb.Instance.AddOrder(order);

            await legacyClient.AcceptOrder(order.Id);
        }
        catch (OperationInterruptedException ex)
        {
            Console.WriteLine($"RabbitMQ Operation Interrupted: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception: {ex.Message}");
        }

        await Task.CompletedTask;
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
}