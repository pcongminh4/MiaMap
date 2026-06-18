namespace Application.Places.GetActiveReports;

public sealed record ActiveReportResult(
    int Id,
    int CreatedByUserId,
    string ReportType,
    string? SubType,
    double Latitude,
    double Longitude,
    string? Description,
    int Upvotes,
    int Downvotes,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc);
