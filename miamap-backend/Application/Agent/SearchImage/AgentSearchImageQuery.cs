using System.IO;
using Application.Results;
using MediatR;

namespace Application.Agent.SearchImage;

public sealed record AgentSearchImageQuery(
	Stream ImageStream,
	double Latitude,
	double Longitude) : IRequest<AgentSearchImageResult>;
