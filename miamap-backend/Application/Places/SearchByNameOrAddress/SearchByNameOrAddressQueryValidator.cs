using FluentValidation;

namespace Application.Places.SearchByNameOrAddress;

public sealed class SearchByNameOrAddressQueryValidator : AbstractValidator<SearchByNameOrAddressQuery>
{
	public SearchByNameOrAddressQueryValidator()
	{
		RuleFor(query => query.SearchText)
			.NotEmpty()
			.WithMessage("Search text is required.")
			.MinimumLength(1)
			.WithMessage("Search text must be at least 1 character.");

		RuleFor(query => query.Limit)
			.InclusiveBetween(1, 200);
	}
}