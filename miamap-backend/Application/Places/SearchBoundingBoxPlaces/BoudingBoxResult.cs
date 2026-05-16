namespace Application.Results;

public sealed record BoundingBoxResult(
	int PlaceId,
	string Name,
	string Category,
	string? Address,
	GeoPoint Location,
	double Rating,
	int ReviewCount
);