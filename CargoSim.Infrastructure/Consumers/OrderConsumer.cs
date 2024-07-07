using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Models;
using CargoSim.Infrastructure.DI;
using CargoSim.Infrastructure.Storage;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Hubs;
using Shared.Utils;

namespace CargoSim.Infrastructure.Consumers;

public class OrderConsumer(IStateService stateService,
                           IDijkstraService dijkstraService,
                           IHahnCargoSimClient legacyClient,
                           IHubContext<MessageHub> hubContext,
                           GridWorkerCompletionSignal completionSignal) : IConsumer<OrderMessage>
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task Consume(ConsumeContext<OrderMessage> context)
    {
        await completionSignal.CompletionSource.Task;

        await _semaphore.WaitAsync();

        try
        {
            if (stateService.CurrentTransporter is null) // TODO: if no transporter,
            {
                await HubWriter.Write(hubContext, "receive-coins", $"Coins: {legacyClient.GetCoinAmount()}");

                var firstOrder = context.Message;

                OrderDb.Instance.AddOrder(firstOrder);

                var transporterId = await legacyClient.BuyTransporter(firstOrder.OriginNodeId);

                var newTransporter = await legacyClient.GetTransporter(transporterId) ?? throw new InvalidOperationException("NULL transporter");

                stateService.SetCurrentTransporter(newTransporter);

                await legacyClient.AcceptOrder(firstOrder.Id);

                var dijkstraResult = await dijkstraService.FindShortestPath(firstOrder, await legacyClient.GetCoinAmount());

                stateService.CurrentTransporterPath.AddRange(dijkstraResult.path);

                await HubWriter.Write(hubContext, "receive-console-message", $"First order ID = {firstOrder.Id}, it's path is: {string.Join("->", dijkstraResult.path)}");
                
                await HubWriter.Write(hubContext, "receive-console-message", $"Transporter path: {string.Join("->", stateService.CurrentTransporterPath)}");

                stateService.SetCurrentOrder(firstOrder);

                stateService.SetCurrentPath(dijkstraResult.path);

                await HubWriter.Write(hubContext, "receive-coins", $"Coins: {legacyClient.GetCoinAmount()}");

                await HubWriter.Write(hubContext, "receive-next-move-timespan", $"{stateService.NextMoveTimeSpan}");

                return;
            }

            int availableCoins = await legacyClient.GetCoinAmount();

            await HubWriter.Write(hubContext, "receive-coins", $"Coins: {availableCoins}");

            int transporterLoad = stateService.CurrentTransporter.Load;

            var order = context.Message;

            if (TransporterWillBeOverloaded(stateService, transporterLoad, order))
            {
                return;
            }

            var (shortestPath, pathCoins, stillHavingEnoughCoinsAfterAcceptingTheOrder) = await dijkstraService.FindShortestPath(order, availableCoins);

            if (NoSuitablePath(shortestPath))
            {
                return;
            }

            if (!stillHavingEnoughCoinsAfterAcceptingTheOrder)
            {
                await HubWriter.Write(hubContext, "receive-console-message", $"stillHavingEnoughCoinsAfterAcceptingTheOrder: {stillHavingEnoughCoinsAfterAcceptingTheOrder}");

                return;
            }

            if (!OrderEnRoute(shortestPath, stateService.CurrentTransporterPath))
            {
                return;
            }

            OrderDb.Instance.AddOrder(order);

            await legacyClient.AcceptOrder(order.Id);

            await WriteConsumedOrders(stateService, hubContext, availableCoins, order, shortestPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static bool NoSuitablePath(List<int> shortestPath)
    {
        return shortestPath.Count == 0;
    }

    private static bool TransporterWillBeOverloaded(IStateService stateService, int transporterLoad, OrderMessage order)
    {
        return transporterLoad + order.Load > stateService.CurrentTransporter.Capacity;
    }

    private static async Task WriteConsumedOrders(IStateService stateService, IHubContext<MessageHub> hubContext, int availableCoins, OrderMessage order, List<int> shortestPath)
    {
        await HubWriter.Write(hubContext, "receive-console-message", $"Adding order {order.Id} : from {order.OriginNodeId} -> {order.TargetNodeId}");

        await HubWriter.Write(hubContext, "receive-console-message", $"Accepted order {order.Id}, it's path is: {string.Join("->", shortestPath)}");

        await HubWriter.Write(hubContext, "receive-console-message", $"Transporter path: {string.Join("->", stateService.CurrentTransporterPath)}");

        await HubWriter.Write(hubContext, "receive-coins", $"Coins: {availableCoins}");
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
            transporterPath.AddRange(orderPath.GetRange(1, orderPath.Count - 1));

            return true;
        }

        if (transporterPath.ContainsSublist(orderPath))
        {
            return true;
        }

        return false;
    }
}