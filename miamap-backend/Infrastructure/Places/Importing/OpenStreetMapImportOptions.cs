namespace Infrastructure.Places.Importing;

public sealed class OpenStreetMapImportOptions
{
	public const string SectionName = "OpenStreetMapImport";

	public bool Enabled { get; set; }

	public string BaseUrl { get; set; } = "https://overpass-api.de/api/interpreter";

	public int MaxPlacesPerArea { get; set; } = 200;

	public int BatchSize { get; set; } = 50;

	public int MaxRetries { get; set; } = 3;

	public int RetryDelaySeconds { get; set; } = 2;

	public List<OpenStreetMapImportArea> Areas { get; set; } = [];
}

public sealed class OpenStreetMapImportArea
{
	public string Name { get; set; } = string.Empty;

	public double South { get; set; }

	public double West { get; set; }

	public double North { get; set; }

	public double East { get; set; }
}
