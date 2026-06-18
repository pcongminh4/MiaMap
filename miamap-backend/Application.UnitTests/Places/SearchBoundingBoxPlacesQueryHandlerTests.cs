using Application.Common.Abstractions.Data;
using Application.Places.SearchBoundingBoxPlaces;
using Application.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class SearchBoundingBoxPlacesQueryHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnBoundingBoxResults_When_QueryIsReceived
	/// Mô tả: Đảm bảo khi gửi SearchBoundingBoxPlacesQuery với các giới hạn tọa độ và giới hạn số lượng (Limit),
	///        QueryHandler sẽ gọi đúng phương thức IPlaceRepository.BoudingBoxSearchAsync và trả về danh sách kết quả mẫu.
	/// Đầu vào: SearchBoundingBoxPlacesQuery với MinLat=10.76, MinLng=106.68, MaxLat=10.79, MaxLng=106.71, Limit=50.
	///          Giả lập IPlaceRepository.BoudingBoxSearchAsync trả về một danh sách BoundingBoxResult mẫu.
	/// Kết quả kỳ vọng: Trả về danh sách BoundingBoxResult khớp với danh sách mẫu.
	///                  IPlaceRepository.BoudingBoxSearchAsync được gọi chính xác 1 lần với đúng các tham số đầu vào.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnBoundingBoxResults_When_QueryIsReceived()
	{
		// Arrange
		var query = new SearchBoundingBoxPlacesQuery(10.76, 106.68, 10.79, 106.71, 50);

		var expectedResults = new List<BoundingBoxResult>
		{
			new(1, "Place A", "cafe", "123 Le Loi", new GeoPoint(10.77, 106.69), 4.5, 100, null),
			new(2, "Place B", "restaurant", "456 Dong Khoi", new GeoPoint(10.78, 106.70), 4.0, 50, null)
		};

		var placeRepository = Substitute.For<IPlaceRepository>();
		placeRepository.BoudingBoxSearchAsync(
			query.MinLatitude,
			query.MinLongitude,
			query.MaxLatitude,
			query.MaxLongitude,
			query.Limit,
			Arg.Any<CancellationToken>()
		).Returns(expectedResults);

		var handler = new SearchBoundingBoxPlacesQueryHandler(placeRepository);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEquivalentTo(expectedResults);

		await placeRepository.Received(1).BoudingBoxSearchAsync(
			query.MinLatitude,
			query.MinLongitude,
			query.MaxLatitude,
			query.MaxLongitude,
			query.Limit,
			Arg.Any<CancellationToken>()
		);
	}
}
