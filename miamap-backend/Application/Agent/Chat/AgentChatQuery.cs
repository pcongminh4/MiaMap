using Application.Results;
using MediatR;

namespace Application.Agent.Chat;

public sealed record AgentChatQuery(
	string Prompt,
	double Latitude,
	double Longitude) : IRequest<AgentChatResult>;
