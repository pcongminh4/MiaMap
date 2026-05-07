using Application.Abstractions.Data;
using MediatR;

namespace Application.Places.SearchBoundingBoxPlaces;

public sealed record SearchBoundingBoxPlacesQuery(
	double MinLatitude,
	double MinLongitude,
	double MaxLatitude,
	double MaxLongitude,
	int Limit) : IRequest<IReadOnlyList<BoudingBoxResult>>;
