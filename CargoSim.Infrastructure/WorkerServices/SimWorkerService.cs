//using CargoSim.Application.Abstractions.Clients;
//using CargoSim.Application.Abstractions.Services;
//using CargoSim.Infrastructure.DI;
//using Microsoft.Extensions.Hosting;

//namespace CargoSim.Infrastructure.WorkerServices;

//public class SimWorkerService(ISimService simService, IHahnCargoSimClient legacyClient, GridWorkerCompletionSignal _completionSignal, OrderProcessingSignal orderProcessingSignal) : BackgroundService
//{
//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        await _completionSignal.CompletionSource.Task;

//        bool firstExecution = true;

//        int transporterId = 0;

//        if (firstExecution)
//        {
//            transporterId = await legacyClient.BuyTransporter();

//            if (transporterId < 0)
//                throw new InvalidOperationException("Invalid transporter ID!");
//        }

//        var transporter = await legacyClient.GetTransporter(transporterId) ?? throw new InvalidOperationException("NULL transporter");

//        while (true)
//        {
//            await orderProcessingSignal.WaitForOrderProcessingCompletionAsync(stoppingToken);

//            await simService.Func(transporter);

//            if (firstExecution)
//                firstExecution = false;

//            await Task.Delay(1000, stoppingToken);
//        }
//    }
//}


//public class OrderProcessingSignal
//{
//    private TaskCompletionSource<bool> _orderProcessingCompletionSource = new TaskCompletionSource<bool>();

//    public async Task WaitForOrderProcessingCompletionAsync(CancellationToken cancellationToken)
//    {
//        using (cancellationToken.Register(() => _orderProcessingCompletionSource.TrySetCanceled()))
//        {
//            await _orderProcessingCompletionSource.Task;
//        }

//        // Reset the TaskCompletionSource for the next signal
//        _orderProcessingCompletionSource = new TaskCompletionSource<bool>();
//    }

//    public void SignalOrderProcessingStarted()
//    {
//        _orderProcessingCompletionSource.TrySetResult(true);
//    }
//}