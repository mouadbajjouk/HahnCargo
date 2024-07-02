using CargoSim.Application.Models;

namespace CargoSim.Application.Abstractions.Storage;

public interface IGridDb
{
    IReadOnlyList<Node> Nodes { get; }
    IReadOnlyList<Edge> Edges { get; }
    IReadOnlyList<Connection> Connections { get; }
}
