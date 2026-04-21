namespace Api.Endpoint.Places;

public sealed class CreatePlaceRequest
{
	public string Name { get; init; } = string.Empty;
	public string Category { get; init; } = string.Empty;
	public double Latitude { get; init; }
	public double Longitude { get; init; }
	public double Rating { get; init; }
	public int ReviewCount { get; init; }
	public string? Address { get; init; }
}
