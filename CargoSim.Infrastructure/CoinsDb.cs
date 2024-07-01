using CargoSim.Application.Abstractions;

namespace CargoSim.Infrastructure;

public class CoinsDb : ICoinsDb
{
    public static readonly object padlock = new();
    private int _coins = 1000;

    private static CoinsDb _instance = default!;

    public static CoinsDb Instance
    {
        get
        {
            lock (padlock)
            {
                _instance ??= new CoinsDb();

                return _instance;
            }
        }
    }

    public int GetCoins() => _coins;

    public void IncrementCoins(int amountToIncrement) => _coins += amountToIncrement;

    public void DecrementCoins(int amountToDecrement)
    {
        if (_coins - amountToDecrement < 0)
            throw new InvalidOperationException("Not enough coins!");

        _coins -= amountToDecrement;
    }
}
