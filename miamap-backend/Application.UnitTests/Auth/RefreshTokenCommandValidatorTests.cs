using Application.Auth.Refresh;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Auth;

public sealed class RefreshTokenCommandValidatorTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Validate_Should_BeValid_When_RefreshTokenIsNotEmpty
	/// Mô tả: Đảm bảo validator trả về hợp lệ khi truyền vào một Refresh Token khác rỗng.
	/// Đầu vào: RefreshTokenCommand chứa chuỗi token không rỗng "valid-token-string".
	/// Kết quả kỳ vọng: Kết quả Validate trả về hợp lệ (IsValid = true).
	/// </summary>
	[Fact]
	public void Validate_Should_BeValid_When_RefreshTokenIsNotEmpty()
	{
		// Arrange
		var command = new RefreshTokenCommand("valid-token-string");
		var validator = new RefreshTokenCommandValidator();

		// Act
		var result = validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Validate_Should_HaveError_When_RefreshTokenIsEmpty
	/// Mô tả: Đảm bảo validator trả về lỗi khi chuỗi Refresh Token bị rỗng hoặc khoảng trắng.
	/// Đầu vào: RefreshTokenCommand chứa chuỗi token rỗng "", khoảng trắng "   " hoặc null.
	/// Kết quả kỳ vọng: Kết quả Validate trả về không hợp lệ (IsValid = false) và có thông báo lỗi cho thuộc tính RefreshToken.
	/// </summary>
	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Validate_Should_HaveError_When_RefreshTokenIsEmpty(string? token)
	{
		// Arrange
		var command = new RefreshTokenCommand(token!);
		var validator = new RefreshTokenCommandValidator();

		// Act
		var result = validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == "RefreshToken" && error.ErrorMessage == "Refresh token is required.");
	}
}
