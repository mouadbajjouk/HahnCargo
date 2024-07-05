using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Services;

public interface IStateService
{
    bool IsFirstTime { get; set; }
    OrderMessage CurrentOrder { get; set; }
    List<int> CurrentOrderPath { get; set; }
    int CurrentPathIndex { get; set; }
    Transporter CurrentTransporter { get; set; }
    List<int> CurrentTransporterPath { get; set; }

    void SetCurrentOrder(OrderMessage currentOrder);
    void SetCurrentPath(List<int> currentPath);
    void SetCurrentPathIndex(int currentPathIndex);
    void SetCurrentTransporter(Transporter currentTransporter);
    void SetCurrentTransporterPath(List<int> currentTransporterPath);
}
