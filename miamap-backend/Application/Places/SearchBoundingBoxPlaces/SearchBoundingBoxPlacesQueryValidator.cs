using FluentValidation;

namespace Application.Places.SearchBoundingBoxPlaces;

public sealed class SearchBoundingBoxPlacesQueryValidator : AbstractValidator<SearchBoundingBoxPlacesQuery>
{
	public SearchBoundingBoxPlacesQueryValidator()
	{
		RuleFor(query => query.MinLatitude)
			.InclusiveBetween(-90, 90);

		RuleFor(query => query.MaxLatitude)
			.InclusiveBetween(-90, 90)
			.GreaterThanOrEqualTo(query => query.MinLatitude);

		RuleFor(query => query.MinLongitude)
			.InclusiveBetween(-180, 180);

		RuleFor(query => query.MaxLongitude)
			.InclusiveBetween(-180, 180)
			.GreaterThanOrEqualTo(query => query.MinLongitude);

		RuleFor(query => query.Limit)
			.InclusiveBetween(1, 500);
	}
}
