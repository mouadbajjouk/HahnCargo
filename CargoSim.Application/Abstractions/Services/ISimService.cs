
namespace CargoSim.Application.Abstractions.Services;

public interface ISimService
{
    Task Func(bool firstExecution);
}
