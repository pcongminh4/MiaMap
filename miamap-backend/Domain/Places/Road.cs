using NetTopologySuite.Geometries;

namespace Domain.Places;

public sealed class Road
{
	public Road(
		LineString geometry,
		double lengthMeters,
		int startNodeId,
		int endNodeId,
		bool isOneWay = false,
		string? roadName = null,
		string? roadType = null,
		int maxSpeedKmh = 50,
		double? weight = null)
	{
		Geometry = geometry;
		LengthMeters = lengthMeters;
		StartNodeId = startNodeId;
		EndNodeId = endNodeId;
		IsOneWay = isOneWay;
		RoadName = roadName;
		RoadType = roadType;
		MaxSpeedKmh = maxSpeedKmh;
		Weight = weight ?? lengthMeters;
		IsActive = true;
		CreatedAtUtc = DateTime.UtcNow;
	}

	private Road()
	{
	}

	public int Id { get; private set; }

	public string? Source { get; private set; }

	public string? ExternalId { get; private set; }

	public LineString Geometry { get; private set; } = null!;

	public double LengthMeters { get; private set; }

	public int StartNodeId { get; private set; }

	public Node StartNode { get; private set; } = null!;

	public int EndNodeId { get; private set; }

	public Node EndNode { get; private set; } = null!;

	public bool IsOneWay { get; private set; }

	public string? RoadName { get; private set; }

	public string? RoadType { get; private set; }

	public int MaxSpeedKmh { get; private set; }

	public double Weight { get; private set; }

	public bool IsActive { get; private set; }

	public DateTime CreatedAtUtc { get; private set; }

	public DateTime? UpdatedAtUtc { get; private set; }

	public static Road CreateFromExternal(
		string source,
		string externalId,
		LineString geometry,
		double lengthMeters,
		int startNodeId,
		int endNodeId,
		bool isOneWay = false,
		string? roadName = null,
		string? roadType = null,
		int maxSpeedKmh = 50,
		double? weight = null)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			throw new ArgumentException("Road source is required.", nameof(source));
		}

		if (string.IsNullOrWhiteSpace(externalId))
		{
			throw new ArgumentException("Road external id is required.", nameof(externalId));
		}

		var road = new Road(
			geometry,
			lengthMeters,
			startNodeId,
			endNodeId,
			isOneWay,
			roadName,
			roadType,
			maxSpeedKmh,
			weight);
		road.Source = source.Trim().ToLowerInvariant();
		road.ExternalId = externalId.Trim();
		return road;
	}

	public void UpdateFromExternal(
		LineString geometry,
		double lengthMeters,
		int startNodeId,
		int endNodeId,
		bool isOneWay = false,
		string? roadName = null,
		string? roadType = null,
		int maxSpeedKmh = 50,
		double? weight = null)
	{
		Geometry = geometry;
		LengthMeters = lengthMeters;
		StartNodeId = startNodeId;
		EndNodeId = endNodeId;
		IsOneWay = isOneWay;
		RoadName = roadName;
		RoadType = roadType;
		MaxSpeedKmh = maxSpeedKmh;
		Weight = weight ?? lengthMeters;
		UpdatedAtUtc = DateTime.UtcNow;
	}
}