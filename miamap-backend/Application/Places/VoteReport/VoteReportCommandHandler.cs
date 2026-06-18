using Application.Common.Abstractions.Data;
using Domain.Places;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Places.VoteReport;

public sealed class VoteReportCommandHandler(
    IReportRepository reportRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<VoteReportCommand>
{
    public async Task Handle(VoteReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reportRepository.GetByIdAsync(request.ReportId, cancellationToken);
        if (report is null || !report.IsActive)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("ReportId", "Report not found or inactive.")
            });
        }
        report.RecordVote(request.UserId, request.IsUpvote);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
