namespace Application.Abstractions.Data;

public sealed record BoudingBoxResult(
	int PlaceId,
	string Name,
	string Category,
	string? Address,
	double Latitude,
	double Longitude,
	double Rating,
	int ReviewCount
);