using Application.Users.Register;
using MediatR;

namespace Api.Endpoint.Auth;

public sealed class RegisterEndPoint : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RegisterCommand(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password);

                var userId = await sender.Send(command, cancellationToken);

                return Results.Created($"/users/{userId}", userId);
            })
            .WithTags(Tags.Auth)
            .WithName("RegisterUser")
            .WithSummary("Registers a new user")
            .WithDescription("Registers a new user by dispatching RegisterCommand via MediatR.")
            .Produces<int>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}