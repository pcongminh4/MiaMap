namespace Application.Results;

public sealed record SearchByNameOrAddressResult(
	int PlaceId,
	string Name,
	string Category,
	string? Address,
	double Latitude,
	double Longitude,
	double Rating,
	int ReviewCount);