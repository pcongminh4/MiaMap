namespace Application.Results;

public sealed record PlaceNearbyResult(
	int PlaceId,
	string Name,
	string Category,
	string? Address,
	double Latitude,
	double Longitude,
	double Rating,
	int ReviewCount,
	double DistanceInMeters);