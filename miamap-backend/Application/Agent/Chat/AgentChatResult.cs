using System.Collections.Generic;

namespace Application.Results;

public sealed record AgentChatResult(
	string Answer,
	IReadOnlyList<int> RecommendedPlaces,
	string MapAction,
	object? MapActionPayload = null);
