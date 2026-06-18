using Application.Common.Abstractions.Data;
using Application.Places.SearchNearbyPlaces;
using Application.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class SearchNearbyPlacesQueryHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnNearbyPlaceResults_When_QueryIsReceived
	/// Mô tả: Đảm bảo khi gửi SearchNearbyPlacesQuery với tọa độ trung tâm, bán kính (Radius) và giới hạn (Limit),
	///        QueryHandler sẽ gọi đúng phương thức IPlaceRepository.SearchNearbyAsync và trả về danh sách kết quả mẫu.
	/// Đầu vào: SearchNearbyPlacesQuery với Lat=10.77, Lng=106.69, RadiusInMeters=500, Limit=20.
	///          Giả lập IPlaceRepository.SearchNearbyAsync trả về danh sách PlaceNearbyResult mẫu.
	/// Kết quả kỳ vọng: Trả về danh sách PlaceNearbyResult khớp với kết quả mock.
	///                  IPlaceRepository.SearchNearbyAsync được gọi chính xác 1 lần với đúng các tham số.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnNearbyPlaceResults_When_QueryIsReceived()
	{
		// Arrange
		var query = new SearchNearbyPlacesQuery(10.77, 106.69, 500, 20);

		var expectedResults = new List<PlaceNearbyResult>
		{
			new(1, "Place A", "cafe", "123 Le Loi", new GeoPoint(10.771, 106.691), 4.5, 100, 120),
			new(2, "Place B", "restaurant", "456 Dong Khoi", new GeoPoint(10.772, 106.692), 4.0, 50, 250)
		};

		var placeRepository = Substitute.For<IPlaceRepository>();
		placeRepository.SearchNearbyAsync(
			query.Latitude,
			query.Longitude,
			query.RadiusInMeters,
			query.Limit,
			Arg.Any<CancellationToken>()
		).Returns(expectedResults);

		var handler = new SearchNearbyPlacesQueryHandler(placeRepository);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEquivalentTo(expectedResults);

		await placeRepository.Received(1).SearchNearbyAsync(
			query.Latitude,
			query.Longitude,
			query.RadiusInMeters,
			query.Limit,
			Arg.Any<CancellationToken>()
		);
	}
}
