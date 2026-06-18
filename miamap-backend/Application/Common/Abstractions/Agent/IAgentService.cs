using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Application.Results;

namespace Application.Common.Abstractions.Agent;

public interface IAgentService
{
	Task<AgentChatResult> ChatAsync(
		string prompt,
		double userLat,
		double userLng,
		CancellationToken cancellationToken = default);

	Task<AgentSearchImageResult> SearchByImageAsync(
		Stream imageStream,
		double userLat,
		double userLng,
		CancellationToken cancellationToken = default);
}
