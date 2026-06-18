namespace Application.Results;

public sealed record PlaceNearbyResult(
	int PlaceId,
	string Name,
	string Category,
	string? Address,
	GeoPoint Location,
	double Rating,
	int ReviewCount,
	double DistanceInMeters,
	string? ImageUrl);