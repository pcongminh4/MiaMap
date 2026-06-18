using Application.Common.Abstractions.Data;
using Application.Places.FindRoute;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class FindRouteQueryHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnRouteResult_When_QueryIsReceived
	/// Mô tả: Đảm bảo khi gửi FindRouteQuery với tọa độ điểm đầu và cuối, QueryHandler sẽ chuyển tiếp tham số tọa độ tới
	///        IRoutingRepository.FindRouteAsync và trả về đúng đối tượng kết quả định tuyến đã được mock.
	/// Đầu vào: FindRouteQuery với tọa độ đầu (10.77, 106.68), tọa độ cuối (10.78, 106.69).
	///          Giả lập IRoutingRepository.FindRouteAsync trả về một FindRouteResult chứa danh sách các điểm GeoPoint.
	/// Kết quả kỳ vọng: Trả về đối tượng FindRouteResult không null và khớp với kết quả mock.
	///                  IRoutingRepository.FindRouteAsync được gọi chính xác 1 lần với đúng các tọa độ.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnRouteResult_When_QueryIsReceived()
	{
		// Arrange
		var query = new FindRouteQuery(10.77, 106.68, 10.78, 106.69);

		var expectedPoints = new List<GeoPoint>
		{
			new(10.77, 106.68),
			new(10.775, 106.685),
			new(10.78, 106.69)
		};
		var expectedResult = new FindRouteResult(true, expectedPoints, 120.5);

		var routingRepository = Substitute.For<IRoutingRepository>();
		routingRepository.FindRouteAsync(
			query.StartLatitude,
			query.StartLongitude,
			query.EndLatitude,
			query.EndLongitude,
			Arg.Any<CancellationToken>()
		).Returns(expectedResult);

		var handler = new FindRouteQueryHandler(routingRepository);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.PathPoints.Should().BeEquivalentTo(expectedPoints);

		await routingRepository.Received(1).FindRouteAsync(
			query.StartLatitude,
			query.StartLongitude,
			query.EndLatitude,
			query.EndLongitude,
			Arg.Any<CancellationToken>()
		);
	}
}
