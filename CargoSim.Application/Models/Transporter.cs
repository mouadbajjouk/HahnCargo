namespace CargoSim.Application.Models;

public record Transporter(int Id, string Owner, Node Position, bool InTransit, int Capacity, int Load, List<OrderMessage> LoadedList);
