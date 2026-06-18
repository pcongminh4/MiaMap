using MediatR;

namespace Application.Places.GetActiveReports;

public sealed record GetActiveReportsQuery(
    double MinLatitude,
    double MinLongitude,
    double MaxLatitude,
    double MaxLongitude) : IRequest<IReadOnlyList<ActiveReportResult>>;
