using Application.Places;
using MediatR;

namespace Application.Places.SearchNearbyPlaces;

public sealed record SearchNearbyPlacesQuery(
	double Latitude,
	double Longitude,
	double RadiusInMeters,
	int Limit) : IRequest<IReadOnlyList<PlaceNearbyResponse>>;
