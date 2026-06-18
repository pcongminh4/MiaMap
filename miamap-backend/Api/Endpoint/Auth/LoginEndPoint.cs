using Application.Users.Login;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;

namespace Api.Endpoint.Auth;

public sealed class LoginEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/auth/login", async (
			HttpContext httpContext,
			LoginCommand request,
			ISender sender,
			IAntiforgery antiforgery,
			CancellationToken cancellationToken) =>
			{
				await antiforgery.ValidateRequestAsync(httpContext);

				// var command = new LoginCommand(
				// 	request.Email,
				// 	request.Password);

				var response = await sender.Send(request, cancellationToken);

				httpContext.Response.Cookies.Append(
					Api.DependencyInjection.AuthCookieName,
					response.AccessToken,
					new CookieOptions
					{
						HttpOnly = true,
						Secure = httpContext.Request.IsHttps,
						SameSite = SameSiteMode.Lax,
						Path = "/",
						Expires = response.ExpiresAtUtc
					});

				httpContext.Response.Cookies.Append(
					Api.DependencyInjection.RefreshCookieName,
					response.RefreshToken,
					new CookieOptions
					{
						HttpOnly = true,
						Secure = httpContext.Request.IsHttps,
						SameSite = SameSiteMode.Lax,
						Path = "/auth",
						Expires = response.RefreshTokenExpiresAtUtc
					});

				return Results.Ok(new { userId = response.UserId });
			})
			.WithTags(Tags.Auth)
			.WithName("LoginUser")
			.WithSummary("Authenticates a user")
			.WithDescription("Authenticates user credentials and stores the tokens in HttpOnly cookies.")
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
