using Application.Common.Abstractions.Data;
using Microsoft.AspNetCore.Antiforgery;

namespace Api.Endpoint.Auth;

public sealed class LogoutEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/auth/logout", async (
			HttpContext httpContext,
			IRefreshTokenRepository refreshTokenRepository,
			IUnitOfWork unitOfWork,
			IAntiforgery antiforgery) =>
		{
			await antiforgery.ValidateRequestAsync(httpContext);

			// Thu hồi token trong DB nếu tồn tại
			if (httpContext.Request.Cookies.TryGetValue(Api.DependencyInjection.RefreshCookieName, out var refreshTokenString) &&
				!string.IsNullOrWhiteSpace(refreshTokenString))
			{
				var refreshToken = await refreshTokenRepository.GetByTokenAsync(refreshTokenString);
				if (refreshToken is not null)
				{
					refreshToken.Revoke();
					await unitOfWork.SaveChangesAsync();
				}
			}

			// Xóa access_token cookie
			httpContext.Response.Cookies.Delete(Api.DependencyInjection.AuthCookieName, new CookieOptions
			{
				HttpOnly = true,
				Secure = httpContext.Request.IsHttps,
				SameSite = SameSiteMode.Lax,
				Path = "/"
			});

			// Xóa refresh_token cookie
			httpContext.Response.Cookies.Delete(Api.DependencyInjection.RefreshCookieName, new CookieOptions
			{
				HttpOnly = true,
				Secure = httpContext.Request.IsHttps,
				SameSite = SameSiteMode.Lax,
				Path = "/auth"
			});

			return Results.Ok(new { message = "Logged out successfully" });
		})
		.WithTags(Tags.Auth)
		.WithName("LogoutUser")
		.WithSummary("Logs out the current user")
		.WithDescription("Clears authorization tokens and revokes the active refresh token.")
		.Produces(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest);
	}
}
