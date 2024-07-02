using CargoSim.Application.Models;
using CargoSim.Infrastructure.Storage;
using MassTransit;
using RabbitMQ.Client.Exceptions;

namespace CargoSim.Infrastructure.Consumers;

public class OrderConsumer() : IConsumer<OrderMessage>
{
    public async Task Consume(ConsumeContext<OrderMessage> context)
    {
        try
        {
            var order = context.Message;

            await Console.Out.WriteLineAsync($"Adding order {order.Id} : from {order.OriginNodeId} -> {order.TargetNodeId}");

            OrderDb.Instance.AddOrder(order);
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
}