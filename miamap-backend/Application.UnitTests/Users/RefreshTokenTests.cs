using Domain.Users;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Users;

public sealed class RefreshTokenTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: IsExpired_Should_ReturnTrue_When_TokenIsExpired
	/// Mô tả: Đảm bảo thuộc tính IsExpired trả về true khi thời gian hiện tại đã vượt quá hạn dùng ExpiresAtUtc.
	/// Đầu vào: ExpiresAtUtc = 1 giờ trước.
	/// Kết quả kỳ vọng: IsExpired = true.
	/// </summary>
	[Fact]
	public void IsExpired_Should_ReturnTrue_When_TokenIsExpired()
	{
		// Arrange
		var expiresAt = DateTime.UtcNow.AddHours(-1);
		var token = new RefreshToken(userId: 1, token: "test-token", expiresAtUtc: expiresAt);

		// Act
		var isExpired = token.IsExpired;

		// Assert
		isExpired.Should().BeTrue();
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: IsExpired_Should_ReturnFalse_When_TokenIsNotExpired
	/// Mô tả: Đảm bảo thuộc tính IsExpired trả về false khi thời gian hiện tại chưa vượt quá hạn dùng ExpiresAtUtc.
	/// Đầu vào: ExpiresAtUtc = 1 giờ sau.
	/// Kết quả kỳ vọng: IsExpired = false.
	/// </summary>
	[Fact]
	public void IsExpired_Should_ReturnFalse_When_TokenIsNotExpired()
	{
		// Arrange
		var expiresAt = DateTime.UtcNow.AddHours(1);
		var token = new RefreshToken(userId: 1, token: "test-token", expiresAtUtc: expiresAt);

		// Act
		var isExpired = token.IsExpired;

		// Assert
		isExpired.Should().BeFalse();
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: IsActive_Should_ReturnTrue_When_TokenIsNotExpiredAndNotRevoked
	/// Mô tả: Đảm bảo thuộc tính IsActive trả về true khi token chưa hết hạn và chưa bị thu hồi.
	/// Đầu vào: IsRevoked = false, ExpiresAtUtc = 1 ngày sau.
	/// Kết quả kỳ vọng: IsActive = true.
	/// </summary>
	[Fact]
	public void IsActive_Should_ReturnTrue_When_TokenIsNotExpiredAndNotRevoked()
	{
		// Arrange
		var expiresAt = DateTime.UtcNow.AddDays(1);
		var token = new RefreshToken(userId: 1, token: "test-token", expiresAtUtc: expiresAt);

		// Act
		var isActive = token.IsActive;

		// Assert
		isActive.Should().BeTrue();
		token.IsRevoked.Should().BeFalse();
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: IsActive_Should_ReturnFalse_When_TokenIsRevoked
	/// Mô tả: Đảm bảo thuộc tính IsActive trả về false và IsRevoked trả về true sau khi gọi hàm Revoke().
	/// Đầu vào: Gọi hàm Revoke() trên token chưa hết hạn.
	/// Kết quả kỳ vọng: IsActive = false, IsRevoked = true.
	/// </summary>
	[Fact]
	public void IsActive_Should_ReturnFalse_When_TokenIsRevoked()
	{
		// Arrange
		var expiresAt = DateTime.UtcNow.AddDays(1);
		var token = new RefreshToken(userId: 1, token: "test-token", expiresAtUtc: expiresAt);

		// Act
		token.Revoke();
		var isActive = token.IsActive;

		// Assert
		isActive.Should().BeFalse();
		token.IsRevoked.Should().BeTrue();
	}
}
