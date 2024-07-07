using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Abstractions.Storage;
using CargoSim.Application.Models;
using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Hubs;

namespace CargoSim.Application.Services;

public class SimService(IHahnCargoSimClient legacyClient,
                        IOrderDb orderDb,
                        IGridDb gridDb,
                        ITransporterDb transporterDb,
                        IStateService stateService,
                        IHubContext<MessageHub> hubContext,
                        IDijkstraService dijkstraService) : ISimService
{
    public async Task Start() => await legacyClient.StartSimulation();

    public async Task Stop() => await legacyClient.StopSimulation();
    
    public Graph GetGraph()
    {
        var graphNodes = gridDb.Nodes.ToList().ConvertAll(node => new GraphNode(node.Id, node.Name));

        var graphLinks = gridDb.Connections.ToList().ConvertAll(connection => new GraphLink(connection.Id,
                                                                                            connection.FirstNodeId.ToString(),
                                                                                            connection.SecondNodeId.ToString(),
                                                                                            connection.Id.ToString()));

        return new Graph(graphNodes, graphLinks);
    }

    public async Task<Transporter?> GetCargo()
    {
        return await legacyClient.GetTransporter(stateService.CurrentTransporter.Id);
    }

    public async Task CreateOrders()
    {
        await legacyClient.CreateOrders();
    }
 
    public async Task Move(bool firstTime)
    {
        stateService.CurrentTransporter = await GetCargo()!; // TODO: null
        await hubContext.Clients.All.SendAsync("receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");

        var orders = orderDb.GetOrders();

        if (firstTime)
        {
            await legacyClient.Move(stateService.CurrentTransporter.Id, stateService.CurrentTransporterPath[1]);

            await hubContext.Clients.All.SendAsync("receive-console-message", $"Moving transporter {stateService.CurrentTransporter.Id} from {stateService.CurrentTransporterPath[0]}->{stateService.CurrentTransporterPath[1]}");
            await hubContext.Clients.All.SendAsync("receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");

            stateService.NextMoveTimeSpan = GetConnectionTimeSpan(stateService.CurrentTransporterPath[0], stateService.CurrentTransporterPath[1]);

            await hubContext.Clients.All.SendAsync("receive-next-move-timespan", $"{stateService.NextMoveTimeSpan}");

            stateService.CurrentTransporterPath.RemoveAt(0);

            return;
        }

        if (stateService.CurrentOrder is null || stateService.CurrentTransporter.InTransit)
        {
            await hubContext.Clients.All.SendAsync("receive-console-message", $"Can't move transporter {stateService.CurrentTransporter.Id}, it's in transit!");
            await hubContext.Clients.All.SendAsync("receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");


            return;
        }

        if (stateService.CurrentTransporterPath.Count > 1)
        {
            if (stateService.CurrentOrder.TargetNodeId == stateService.CurrentTransporterPath[stateService.CurrentPathIndex])
            {
                // order arrived
                orderDb.Delete(orderDb.GetOrders().Find(order => order.Id == stateService.CurrentOrder.Id));

                stateService.SetCurrentOrder(orderDb.GetOrders().First()); // TODO: like a queue, maybe implement a queue instead of list !!!

                await hubContext.Clients.All.SendAsync("receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");
            }

            await legacyClient.Move(stateService.CurrentTransporter.Id, stateService.CurrentTransporterPath[stateService.CurrentPathIndex + 1]);

            await hubContext.Clients.All.SendAsync("receive-console-message", $"Moving transporter {stateService.CurrentTransporter.Id} from {stateService.CurrentTransporterPath[0]}->{stateService.CurrentTransporterPath[1]}");
            await hubContext.Clients.All.SendAsync("receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");

            stateService.NextMoveTimeSpan = GetConnectionTimeSpan(stateService.CurrentTransporterPath[0], stateService.CurrentTransporterPath[1]);

            await hubContext.Clients.All.SendAsync("receive-next-move-timespan", $"{stateService.NextMoveTimeSpan}");

            stateService.CurrentTransporterPath.RemoveAt(stateService.CurrentPathIndex);
        }
        else
        {
            // order arrived
            // ++ coins

            orderDb.Delete(orderDb.GetOrders().Find(order => order.Id == stateService.CurrentOrder.Id));

            stateService.SetCurrentOrder(orderDb.GetOrders().First()); // TODO: like a queue, maybe implement a queue instead of list !!!
            await hubContext.Clients.All.SendAsync("receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");
        }
    }

    private TimeSpan GetConnectionTimeSpan(int originNodeId, int targetNodeId)
    {
        var connection = gridDb.Connections.Single(connection => connection.FirstNodeId == originNodeId
                                                              && connection.SecondNodeId == targetNodeId);

        return gridDb.Edges.Single(edge => edge.Id == connection.EdgeId).Time;
    }
}
