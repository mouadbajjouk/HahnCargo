namespace CargoSim.Application.Models;

public record Transporter(int Id, string Owner, int PositionNodeId, bool InTransit, int Capacity, int Load, List<OrderMessage> LoadedList);
