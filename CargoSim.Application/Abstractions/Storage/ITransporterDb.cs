using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Storage;

public interface ITransporterDb
{
    void Add(Transporter transporter);
    List<Transporter> GetAll();
    Transporter? GetById(int transporterId);
}
