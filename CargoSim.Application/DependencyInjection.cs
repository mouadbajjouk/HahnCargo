using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CargoSim.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IStateService, StateService>();

        services.AddSingleton<ISimService, SimService>();

        services.AddSingleton<IDijkstraService, DijkstraService>();

        return services;
    }
}
