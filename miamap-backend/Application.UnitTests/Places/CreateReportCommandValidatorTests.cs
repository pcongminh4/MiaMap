using Application.Places.CreateReport;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Places;

public sealed class CreateReportCommandValidatorTests
{
    private readonly CreateReportCommandValidator _validator = new();

    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Validate_Should_BeValid_When_CommandIsValid
    /// Mô tả: Đảm bảo khi gửi CreateReportCommand có tọa độ trong District 1 và loại sự cố hợp lệ, Validator trả về hợp lệ.
    /// Đầu vào: CreateReportCommand với CreatedByUserId=1, ReportType="traffic_jam", Latitude=10.77, Longitude=106.69.
    /// Kết quả kỳ vọng: ValidationResult.IsValid là true.
    /// </summary>
    [Fact]
    public void Validate_Should_BeValid_When_CommandIsValid()
    {
        var command = new CreateReportCommand(1, "traffic_jam", null, 10.77, 106.69, "Ok");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Validate_Should_BeInvalid_When_ReportTypeIsInvalid
    /// Mô tả: Đảm bảo Validator báo lỗi nếu loại sự cố không thuộc danh sách cho phép (traffic_jam, police, accident, hazard).
    /// Đầu vào: CreateReportCommand với ReportType="unknown_type".
    /// Kết quả kỳ vọng: ValidationResult.IsValid là false. Có thông báo lỗi cho thuộc tính ReportType.
    /// </summary>
    [Fact]
    public void Validate_Should_BeInvalid_When_ReportTypeIsInvalid()
    {
        var command = new CreateReportCommand(1, "unknown_type", null, 10.77, 106.69, "Ok");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "ReportType");
    }

    /// <summary>
    /// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
    /// Tên test: Validate_Should_BeInvalid_When_CoordinatesAreOutsideDistrict1
    /// Mô tả: Đảm bảo Validator báo lỗi nếu tọa độ nằm ngoài phạm vi Quận 1 (Latitude: 10.7600 to 10.7950, Longitude: 106.6800 to 106.7150).
    /// Đầu vào: CreateReportCommand với Latitude=10.5, Longitude=106.5.
    /// Kết quả kỳ vọng: ValidationResult.IsValid là false. Có thông báo lỗi cho cả Latitude và Longitude.
    /// </summary>
    [Fact]
    public void Validate_Should_BeInvalid_When_CoordinatesAreOutsideDistrict1()
    {
        var command = new CreateReportCommand(1, "traffic_jam", null, 10.5, 106.5, "Ok");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Latitude");
        result.Errors.Should().Contain(e => e.PropertyName == "Longitude");
    }
}
