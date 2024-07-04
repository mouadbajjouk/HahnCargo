using CargoSim.Application.Abstractions.Clients;
using CargoSim.Infrastructure.DI;
using CargoSim.Infrastructure.Storage;
using Microsoft.Extensions.Hosting;

namespace CargoSim.Infrastructure.WorkerServices;

public class GridWorkerService(IHahnCargoSimClient legacyClient, GridWorkerCompletionSignal _completionSignal) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var x = await legacyClient.GetGrid();

        GridDb.Instance.SetNodes(x.Nodes);

        GridDb.Instance.SetEdges(x.Edges);

        GridDb.Instance.SetConnections(x.Connections);

        _completionSignal.CompletionSource.SetResult(true);
    }
}
