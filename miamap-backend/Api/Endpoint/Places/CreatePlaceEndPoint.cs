using Api.Endpoint;
using Application.Places.CreatePlace;
using MediatR;

namespace Api.Endpoint.Places;

public sealed class CreatePlaceEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/places", async (CreatePlaceRequest request, ISender sender, CancellationToken cancellationToken) =>
			{
				var command = new CreatePlaceCommand(
					request.Name,
					request.Category,
					request.Latitude,
					request.Longitude,
					request.Rating,
					request.ReviewCount,
					request.Address);

				var placeId = await sender.Send(command, cancellationToken);
				return Results.Created($"/places/{placeId}", placeId);
			})
			.WithTags(Tags.Places)
			.WithName("CreatePlace")
			.WithSummary("Creates a place")
			.WithDescription("Creates a new place for location search.")
			.Produces<int>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
