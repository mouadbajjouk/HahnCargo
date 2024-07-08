using CargoSim.Application.Abstractions.Clients;
using CargoSim.Infrastructure.DI;
using CargoSim.Infrastructure.Storage;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;

namespace CargoSim.Infrastructure.WorkerServices;

public class GridWorkerService(IHahnCargoSimClient legacyClient, GridWorkerCompletionSignal completionSignal) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AsyncRetryPolicy retryPolicy = Policy.Handle<Exception>()
                                             .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        await retryPolicy.ExecuteAsync(async () =>
        {
            var x = await legacyClient.GetGrid();

            GridDb.Instance.SetNodes(x.Nodes);

            GridDb.Instance.SetEdges(x.Edges);

            GridDb.Instance.SetConnections(x.Connections);

            completionSignal.CompletionSource.SetResult(true);
        });
    }
}
