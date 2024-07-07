using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Storage;
using CargoSim.Infrastructure.Clients;
using CargoSim.Infrastructure.Consumers;
using CargoSim.Infrastructure.Services;
using CargoSim.Infrastructure.Storage;
using CargoSim.Infrastructure.WorkerServices;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace CargoSim.Infrastructure.DI;

public class GridWorkerCompletionSignal // TODO: send to new file
{
    public TaskCompletionSource<bool> CompletionSource { get; } = new TaskCompletionSource<bool>();
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, RabbitMqSettings rabbitMqSettings)
    {
        AddStorage(services);

        AddHttpClients(services);

        AddMessaging(services, rabbitMqSettings);

        services.AddSingleton<GridWorkerCompletionSignal>();

        services.AddHostedService<GridWorkerService>();

        return services;
    }

    private static void AddStorage(IServiceCollection services)
    {
        services.AddSingleton<IOrderDb>(OrderDb.Instance);

        services.AddSingleton<IGridDb>(GridDb.Instance);

        services.AddSingleton<ICoinsDb>(CoinsDb.Instance);

        services.AddSingleton<ITransporterDb>(TransporterDb.Instance);
    }

    private static void AddMessaging(IServiceCollection services, RabbitMqSettings rabbitMqSettings)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

            configure.AddConsumer<OrderConsumer>();

            configure.UsingRabbitMq((context, config) =>
            {
                config.Host(new Uri("amqp://guest:guest@cargosim-queue:5672"), host => // TODO
                {
                    host.Username(rabbitMqSettings.Username);
                    host.Password(rabbitMqSettings.Password);
                });


                config.ReceiveEndpoint("HahnCargoSim_NewOrders", ep =>
                {
                    // by default, MassTransit sets durable to true, since the legacy sets durable to false, we set it false here as well
                    // see: https://masstransit.io/documentation/configuration/transports/rabbitmq#endpoint-configuration in Endpoint Configuration section
                    ep.Durable = false;

                    ep.ClearSerialization();
                    ep.UseRawJsonSerializer();

                    ep.ConfigureConsumer<OrderConsumer>(context);
                });
            });
        });
    }

    private static void AddHttpClients(IServiceCollection services)
    {
        services.AddHttpClient<IHahnCargoSimClient, HahnCargoSimClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://host.docker.internal:7115/"); // TODO : appsettings
        }).ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

        services.AddHttpClient<JwtService>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://host.docker.internal:7115/"); // TODO : appsettings
        }).ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });
    }
}
