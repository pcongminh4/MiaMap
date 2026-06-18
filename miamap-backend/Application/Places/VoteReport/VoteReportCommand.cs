using MediatR;

namespace Application.Places.VoteReport;

public sealed record VoteReportCommand(
    int ReportId,
    int UserId,
    bool IsUpvote) : IRequest;
