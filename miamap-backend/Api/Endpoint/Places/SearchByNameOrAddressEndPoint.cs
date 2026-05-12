using Application.Places.SearchByNameOrAddress;
using Application.Results;
using MediatR;

namespace Api.Endpoint.Places;

public sealed class SearchByNameOrAddressEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/places/search", async (
			[AsParameters] SearchByNameOrAddressQuery request,
			ISender sender,
			CancellationToken cancellationToken = default) =>
			{
				

				var response = await sender.Send(request, cancellationToken);
				return Results.Ok(response);
			})
			.WithTags(Tags.Places)
			.WithName("SearchByNameOrAddress")
			.WithSummary("Searches places by name or address")
			.WithDescription("Returns active places matching the search text in name or address. Search text is split by spaces and each word is matched against name or address (case-insensitive).")
			.Produces<IEnumerable<SearchByNameOrAddressResult>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}