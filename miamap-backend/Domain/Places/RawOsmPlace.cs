using NetTopologySuite.Geometries;

namespace Domain.Places;

public sealed class RawOsmPlace
{
	public long Id { get; set; }

	public Point Location { get; set; } = null!;

	public string Name { get; set; } = string.Empty;

	public string Category { get; set; } = string.Empty;

	public string? Address { get; set; }

	public string? Tags { get; set; } // JSONB
}
