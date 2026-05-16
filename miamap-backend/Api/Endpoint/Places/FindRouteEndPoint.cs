using Application.Places.FindRoute;
using MediatR;

namespace Api.Endpoint.Places;

public sealed class FindRouteEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/places/route", async (
			[AsParameters] FindRouteQuery request,
			ISender sender,
			CancellationToken cancellationToken) =>
			{	
				var response = await sender.Send(request, cancellationToken);
				return Results.Ok(response);
			})
			.WithTags(Tags.Places)
			.WithName("FindRoute")
			.WithSummary("Finds a route between two points")
			.WithDescription("Uses Dijkstra algorithm to find the shortest path between start and end coordinates")
			.Produces<FindRouteResult>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}