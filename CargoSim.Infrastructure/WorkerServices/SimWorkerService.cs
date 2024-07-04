using CargoSim.Application.Abstractions.Services;
using CargoSim.Infrastructure.DI;
using Microsoft.Extensions.Hosting;

namespace CargoSim.Infrastructure.WorkerServices;

public class SimWorkerService(ISimService simService, GridWorkerCompletionSignal _completionSignal) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _completionSignal.CompletionSource.Task;

        bool firstExecution = true;

        while (true)
        {
            await simService.Func(firstExecution);

            firstExecution = false;

            await Task.Delay(1000, stoppingToken);
        }
    }
}
