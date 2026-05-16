namespace Application.Results;

public sealed record SearchByNameOrAddressResult(
	int PlaceId,
	string Name,
	string Category,	
	string? Address,
	GeoPoint Location,
	double Rating,
	int ReviewCount);