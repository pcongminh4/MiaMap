using System.Security.Claims;
using Application.Users.GetUser;
using Infrastructure.Authentication;
using MediatR;

namespace Api.Endpoint.Users;

public sealed class GetMeEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/users/me", async (
			ClaimsPrincipal claimsPrincipal,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var userId = claimsPrincipal.GetUserId();
			var query = new GetUserQuery(userId);
			var response = await sender.Send(query, cancellationToken);
			return Results.Ok(response);
		})
		.RequireAuthorization()
		.WithTags(Tags.Auth)
		.WithName("GetCurrentUser")
		.WithSummary("Gets current user profile")
		.WithDescription("Retrieves info of the currently logged-in user using the JWT claim.")
		.Produces<GetUserResult>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status401Unauthorized)
		.ProducesProblem(StatusCodes.Status400BadRequest);
	}
}
