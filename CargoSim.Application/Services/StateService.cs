using CargoSim.Application.Abstractions.Services;
using CargoSim.Application.Models;

namespace CargoSim.Application.Services;

public class StateService : IStateService // TODO: set setters to private
{
    public string JwtToken { get; set; }

    public bool IsFirstTime { get; set; } = true;

    public OrderMessage CurrentOrder { get; set; } = default!;

    public List<int> CurrentOrderPath { get; set; } = [];

    public int CurrentPathIndex { get; set; } = 0;

    public Transporter CurrentTransporter { get; set; } = default!;

    public List<int> CurrentTransporterPath { get; set; } = [];

    public void SetCurrentOrder(OrderMessage currentOrder) => CurrentOrder = currentOrder;

    public void SetCurrentPath(List<int> currentPath) => CurrentOrderPath = currentPath;

    public void SetCurrentPathIndex(int currentPathIndex) => CurrentPathIndex = currentPathIndex;

    public void SetCurrentTransporter(Transporter currentTransporter) => CurrentTransporter = currentTransporter;

    public void SetCurrentTransporterPath(List<int> currentTransporterPath) => CurrentTransporterPath = currentTransporterPath;
}
