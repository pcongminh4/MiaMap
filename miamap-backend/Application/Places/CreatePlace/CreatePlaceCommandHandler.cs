using Application.Common.Abstractions.Data;
using Domain.Places;
using MediatR;

namespace Application.Places.CreatePlace;

public sealed class CreatePlaceCommandHandler(
	IPlaceRepository placeRepository,
	IUnitOfWork unitOfWork)
	: IRequestHandler<CreatePlaceCommand, int>
{
	public async Task<int> Handle(CreatePlaceCommand request, CancellationToken cancellationToken)
	{
		var place = new Place(
			request.Name.Trim(),
			request.Category.Trim().ToLowerInvariant(),
			request.Latitude,
			request.Longitude,
			request.Rating,
			request.ReviewCount,
			string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim());

		await placeRepository.AddAsync(place, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return place.Id;
	}
}

