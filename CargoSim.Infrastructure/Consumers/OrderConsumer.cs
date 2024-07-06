using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Models;
using CargoSim.Infrastructure.Storage;
using MassTransit;
using RabbitMQ.Client.Exceptions;
using Shared;

namespace CargoSim.Infrastructure.Consumers;

public class OrderConsumer(IStateService stateService, IDijkstraService dijkstraService, IHahnCargoSimClient legacyClient) : IConsumer<OrderMessage>
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);


    public async Task Consume(ConsumeContext<OrderMessage> context)
    {
        await _semaphore.WaitAsync();

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

                await Console.Out.WriteLineAsync($"First order ID = {firstOrder.Id}, it's path is: {string.Join("->", dijkstraResult.path)}");

                stateService.CurrentTransporterPath.AddRange(dijkstraResult.path);

                await Console.Out.WriteLineAsync($"Global CurrentTransporterPath 1: {string.Join("->", stateService.CurrentTransporterPath)}");

                stateService.SetCurrentOrder(firstOrder);

                stateService.SetCurrentPath(dijkstraResult.path);

                //stateService.SetCurrentPathIndex(stateService.CurrentPathIndex);

                return;
            }

            int transporterLoad = stateService.CurrentTransporter.Load;

            int availableCoins = await legacyClient.GetCoinAmount();

            var order = context.Message;

            if (transporterLoad + order.Load > stateService.CurrentTransporter.Capacity)
            {
                return;
            }

            var (shortestPath, pathCoins, stillHavingEnoughCoinsAfterAcceptingTheOrder) = await dijkstraService.FindShortestPath(order, availableCoins);

            if (shortestPath.Count == 0)
            {
                await Console.Out.WriteLineAsync($"Rejected order {order.Id}");

                return;
            }

            if (!stillHavingEnoughCoinsAfterAcceptingTheOrder)
            {
                await Console.Out.WriteLineAsync($"stillHavingEnoughCoinsAfterAcceptingTheOrder: {stillHavingEnoughCoinsAfterAcceptingTheOrder}");

                return;
            }

            if (!OrderEnRoute(shortestPath, stateService.CurrentTransporterPath))
            {
                await Console.Out.WriteLineAsync($"Order {order.Id} not en route!");

                return;
            }

            await Console.Out.WriteLineAsync($"Adding order {order.Id} : from {order.OriginNodeId} -> {order.TargetNodeId}");

            OrderDb.Instance.AddOrder(order);

            await legacyClient.AcceptOrder(order.Id);

            await Console.Out.WriteLineAsync($"Accepted order {order.Id}, it's path is: {string.Join("->", shortestPath)}");

            await Console.Out.WriteLineAsync($"Global CurrentTransporterPath 2: {string.Join("->", stateService.CurrentTransporterPath)}");
        }
        catch (OperationInterruptedException ex)
        {
            Console.WriteLine($"RabbitMQ Operation Interrupted: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
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