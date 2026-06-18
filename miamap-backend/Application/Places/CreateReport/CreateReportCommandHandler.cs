using Application.Common.Abstractions.Data;
using Domain.Places;
using MediatR;

namespace Application.Places.CreateReport;

public sealed class CreateReportCommandHandler(
    IReportRepository reportRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateReportCommand, int>
{
    public async Task<int> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        // Thời gian sống: kẹt xe = 30 phút, cảnh sát/chướng ngại vật = 60 phút, tai nạn = 120 phút
        int durationMinutes = request.ReportType.ToLowerInvariant() switch
        {
            "traffic_jam" => 30,
            "police" => 60,
            "hazard" => 60,
            "accident" => 120,
            _ => 30
        };

        var report = new Report(
            request.CreatedByUserId,
            request.ReportType,
            request.SubType,
            request.Latitude,
            request.Longitude,
            request.Description,
            durationMinutes);

        await reportRepository.AddAsync(report, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return report.Id;
    }
}
