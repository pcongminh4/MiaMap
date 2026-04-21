using FluentValidation;

namespace Application.Places.CreatePlace;

public sealed class CreatePlaceCommandValidator : AbstractValidator<CreatePlaceCommand>
{
	public CreatePlaceCommandValidator()
	{
		RuleFor(command => command.Name)
			.NotEmpty()
			.MaximumLength(200);

		RuleFor(command => command.Category)
			.NotEmpty()
			.MaximumLength(100);

		RuleFor(command => command.Latitude)
			.InclusiveBetween(-90, 90);

		RuleFor(command => command.Longitude)
			.InclusiveBetween(-180, 180);

		RuleFor(command => command.Rating)
			.InclusiveBetween(0, 5);

		RuleFor(command => command.ReviewCount)
			.GreaterThanOrEqualTo(0);

		RuleFor(command => command.Address)
			.MaximumLength(500)
			.When(command => !string.IsNullOrWhiteSpace(command.Address));
	}
}
