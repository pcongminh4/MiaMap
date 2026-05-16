namespace Application.Places.FindRoute;

public sealed record FindRouteResult(
	bool Found,
	IReadOnlyList<GeoPoint> PathPoints,
	double TotalDistanceMeters);

