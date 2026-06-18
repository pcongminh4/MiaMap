using System.Threading;
using System.Threading.Tasks;
using Application.Common.Abstractions.Agent;
using Application.Results;
using MediatR;

namespace Application.Agent.SearchImage;

public sealed class AgentSearchImageQueryHandler(IAgentService agentService)
	: IRequestHandler<AgentSearchImageQuery, AgentSearchImageResult>
{
	public async Task<AgentSearchImageResult> Handle(AgentSearchImageQuery request, CancellationToken cancellationToken)
	{
		return await agentService.SearchByImageAsync(
			request.ImageStream,
			request.Latitude,
			request.Longitude,
			cancellationToken);
	}
}
