using Microsoft.Extensions.Hosting;

namespace CargoSim.Infrastructure;

public class GridWorkerService(HahnCargoSimClient legacyClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var x = await legacyClient.GetGrid();

        GridDb.Instance.SetNodes(x.Nodes);

        GridDb.Instance.SetEdges(x.Edges);

        GridDb.Instance.SetConnections(x.Connections);

        
    }
}
