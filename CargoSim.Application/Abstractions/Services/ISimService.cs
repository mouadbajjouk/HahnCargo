using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Services;

public interface ISimService
{
    Task Func(Transporter transporter);
    Task Stop();
    Task Start();
    Task Move(bool firstTime);
}
