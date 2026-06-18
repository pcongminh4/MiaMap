using Application.Common.Abstractions.Data;
using Application.Places.CreatePlace;
using Domain.Places;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class CreatePlaceCommandHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnPlaceIdAndSavePlace_When_CommandIsValid
	/// Mô tả: Đảm bảo khi gửi CreatePlaceCommand hợp lệ, Handler sẽ trim khoảng trắng của tên và địa chỉ,
	///        viết thường tên phân loại (category), thêm Place mới vào DB qua IPlaceRepository và gọi SaveChangesAsync trên IUnitOfWork.
	/// Đầu vào: CreatePlaceCommand với Name=" Nhà Hàng A ", Category=" Restaurant ", Latitude=10.77, Longitude=106.69,
	///          Rating=4.5, ReviewCount=100, Address=" 123 Nguyen Hue ".
	/// Kết quả kỳ vọng: Trả về ID của địa điểm vừa tạo (giả định 0 vì chưa lưu DB thật).
	///                  IPlaceRepository.AddAsync được gọi chính xác 1 lần với đối tượng Place đã được chuẩn hóa dữ liệu
	///                  (Name="Nhà Hàng A", Category="restaurant", Address="123 Nguyen Hue").
	///                  IUnitOfWork.SaveChangesAsync được gọi chính xác 1 lần.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnPlaceIdAndSavePlace_When_CommandIsValid()
	{
		// Arrange
		var command = new CreatePlaceCommand(
			" Nhà Hàng A ",
			" Restaurant ",
			10.77,
			106.69,
			4.5,
			100,
			" 123 Nguyen Hue "
		);

		var placeRepository = Substitute.For<IPlaceRepository>();
		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new CreatePlaceCommandHandler(placeRepository, unitOfWork);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().Be(0); // Id mặc định của Place mới tạo là 0

		await placeRepository.Received(1).AddAsync(Arg.Is<Place>(place =>
			place.Name == "Nhà Hàng A" &&
			place.Category == "restaurant" &&
			place.Point.Y == 10.77 &&
			place.Point.X == 106.69 &&
			place.Rating == 4.5 &&
			place.ReviewCount == 100 &&
			place.Address == "123 Nguyen Hue"
		), Arg.Any<CancellationToken>());

		await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}
}
