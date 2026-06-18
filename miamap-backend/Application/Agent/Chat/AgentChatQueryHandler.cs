using System.Threading;
using System.Threading.Tasks;
using Application.Common.Abstractions.Agent;
using Application.Results;
using MediatR;

namespace Application.Agent.Chat;

public sealed class AgentChatQueryHandler(IAgentService agentService)
	: IRequestHandler<AgentChatQuery, AgentChatResult>
{
	public async Task<AgentChatResult> Handle(AgentChatQuery request, CancellationToken cancellationToken)
	{
		return await agentService.ChatAsync(
			request.Prompt,
			request.Latitude,
			request.Longitude,
			cancellationToken);
	}
}
