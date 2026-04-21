namespace Api.Endpoint.Places;

public sealed class SearchNearbyPlacesRequest
{
	public double Latitude { get; init; }
	public double Longitude { get; init; }
	public double RadiusInMeters { get; init; } = 1000;
	public int Limit { get; init; } = 20;
}
