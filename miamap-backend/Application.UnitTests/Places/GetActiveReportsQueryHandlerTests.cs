using Application.Common.Abstractions.Data;
using Application.Places.GetActiveReports;
using Domain.Places;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class GetActiveReportsQueryHandlerTests
{
    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Handle_Should_ReturnActiveReportsInsideBoundingBox_When_QueryIsValid
    /// Mô tả: Đảm bảo khi gửi GetActiveReportsQuery, Handler sẽ gọi IReportRepository.GetActiveReportsInBoundingBoxAsync
    ///        với đúng tọa độ bounding box đầu vào và trả về danh sách DTO ActiveReportResult tương ứng.
    /// Đầu vào: GetActiveReportsQuery với MinLatitude=10.76, MinLongitude=106.68, MaxLatitude=10.79, MaxLongitude=106.71.
    /// Kết quả kỳ vọng: Trả về danh sách chứa 1 ActiveReportResult có đầy đủ thông tin được ánh xạ chính xác từ thực thể Report mẫu.
    ///                  IReportRepository.GetActiveReportsInBoundingBoxAsync được gọi đúng 1 lần với các giá trị bounding box chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ReturnActiveReportsInsideBoundingBox_When_QueryIsValid()
    {
        // Arrange
        var query = new GetActiveReportsQuery(10.76, 106.68, 10.79, 106.71);
        var reportRepository = Substitute.For<IReportRepository>();
        
        var report = new Report(
            createdByUserId: 1,
            reportType: "police",
            subType: "checkpoint",
            latitude: 10.77,
            longitude: 106.69,
            description: "Chốt cảnh sát",
            durationMinutes: 60
        );

        reportRepository.GetActiveReportsInBoundingBoxAsync(
            10.76, 106.68, 10.79, 106.71, Arg.Any<CancellationToken>()
        ).Returns(new List<Report> { report });

        var handler = new GetActiveReportsQueryHandler(reportRepository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var activeReport = result[0];
        activeReport.CreatedByUserId.Should().Be(1);
        activeReport.ReportType.Should().Be("police");
        activeReport.SubType.Should().Be("checkpoint");
        activeReport.Latitude.Should().Be(10.77);
        activeReport.Longitude.Should().Be(106.69);
        activeReport.Description.Should().Be("Chốt cảnh sát");

        await reportRepository.Received(1).GetActiveReportsInBoundingBoxAsync(
            10.76, 106.68, 10.79, 106.71, Arg.Any<CancellationToken>()
        );
    }
}
