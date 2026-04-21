namespace Application.Places;

public sealed record PlaceNearbyResponse(
	int PlaceId,
	string Name,
	string Category,
	string? Address,
	double Latitude,
	double Longitude,
	double Rating,
	int ReviewCount,
	double DistanceInMeters);
