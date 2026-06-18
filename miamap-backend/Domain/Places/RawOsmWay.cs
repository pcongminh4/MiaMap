namespace Domain.Places;

public sealed class RawOsmWay
{
	public long Id { get; set; }

	public long[] NodeIds { get; set; } = [];

	public string? Tags { get; set; } // JSONB

	public string? Highway { get; set; }

	public string? Name { get; set; }

	public string? Oneway { get; set; }

	public string? Maxspeed { get; set; }
}
