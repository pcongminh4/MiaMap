namespace Api.Endpoint.Places;

public sealed class SearchBoundingBoxPlacesRequest
{
	public double MinLatitude { get; init; }
	public double MinLongitude { get; init; }
	public double MaxLatitude { get; init; }
	public double MaxLongitude { get; init; }
	public int Limit { get; init; } = 100;
}
