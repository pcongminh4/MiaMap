using FluentValidation;

namespace Application.Places.SearchNearbyPlaces;

public sealed class SearchNearbyPlacesQueryValidator : AbstractValidator<SearchNearbyPlacesQuery>
{
	public SearchNearbyPlacesQueryValidator()
	{
		RuleFor(query => query.Latitude)
			.InclusiveBetween(-90, 90);

		RuleFor(query => query.Longitude)
			.InclusiveBetween(-180, 180);

		RuleFor(query => query.RadiusInMeters)
			.GreaterThan(0)
			.LessThanOrEqualTo(50_000);

		RuleFor(query => query.Limit)
			.InclusiveBetween(1, 200);
	}
}
