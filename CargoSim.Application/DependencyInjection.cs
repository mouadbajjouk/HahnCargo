using CargoSim.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CargoSim.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SimService, SimService>();

        services.AddScoped<DijkstraService, DijkstraService>();

        return services;
    }
}
