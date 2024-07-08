using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Abstractions.Storage;
using CargoSim.Application.Models;
using Microsoft.AspNetCore.SignalR;
using Shared.Hubs;
using Shared.Utils;

namespace CargoSim.Application.Services;

public class SimService(IHahnCargoSimClient legacyClient,
                        IOrderDb orderDb,
                        IGridDb gridDb,
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
        stateService.CurrentTransporter = await GetCargo(); // TODO: null

        await HubWriter.Write(hubContext, "receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");

        if (firstTime)
        {
            await HandleTransporterMove(legacyClient, stateService, hubContext);

            return;
        }

        if (stateService.CurrentOrder is null || stateService.CurrentTransporter.InTransit)
        {
            await HubWriter.Write(hubContext, "receive-console-message", $"Can't move transporter {stateService.CurrentTransporter.Id}, it's in transit!");

            await HubWriter.Write(hubContext, "receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");

            return;
        }

        if (stateService.CurrentTransporterPath.Count > 1)
        {
            if (stateService.CurrentOrder.TargetNodeId == stateService.CurrentTransporterPath[stateService.CurrentPathIndex])
            {
                // order arrived
                await HandleOrderArrived(legacyClient, orderDb, stateService, hubContext);

                await HubWriter.Write(hubContext, "receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");

                await HandleTransporterMove(legacyClient, stateService, hubContext);
            }
            else
            {
                await HandleTransporterMove(legacyClient, stateService, hubContext);
            }

        }
        else
        {
            // order arrived

            await HandleOrderArrived(legacyClient, orderDb, stateService, hubContext);
        }
    }

    private async Task HandleTransporterMove(IHahnCargoSimClient legacyClient, IStateService stateService, IHubContext<MessageHub> hubContext)
    {
        await legacyClient.Move(stateService.CurrentTransporter.Id, stateService.CurrentTransporterPath[1]);

        await HubWriter.Write(hubContext, "receive-console-message", $"Moving transporter {stateService.CurrentTransporter.Id} from {stateService.CurrentTransporterPath[0]}->{stateService.CurrentTransporterPath[1]}");

        await HubWriter.Write(hubContext, "receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");

        stateService.NextMoveTimeSpan = GetConnectionTimeSpan(stateService.CurrentTransporterPath[0], stateService.CurrentTransporterPath[1]);

        await HubWriter.Write(hubContext, "receive-next-move-timespan", $"{stateService.NextMoveTimeSpan}");

        stateService.CurrentTransporterPath.RemoveAt(stateService.CurrentPathIndex);
    }

    private static async Task HandleOrderArrived(IHahnCargoSimClient legacyClient, IOrderDb orderDb, IStateService stateService, IHubContext<MessageHub> hubContext)
    {
        orderDb.Delete(orderDb.GetOrders().Find(order => order.Id == stateService.CurrentOrder.Id));

        stateService.SetCurrentOrder(orderDb.GetOrders().First()); // TODO: like a queue, maybe implement a queue instead of list !!!

        await HubWriter.Write(hubContext, "receive-coins", $"Coins: {await legacyClient.GetCoinAmount()}");
    }

    private TimeSpan GetConnectionTimeSpan(int originNodeId, int targetNodeId)
    {
        var connection = gridDb.Connections.Single(connection => connection.FirstNodeId == originNodeId
                                                              && connection.SecondNodeId == targetNodeId);

        return gridDb.Edges.Single(edge => edge.Id == connection.EdgeId).Time;
    }
}
