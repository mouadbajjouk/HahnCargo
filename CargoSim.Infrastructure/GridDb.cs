using CargoSim.Application.Abstractions;
using CargoSim.Application.Models;

namespace CargoSim.Infrastructure;

public class GridDb : IGridDb
{
    public static readonly object padlock = new();

    private static GridDb _instance = default!;

    private List<Node> _nodes = [];
    private List<Edge> _edges = [];
    private List<Connection> _connections = [];

    public IReadOnlyList<Node> Nodes => _nodes;
    public IReadOnlyList<Edge> Edges => _edges;
    public IReadOnlyList<Connection> Connections => _connections;

    public static GridDb Instance
    {
        get
        {
            lock (padlock)
            {
                _instance ??= new GridDb();

                return _instance;
            }
        }
    }

    public void SetNodes(List<Node> nodes) => _nodes = nodes;

    public void SetEdges(List<Edge> edges) => _edges = edges;

    public void SetConnections(List<Connection> connections) => _connections = connections;
}
