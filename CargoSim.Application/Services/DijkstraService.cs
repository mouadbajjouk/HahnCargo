using CargoSim.Application.Abstractions.Clients;
using CargoSim.Application.Abstractions.Storage;
using CargoSim.Application.Models;

namespace CargoSim.Application.Services;

public class DijkstraService(IGridDb gridDb, ICoinsDb coinsDb)
{
    private readonly List<Node> nodes = gridDb.Nodes.ToList();
    private readonly List<Edge> edges = gridDb.Edges.ToList();
    private readonly List<Connection> connections = gridDb.Connections.ToList();
    private int _availableCoins = coinsDb.GetCoins();

    public List<int> FindShortestPath(OrderMessage order)
    {
        var originNodeId = order.OriginNodeId;
        var targetNodeId = order.TargetNodeId;
        var expirationDate = DateTime.Parse(order.ExpirationDateUtc);

        var distances = new Dictionary<int, (int Cost, TimeSpan Time)>();
        var previousNodes = new Dictionary<int, int>();
        var unvisitedNodes = new HashSet<int>(nodes.Select(n => n.Id));

        foreach (var nodeId in nodes.Select(n => n.Id))
        {
            distances[nodeId] = (int.MaxValue, TimeSpan.MaxValue);
        }
        distances[originNodeId] = (0, TimeSpan.Zero);

        while (unvisitedNodes.Count > 0)
        {
            var currentNodeId = unvisitedNodes
                .OrderBy(nodeId => distances[nodeId].Cost)
                .ThenBy(nodeId => distances[nodeId].Time)
                .First();

            if (currentNodeId == targetNodeId || distances[currentNodeId].Cost == int.MaxValue)
                break;

            unvisitedNodes.Remove(currentNodeId);

            var neighborConnections = connections
                .Where(c => c.FirstNodeId == currentNodeId || c.SecondNodeId == currentNodeId)
                .ToList();

            foreach (var connection in neighborConnections)
            {
                var neighborNodeId = connection.FirstNodeId == currentNodeId ? connection.SecondNodeId : connection.FirstNodeId;
                if (!unvisitedNodes.Contains(neighborNodeId))
                    continue;

                var edge = edges.First(e => e.Id == connection.EdgeId);
                var tentativeCost = distances[currentNodeId].Cost + edge.Cost;
                var tentativeTime = distances[currentNodeId].Time + edge.Time;
                var arrivalTime = DateTime.UtcNow + tentativeTime;

                if (tentativeCost < distances[neighborNodeId].Cost && arrivalTime <= expirationDate)
                {
                    distances[neighborNodeId] = (tentativeCost, tentativeTime);
                    previousNodes[neighborNodeId] = currentNodeId;
                }
            }
        }

        var path = new List<int>();
        var pathNodeId = targetNodeId;

        if (distances[targetNodeId].Cost > _availableCoins)
        {
            Console.WriteLine("Insufficient coins to deliver the order.");
            return path; // Return an empty path to indicate that the order cannot be delivered
        }

        while (previousNodes.ContainsKey(pathNodeId))
        {
            path.Insert(0, pathNodeId);
            pathNodeId = previousNodes[pathNodeId];
        }

        if (path.Count > 0)
        {
            path.Insert(0, originNodeId);
        }

        return path;
    }
}
