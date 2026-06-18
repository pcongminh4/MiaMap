using MediatR;

namespace Application.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;
