using CargoSim.Application.Abstractions.Storage;
using CargoSim.Application.Models;

namespace CargoSim.Infrastructure.Storage;

public class TransporterDb : ITransporterDb
{
    public static readonly object padlock = new();

    private static TransporterDb _instance = default!;

    private List<Transporter> _transporters = [];

    public IReadOnlyList<Transporter> Transporters => _transporters;

    public static TransporterDb Instance
    {
        get
        {
            lock (padlock)
            {
                _instance ??= new TransporterDb();

                return _instance;
            }
        }
    }

    public void Add(Transporter transporter)
    {
        _transporters.Add(transporter);
    }

    public Transporter? GetById(int transporterId)
    {
        return _transporters.Find(transporter => transporter.Id == transporterId);
    }

    public List<Transporter> GetAll()
    {
        return _transporters.ToList();
    }
}
