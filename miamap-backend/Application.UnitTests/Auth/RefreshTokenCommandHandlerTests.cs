using Application.Common.Abstractions.Authentication;
using Application.Common.Abstractions.Data;
using Application.Auth.Refresh;
using Domain.Users;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Auth;

public sealed class RefreshTokenCommandHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnNewTokensAndRevokeOldToken_When_RefreshTokenIsValid
	/// Mô tả: Đảm bảo khi gửi một refresh token hợp lệ và còn hạn, handler sẽ sinh ra Access Token và Refresh Token mới,
	///        thu hồi token cũ, lưu token mới vào DB và gọi SaveChanges.
	/// Đầu vào: RefreshTokenCommand chứa token cũ "old-refresh-token".
	///          Giả lập IRefreshTokenRepository.GetByTokenAsync trả về RefreshToken hợp lệ (IsActive = true).
	///          Giả lập IUserRepository.GetByIdAsync trả về User hợp lệ.
	///          Giả lập IJwtService.GenerateToken trả về Access Token mới.
	/// Kết quả kỳ vọng: Trả về RefreshTokenResult chứa access/refresh token mới.
	///                  Token cũ bị đổi trạng thái IsRevoked = true.
	///                  IRefreshTokenRepository.AddAsync được gọi để lưu token mới.
	///                  IUnitOfWork.SaveChangesAsync được gọi.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnNewTokensAndRevokeOldToken_When_RefreshTokenIsValid()
	{
		// Arrange
		var oldTokenString = "old-refresh-token";
		var command = new RefreshTokenCommand(oldTokenString);

		var userId = 123;
		var oldToken = new RefreshToken(userId, oldTokenString, DateTime.UtcNow.AddDays(1));

		var user = new User("Jane", "Doe", "jane@example.com", "hashed_password");
		var idProperty = typeof(User).GetProperty("Id");
		idProperty?.SetValue(user, userId);

		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		refreshTokenRepository.GetByTokenAsync(oldTokenString, Arg.Any<CancellationToken>()).Returns(oldToken);

		var userRepository = Substitute.For<IUserRepository>();
		userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

		var jwtService = Substitute.For<IJwtService>();
		var newJwtToken = new JwtToken("new-access-token", DateTime.UtcNow.AddMinutes(15));
		jwtService.GenerateToken(user).Returns(newJwtToken);

		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new RefreshTokenCommandHandler(
			refreshTokenRepository,
			userRepository,
			jwtService,
			unitOfWork);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.AccessToken.Should().Be("new-access-token");
		result.AccessTokenExpiresAtUtc.Should().Be(newJwtToken.ExpiresAtUtc);
		result.NewRefreshToken.Should().NotBeNullOrWhiteSpace();
		result.NewRefreshToken.Should().NotBe(oldTokenString);
		result.NewRefreshTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);

		oldToken.IsRevoked.Should().BeTrue(); // Token cũ đã bị thu hồi

		await refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
		await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ThrowValidationException_When_RefreshTokenIsExpiredOrRevoked
	/// Mô tả: Đảm bảo khi refresh token gửi lên đã bị thu hồi hoặc đã hết hạn, Handler sẽ ném ngoại lệ ValidationException và không thực hiện xử lý tiếp.
	/// Đầu vào: RefreshTokenCommand với token không còn hoạt động.
	///          Giả lập IRefreshTokenRepository.GetByTokenAsync trả về một RefreshToken có IsActive = false.
	/// Kết quả kỳ vọng: Ném ra ValidationException với PropertyName "RefreshToken" và thông báo lỗi.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ThrowValidationException_When_RefreshTokenIsExpiredOrRevoked()
	{
		// Arrange
		var tokenString = "expired-token";
		var command = new RefreshTokenCommand(tokenString);

		// Tạo token đã hết hạn
		var token = new RefreshToken(userId: 1, tokenString, DateTime.UtcNow.AddDays(-1));

		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		refreshTokenRepository.GetByTokenAsync(tokenString, Arg.Any<CancellationToken>()).Returns(token);

		var userRepository = Substitute.For<IUserRepository>();
		var jwtService = Substitute.For<IJwtService>();
		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new RefreshTokenCommandHandler(
			refreshTokenRepository,
			userRepository,
			jwtService,
			unitOfWork);

		// Act
		Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert
		var exception = await act.Should().ThrowAsync<ValidationException>();
		exception.Which.Errors.Should().ContainSingle(error =>
			error.PropertyName == "RefreshToken" && error.ErrorMessage == "Refresh token is invalid or expired.");

		await userRepository.DidNotReceiveWithAnyArgs().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
		await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ThrowValidationException_When_UserDoesNotExist
	/// Mô tả: Đảm bảo trường hợp Refresh Token hợp lệ nhưng không tìm thấy User liên kết trong DB, Handler ném ngoại lệ ValidationException.
	/// Đầu vào: RefreshTokenCommand với token hợp lệ.
	///          Giả lập IRefreshTokenRepository.GetByTokenAsync trả về RefreshToken mẫu.
	///          Giả lập IUserRepository.GetByIdAsync trả về null.
	/// Kết quả kỳ vọng: Ném ra ValidationException với PropertyName "RefreshToken" và thông báo "User not found.".
	/// </summary>
	[Fact]
	public async Task Handle_Should_ThrowValidationException_When_UserDoesNotExist()
	{
		// Arrange
		var tokenString = "valid-token";
		var command = new RefreshTokenCommand(tokenString);

		var userId = 55;
		var token = new RefreshToken(userId, tokenString, DateTime.UtcNow.AddDays(1));

		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		refreshTokenRepository.GetByTokenAsync(tokenString, Arg.Any<CancellationToken>()).Returns(token);

		var userRepository = Substitute.For<IUserRepository>();
		userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

		var jwtService = Substitute.For<IJwtService>();
		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new RefreshTokenCommandHandler(
			refreshTokenRepository,
			userRepository,
			jwtService,
			unitOfWork);

		// Act
		Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert
		var exception = await act.Should().ThrowAsync<ValidationException>();
		exception.Which.Errors.Should().ContainSingle(error =>
			error.PropertyName == "RefreshToken" && error.ErrorMessage == "User not found.");

		jwtService.DidNotReceiveWithAnyArgs().GenerateToken(Arg.Any<User>());
		await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(Arg.Any<CancellationToken>());
	}
}
