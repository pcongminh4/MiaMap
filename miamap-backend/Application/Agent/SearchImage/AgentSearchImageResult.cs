namespace Application.Results;

public sealed record AgentSearchImageResult(
	bool Success,
	string Message,
	int? RecognizedPlaceId,
	string? RecognizedName,
	double? Latitude,
	double? Longitude,
	string? Category);
