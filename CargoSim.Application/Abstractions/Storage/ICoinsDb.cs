using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Storage;

public interface ICoinsDb
{
    void DecrementCoins(int amountToDecrement);
    int GetCoins();
    void IncrementCoins(int amountToIncrement);
}
