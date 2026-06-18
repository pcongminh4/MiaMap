using Application.Auth.Refresh;
using MediatR;

namespace Api.Endpoint.Auth;

public sealed class RefreshTokenEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/auth/refresh", async (
			HttpContext httpContext,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			if (!httpContext.Request.Cookies.TryGetValue(Api.DependencyInjection.RefreshCookieName, out var refreshToken) ||
				string.IsNullOrWhiteSpace(refreshToken))
			{
				return Results.BadRequest(new { error = "Refresh token is missing." });
			}

			var command = new RefreshTokenCommand(refreshToken);
			var response = await sender.Send(command, cancellationToken);

			// Set Access Token cookie mới
			httpContext.Response.Cookies.Append(
				Api.DependencyInjection.AuthCookieName,
				response.AccessToken,
				new CookieOptions
				{
					HttpOnly = true,
					Secure = httpContext.Request.IsHttps,
					SameSite = SameSiteMode.Lax,
					Path = "/",
					Expires = response.AccessTokenExpiresAtUtc
				});

			// Set Refresh Token cookie mới
			httpContext.Response.Cookies.Append(
				Api.DependencyInjection.RefreshCookieName,
				response.NewRefreshToken,
				new CookieOptions
				{
					HttpOnly = true,
					Secure = httpContext.Request.IsHttps,
					SameSite = SameSiteMode.Lax,
					Path = "/auth",
					Expires = response.NewRefreshTokenExpiresAtUtc
				});

			return Results.Ok(new { message = "Token refreshed successfully" });
		})
		.WithTags(Tags.Auth)
		.WithName("RefreshToken")
		.WithSummary("Refreshes access token")
		.WithDescription("Refreshes and issues new access and refresh tokens using HttpOnly cookies.")
		.Produces(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest);
	}
}
