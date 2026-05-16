using NetTopologySuite.Geometries;

namespace Domain.Places;

public sealed class Node
{
	public Node(Point location, string? name = null)
	{
		Location = location;
		Name = name;
		IsActive = true;
		CreatedAtUtc = DateTime.UtcNow;
	}

	private Node()
	{
	}

	public int Id { get; private set; }

	public string? Source { get; private set; }

	public string? ExternalId { get; private set; }

	public Point Location { get; private set; } = new(0, 0) { SRID = 4326 };

	public string? Name { get; private set; }

	public bool IsActive { get; private set; }

	public DateTime CreatedAtUtc { get; private set; }

	public DateTime? UpdatedAtUtc { get; private set; }

	public ICollection<Road> OutgoingRoads { get; private set; } = new List<Road>();

	public ICollection<Road> IncomingRoads { get; private set; } = new List<Road>();

	public static Node CreateFromExternal(
		string source,
		string externalId,
		double latitude,
		double longitude,
		string? name,
		DateTime createdAtUtc)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			throw new ArgumentException("Node source is required.", nameof(source));
		}

		if (string.IsNullOrWhiteSpace(externalId))
		{
			throw new ArgumentException("Node external id is required.", nameof(externalId));
		}

		var node = new Node(new Point(longitude, latitude) { SRID = 4326 }, name?.Trim());
		node.Source = source.Trim().ToLowerInvariant();
		node.ExternalId = externalId.Trim();
		node.CreatedAtUtc = createdAtUtc;
		return node;
	}

	public void UpdateFromExternal(double latitude, double longitude, string? name)
	{
		Location = new Point(longitude, latitude) { SRID = 4326 };
		Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
		UpdatedAtUtc = DateTime.UtcNow;
	}
}