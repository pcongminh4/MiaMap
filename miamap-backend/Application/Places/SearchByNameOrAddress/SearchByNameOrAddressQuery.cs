using Application.Common.Abstractions.Data;
using Application.Results;
using MediatR;

namespace Application.Places.SearchByNameOrAddress;

public sealed record SearchByNameOrAddressQuery(
	string SearchText,
	int Limit = 6) : IRequest<IReadOnlyList<SearchByNameOrAddressResult>>;