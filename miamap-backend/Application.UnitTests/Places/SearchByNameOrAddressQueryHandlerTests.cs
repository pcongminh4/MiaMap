using Application.Common.Abstractions.Data;
using Application.Places.SearchByNameOrAddress;
using Application.Results;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class SearchByNameOrAddressQueryHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnSearchByNameOrAddressResults_When_QueryIsReceived
	/// Mô tả: Đảm bảo khi gửi SearchByNameOrAddressQuery với từ khóa tìm kiếm (SearchText) và giới hạn (Limit),
	///        QueryHandler sẽ gọi đúng phương thức IPlaceRepository.SearchByNameOrAddressAsync và trả về danh sách kết quả mẫu.
	/// Đầu vào: SearchByNameOrAddressQuery với SearchText="café", Limit=10.
	///          Giả lập IPlaceRepository.SearchByNameOrAddressAsync trả về danh sách SearchByNameOrAddressResult mẫu.
	/// Kết quả kỳ vọng: Trả về danh sách SearchByNameOrAddressResult khớp với kết quả mock.
	///                  IPlaceRepository.SearchByNameOrAddressAsync được gọi chính xác 1 lần với đúng các tham số.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnSearchByNameOrAddressResults_When_QueryIsReceived()
	{
		// Arrange
		var query = new SearchByNameOrAddressQuery("café", 10);

		var expectedResults = new List<SearchByNameOrAddressResult>
		{
			new(1, "Highlands Coffee", "cafe", "123 Le Loi", new GeoPoint(10.772, 106.698), 4.2, 150, null),
			new(2, "Trung Nguyen Legend", "cafe", "456 Dong Khoi", new GeoPoint(10.775, 106.695), 4.5, 200, null)
		};

		var placeRepository = Substitute.For<IPlaceRepository>();
		placeRepository.SearchByNameOrAddressAsync(
			query.SearchText,
			query.Limit,
			Arg.Any<CancellationToken>()
		).Returns(expectedResults);

		var handler = new SearchByNameOrAddressQueryHandler(placeRepository);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEquivalentTo(expectedResults);

		await placeRepository.Received(1).SearchByNameOrAddressAsync(
			query.SearchText,
			query.Limit,
			Arg.Any<CancellationToken>()
		);
	}
}
