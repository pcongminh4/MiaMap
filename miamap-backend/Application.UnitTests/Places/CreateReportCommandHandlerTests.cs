using Application.Common.Abstractions.Data;
using Application.Places.CreateReport;
using Domain.Places;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class CreateReportCommandHandlerTests
{
    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Handle_Should_SaveReportWithAppropriateExpiration_When_CommandIsValid
    /// Mô tả: Đảm bảo khi gửi CreateReportCommand hợp lệ, Handler sẽ tạo đối tượng Report mới với thời gian sống hết hạn
    ///        tương ứng với loại sự cố (kẹt xe = 30 phút, cảnh sát/chướng ngại vật = 60 phút, tai nạn = 120 phút),
    ///        thêm vào DB qua IReportRepository và gọi SaveChangesAsync trên IUnitOfWork.
    /// Đầu vào: CreateReportCommand với CreatedByUserId=1, ReportType="traffic_jam", SubType="heavy",
    ///          Latitude=10.77, Longitude=106.69, Description="Kẹt xe nặng".
    /// Kết quả kỳ vọng: Trả về ID của báo cáo vừa tạo (giả định 0).
    ///                  IReportRepository.AddAsync được gọi chính xác 1 lần với thực thể Report có ExpiresAtUtc
    ///                  khoảng 30 phút so với thời điểm hiện tại.
    ///                  IUnitOfWork.SaveChangesAsync được gọi chính xác 1 lần.
    /// </summary>
    [Fact]
    public async Task Handle_Should_SaveReportWithAppropriateExpiration_When_CommandIsValid()
    {
        // Arrange
        var command = new CreateReportCommand(
            1,
            "traffic_jam",
            "heavy",
            10.77,
            106.69,
            "Kẹt xe nặng"
        );

        var reportRepository = Substitute.For<IReportRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new CreateReportCommandHandler(reportRepository, unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(0);

        await reportRepository.Received(1).AddAsync(Arg.Is<Report>(r =>
            r.CreatedByUserId == 1 &&
            r.ReportType == "traffic_jam" &&
            r.SubType == "heavy" &&
            r.Location.Y == 10.77 &&
            r.Location.X == 106.69 &&
            r.Description == "Kẹt xe nặng" &&
            r.IsActive &&
            (r.ExpiresAtUtc - DateTime.UtcNow).TotalMinutes >= 28 &&
            (r.ExpiresAtUtc - DateTime.UtcNow).TotalMinutes <= 32
        ), Arg.Any<CancellationToken>());

        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
