namespace CargoSim.Application.Models;

public record Graph(List<GraphNode> Nodes, List<GraphLink> Links);

public record GraphNode(int Id, string Label);

public record GraphLink(int Id, string Source, string Target, string Label);