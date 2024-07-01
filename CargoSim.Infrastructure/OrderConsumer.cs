using CargoSim.Application.Models;
using MassTransit;
using RabbitMQ.Client.Exceptions;

namespace CargoSim.Infrastructure;

public class OrderConsumer() : IConsumer<OrderMessage>
{
    public async Task Consume(ConsumeContext<OrderMessage> context)
    {
        try
        {
            var order = context.Message;
            
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