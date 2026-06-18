using MediatR;

namespace Application.Places.CreateReport;

public sealed record CreateReportCommand(
    int CreatedByUserId,
    string ReportType,
    string? SubType,
    double Latitude,
    double Longitude,
    string? Description) : IRequest<int>;
