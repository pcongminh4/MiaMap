using MediatR;

namespace Application.Places.CreatePlace;

public sealed record CreatePlaceCommand(
	string Name,
	string Category,
	double Latitude,
	double Longitude,
	double Rating,
	int ReviewCount,
	string? Address) : IRequest<int>;
