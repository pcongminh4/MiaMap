using Application.Abstractions.Data;
using Application.Places.FindRoute;
using Domain.Places;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Places;

public sealed class RoutingRepository(ApplicationDbContext dbContext) : IRoutingRepository
{
	public async Task<FindRouteResult> FindRouteAsync(
		double startLatitude,
		double startLongitude,
		double endLatitude,
		double endLongitude,
		CancellationToken cancellationToken = default)
	{
		// Calculate bounding box
		var padding = 0.05;

		var minLat = Math.Min(startLatitude, endLatitude) - padding;
		var maxLat = Math.Max(startLatitude, endLatitude) + padding;
		var minLng = Math.Min(startLongitude, endLongitude) - padding;
		var maxLng = Math.Max(startLongitude, endLongitude) + padding;

		// Create spatial bounding box
		var envelope = new Envelope(minLng, maxLng, minLat, maxLat);

		var boundingBox = new GeometryFactory(new PrecisionModel(), 4326)
			.ToGeometry(envelope);

		// Query nodes in bounding box
		var nodesInBox = await dbContext.Nodes
			.AsNoTracking()
			.Where(n => n.IsActive)
			.Where(n => n.Location.Intersects(boundingBox))
			.ToListAsync(cancellationToken)
			.ConfigureAwait(false);

		if (nodesInBox.Count == 0)
		{
			return new FindRouteResult(false, [], 0);
		}

		// Query roads in bounding box
		var roadsInBox = await dbContext.Roads
			.AsNoTracking()
			.Where(r => r.IsActive)
			.Where(r => r.Geometry.Intersects(boundingBox))
			.ToListAsync(cancellationToken)
			.ConfigureAwait(false);

		if (roadsInBox.Count == 0)
		{
			return new FindRouteResult(false, [], 0);
		}

		// Find nearest nodes
		var startPoint = new Point(startLongitude, startLatitude)
		{
			SRID = 4326
		};

		var endPoint = new Point(endLongitude, endLatitude)
		{
			SRID = 4326
		};

		var graph = BuildGraph(nodesInBox, roadsInBox);

		var routableNodes = nodesInBox
		.Where(n =>
			graph.ContainsKey(n.Id) &&
			graph[n.Id].Count > 1)
		.ToList();

		var startNode = routableNodes
	.OrderBy(n => n.Location.Distance(startPoint))
	.First();

var endNode = routableNodes
	.OrderBy(n => n.Location.Distance(endPoint))
	.First();	

		Console.WriteLine("=== START NODE ROADS ===");

		var startRoads = roadsInBox
			.Where(r =>
				r.StartNodeId == startNode.Id ||
				r.EndNodeId == startNode.Id)
			.ToList();

		foreach (var road in startRoads)
		{
			Console.WriteLine(
				$"Road {road.Id}: " +
				$"{road.StartNodeId} -> {road.EndNodeId}, " +
				$"OneWay={road.IsOneWay}, " +
				$"Weight={road.Weight}");
		}

		Console.WriteLine("=== END NODE ROADS ===");

		var endRoads = roadsInBox
			.Where(r =>
				r.StartNodeId == endNode.Id ||
				r.EndNodeId == endNode.Id)
			.ToList();

		foreach (var road in endRoads)
		{
			Console.WriteLine(
				$"Road {road.Id}: " +
				$"{road.StartNodeId} -> {road.EndNodeId}, " +
				$"OneWay={road.IsOneWay}, " +
				$"Weight={road.Weight}");
		}
		Console.WriteLine($"StartNode: {startNode.Id}");
		Console.WriteLine($"EndNode: {endNode.Id}");

		Console.WriteLine($"StartNode Coord: {startNode.Location.Y}, {startNode.Location.X}");
		Console.WriteLine($"EndNode Coord: {endNode.Location.Y}, {endNode.Location.X}");

		Console.WriteLine($"Distance to start: {startNode.Location.Distance(startPoint)}");
		Console.WriteLine($"Distance to end: {endNode.Location.Distance(endPoint)}");

		// Build graph
		

		Console.WriteLine($"Graph Nodes Count: {graph.Count}");

		foreach (var kv in graph.Take(10))
		{
			Console.WriteLine($"Node {kv.Key} has {kv.Value.Count} neighbors");

			foreach (var neighbor in kv.Value.Take(5))
			{
				Console.WriteLine(
					$"  -> {neighbor.TargetNodeId}, weight={neighbor.Weight}");
			}
		}

		// Run Dijkstra
		var (pathNodes, totalDistance) = Dijkstra(
			graph,
			startNode.Id,
			endNode.Id);

		if (pathNodes.Count == 0)
		{
			return new FindRouteResult(false, [], 0);
		}

		// Build path points
		var pathPoints = new List<GeoPoint>
		{
			new(startLatitude, startLongitude)
		};

		// Add intermediate geometry points
		for (int i = 0; i < pathNodes.Count - 1; i++)
		{
			var fromNodeId = pathNodes[i];
			var toNodeId = pathNodes[i + 1];

			var road = roadsInBox.FirstOrDefault(r =>
				(r.StartNodeId == fromNodeId &&
				 r.EndNodeId == toNodeId) ||

				(r.StartNodeId == toNodeId &&
				 r.EndNodeId == fromNodeId &&
				 !r.IsOneWay));

			if (road?.Geometry != null)
			{
				var coords = road.Geometry.Coordinates;

				for (int j = 1; j < coords.Length - 1; j++)
				{
					pathPoints.Add(
						new GeoPoint(coords[j].Y, coords[j].X));
				}
			}
		}

		pathPoints.Add(new(endLatitude, endLongitude));

		return new FindRouteResult(
			true,
			pathPoints,
			totalDistance);
	}

	private static Dictionary<int, List<(int TargetNodeId, double Weight, bool IsOneWay)>> BuildGraph(
		IReadOnlyList<Node> nodes,
		IReadOnlyList<Road> roads)
	{
		var graph = nodes.ToDictionary(n => n.Id, _ => new List<(int, double, bool)>());

		foreach (var road in roads)
		{
			// Add edge from start to end
			if (graph.ContainsKey(road.StartNodeId))
			{
				graph[road.StartNodeId].Add((road.EndNodeId, road.Weight, road.IsOneWay));
			}

			// Add edge from end to start (unless one-way)
			if (!road.IsOneWay && graph.ContainsKey(road.EndNodeId))
			{
				graph[road.EndNodeId].Add((road.StartNodeId, road.Weight, false));
			}
		}

		return graph;
	}

	private static (IReadOnlyList<int> PathNodes, double TotalDistance) Dijkstra(
	Dictionary<int, List<(int TargetNodeId, double Weight, bool IsOneWay)>> graph,
	int startNodeId,
	int endNodeId)
	{
		var distances = new Dictionary<int, double>();
		var previous = new Dictionary<int, (int NodeId, double Weight)>();

		var unvisited = new SortedSet<(double Distance, int NodeId)>(
			Comparer<(double, int)>.Create((a, b) =>
			{
				int cmp = a.Item1.CompareTo(b.Item1);
				return cmp != 0 ? cmp : a.Item2.CompareTo(b.Item2);
			}));

		foreach (var nodeId in graph.Keys)
		{
			distances[nodeId] = double.PositiveInfinity;
		}

		distances[startNodeId] = 0;
		unvisited.Add((0, startNodeId));

		while (unvisited.Count > 0)
		{

			var (currentDistance, currentNodeId) = unvisited.Min;
			unvisited.Remove(unvisited.Min);
			Console.WriteLine(
		$"Visiting Node {currentNodeId}, distance={currentDistance}");
			if (currentNodeId == endNodeId)
			{
				break;
			}

			if (currentDistance > distances[currentNodeId])
			{
				continue;
			}

			if (!graph.TryGetValue(currentNodeId, out var neighbors))
			{
				continue;
			}



			foreach (var (targetNodeId, weight, _) in neighbors)
			{
				Console.WriteLine(
	$"Checking edge {currentNodeId} -> {targetNodeId}, weight={weight}");
				var newDistance = currentDistance + weight;

				if (newDistance < distances[targetNodeId])
				{
					distances[targetNodeId] = newDistance;
					previous[targetNodeId] = (currentNodeId, weight);

					unvisited.Add((newDistance, targetNodeId));
				}
			}
		}

		// No path found
		if (double.IsInfinity(distances[endNodeId]))
		{
			return ([], 0);
		}

		// Reconstruct path
		var path = new List<int>();
		var current = endNodeId;

		while (current != startNodeId)
		{
			path.Add(current);

			if (!previous.TryGetValue(current, out var prev))
			{
				return ([], 0);
			}

			current = prev.NodeId;
		}

		path.Add(startNodeId);
		path.Reverse();

		return (path, distances[endNodeId]);
	}
}