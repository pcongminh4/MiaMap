using FluentValidation;

namespace Application.Places.CreateReport;

public sealed class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "traffic_jam", "police", "accident", "hazard"
    };

    public CreateReportCommandValidator()
    {
        RuleFor(x => x.CreatedByUserId).GreaterThan(0);
        RuleFor(x => x.ReportType).NotEmpty().Must(type => AllowedTypes.Contains(type))
            .WithMessage("Report type must be traffic_jam, police, accident, or hazard.");
        RuleFor(x => x.Latitude).InclusiveBetween(10.7600, 10.7950)
            .WithMessage("Latitude must be within District 1 (10.7600 to 10.7950).");
        RuleFor(x => x.Longitude).InclusiveBetween(106.6800, 106.7150)
            .WithMessage("Longitude must be within District 1 (106.6800 to 106.7150).");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
