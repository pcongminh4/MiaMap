using MediatR;

namespace Application.Users.GetUser;

public sealed record GetUserQuery(int UserId) : IRequest<GetUserResult>;
