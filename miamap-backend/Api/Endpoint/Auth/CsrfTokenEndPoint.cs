using Microsoft.AspNetCore.Antiforgery;

namespace Api.Endpoint.Auth;

public sealed class CsrfTokenEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/auth/csrf-token", (IAntiforgery antiforgery, HttpContext httpContext) =>
		{
			var tokens = antiforgery.GetAndStoreTokens(httpContext);
			return Results.Ok(new { csrfToken = tokens.RequestToken });
		})
		.WithTags(Tags.Auth)
		.WithName("GetCsrfToken")
		.WithSummary("Gets CSRF token for login")
		.WithDescription("Returns a CSRF token required for login endpoint.")
		.Produces(StatusCodes.Status200OK)
		.AllowAnonymous();
	}
}
