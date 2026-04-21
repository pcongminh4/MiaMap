using Application.Users.Login;
using MediatR;

namespace Api.Endpoint.Auth;

public sealed class LoginEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/auth/login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
			{
				var command = new LoginCommand(
					request.Email,
					request.Password);

				var response = await sender.Send(command, cancellationToken);
				return Results.Ok(response);
			})
			.WithTags(Tags.Auth)
			.WithName("LoginUser")
			.WithSummary("Authenticates a user")
			.WithDescription("Authenticates user credentials and returns JWT access token.")
			.Produces<LoginResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
