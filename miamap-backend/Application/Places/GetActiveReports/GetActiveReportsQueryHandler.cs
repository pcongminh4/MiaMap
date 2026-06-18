using Application.Common.Abstractions.Data;
using MediatR;

namespace Application.Places.GetActiveReports;

public sealed class GetActiveReportsQueryHandler(IReportRepository reportRepository)
    : IRequestHandler<GetActiveReportsQuery, IReadOnlyList<ActiveReportResult>>
{
    public async Task<IReadOnlyList<ActiveReportResult>> Handle(
        GetActiveReportsQuery request,
        CancellationToken cancellationToken)
    {
        var reports = await reportRepository.GetActiveReportsInBoundingBoxAsync(
            request.MinLatitude,
            request.MinLongitude,
            request.MaxLatitude,
            request.MaxLongitude,
            cancellationToken);

        return reports.Select(r => new ActiveReportResult(
            r.Id,
            r.CreatedByUserId,
            r.ReportType,
            r.SubType,
            r.Location.Y,
            r.Location.X,
            r.Description,
            r.Upvotes,
            r.Downvotes,
            r.CreatedAtUtc,
            r.ExpiresAtUtc
        )).ToList();
    }
}
