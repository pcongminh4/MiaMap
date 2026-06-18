using NetTopologySuite.Geometries;

namespace Domain.Places;

public sealed class RawOsmNode
{
	public long Id { get; set; }

	public Point Location { get; set; } = null!;

	public string? Name { get; set; }

	public string? Tags { get; set; } // JSONB
}
