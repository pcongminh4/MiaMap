using MediatR;

namespace Application.Places.FindRoute;

public sealed record FindRouteQuery(
	double StartLatitude,
	double StartLongitude,
	double EndLatitude,
	double EndLongitude) : IRequest<FindRouteResult>;