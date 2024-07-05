using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Services;

public interface IDijkstraService
{
    Task<(List<int> path, int pathCoins, bool stillHavingEnoughCoinsAfterAcceptingTheOrder)> FindShortestPath(OrderMessage order, int availableCoins);
}
