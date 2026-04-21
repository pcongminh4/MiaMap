namespace Application.Abstractions.Data;

public sealed record ExternalPlaceUpsertRequest(
	string Source,
	string ExternalId,
	string ExternalType,
	string Name,
	string Category,
	string? Address,
	double Latitude,
	double Longitude,
	string? Tags);