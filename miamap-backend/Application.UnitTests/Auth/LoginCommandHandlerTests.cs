using Api.Endpoint.Auth;
using Application.Common.Abstractions.Authentication;
using Application.Common.Abstractions.Data;
using Application.Users.Login;
using Domain.Users;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Auth;

public sealed class LoginCommandHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnLoginResultAndSaveRefreshToken_When_CredentialsAreValid
	/// Mô tả: Đảm bảo khi người dùng gửi email và mật khẩu chính xác, handler sẽ xác thực thành công,
	///        tạo mới một Refresh Token, lưu vào DB, gọi SaveChanges của UnitOfWork và trả về kết quả đăng nhập.
	/// Đầu vào: LoginCommand với email "test@example.com", mật khẩu "Password123!".
	///          Giả lập IUserRepository.GetByEmailAsync trả về User mẫu.
	///          Giả lập IPasswordHasher.Verify trả về true (mật khẩu khớp).
	///          Giả lập IJwtService.GenerateToken trả về Access Token mẫu.
	/// Kết quả kỳ vọng: Trả về LoginResult có đầy đủ thông tin,
	///                  IRefreshTokenRepository.AddAsync được gọi chính xác 1 lần,
	///                  IUnitOfWork.SaveChangesAsync được gọi chính xác 1 lần.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnLoginResultAndSaveRefreshToken_When_CredentialsAreValid()
	{
		// Arrange
		var command = new LoginCommand("test@example.com", "Password123!");

		var user = new User("John", "Doe", "test@example.com", "hashed_password");
		var userId = 42;
		var idProperty = typeof(User).GetProperty("Id");
		idProperty?.SetValue(user, userId);

		var userRepository = Substitute.For<IUserRepository>();
		userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);

		var passwordHasher = Substitute.For<IPasswordHasher>();
		passwordHasher.Verify(command.Password, user.PasswordHash).Returns(true);

		var jwtService = Substitute.For<IJwtService>();
		var jwtToken = new JwtToken("access-token-string", DateTime.UtcNow.AddMinutes(15));
		jwtService.GenerateToken(user).Returns(jwtToken);

		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new LoginCommandHandler(
			userRepository,
			refreshTokenRepository,
			passwordHasher,
			jwtService,
			unitOfWork);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.UserId.Should().Be(userId);
		result.AccessToken.Should().Be("access-token-string");
		result.ExpiresAtUtc.Should().Be(jwtToken.ExpiresAtUtc);
		result.RefreshToken.Should().NotBeNullOrWhiteSpace();
		result.RefreshTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);

		await refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
		await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ThrowValidationException_When_EmailDoesNotExist
	/// Mô tả: Đảm bảo khi gửi email đăng nhập không tồn tại trong DB, Handler sẽ ném lỗi ValidationException.
	/// Đầu vào: LoginCommand với email không tồn tại. Giả lập IUserRepository.GetByEmailAsync trả về null.
	/// Kết quả kỳ vọng: Ném ra ValidationException, lỗi chứa Property Name "Password" và nội dung báo lỗi thông tin không hợp lệ.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ThrowValidationException_When_EmailDoesNotExist()
	{
		// Arrange
		var command = new LoginCommand("notfound@example.com", "Password123!");

		var userRepository = Substitute.For<IUserRepository>();
		userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var passwordHasher = Substitute.For<IPasswordHasher>();
		var jwtService = Substitute.For<IJwtService>();
		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new LoginCommandHandler(
			userRepository,
			refreshTokenRepository,
			passwordHasher,
			jwtService,
			unitOfWork);

		// Act
		Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert
		var exception = await act.Should().ThrowAsync<ValidationException>();
		exception.Which.Errors.Should().ContainSingle(error =>
			error.PropertyName == "Password" && error.ErrorMessage == UserError.InvalidCredentials.Description);

		await refreshTokenRepository.DidNotReceiveWithAnyArgs().AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
		await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ThrowValidationException_When_PasswordIsIncorrect
	/// Mô tả: Đảm bảo khi email đúng nhưng mật khẩu gửi lên bị sai, Handler sẽ từ chối đăng nhập và ném lỗi ValidationException.
	/// Đầu vào: LoginCommand với mật khẩu không đúng.
	///          Giả lập IUserRepository.GetByEmailAsync trả về User mẫu.
	///          Giả lập IPasswordHasher.Verify trả về false.
	/// Kết quả kỳ vọng: Ném ra ValidationException, lỗi chứa Property Name "Password" và nội dung báo lỗi thông tin không hợp lệ.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ThrowValidationException_When_PasswordIsIncorrect()
	{
		// Arrange
		var command = new LoginCommand("test@example.com", "WrongPassword!");

		var user = new User("John", "Doe", "test@example.com", "hashed_password");

		var userRepository = Substitute.For<IUserRepository>();
		userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);

		var passwordHasher = Substitute.For<IPasswordHasher>();
		passwordHasher.Verify(command.Password, user.PasswordHash).Returns(false);

		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var jwtService = Substitute.For<IJwtService>();
		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new LoginCommandHandler(
			userRepository,
			refreshTokenRepository,
			passwordHasher,
			jwtService,
			unitOfWork);

		// Act
		Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

		// Assert
		var exception = await act.Should().ThrowAsync<ValidationException>();
		exception.Which.Errors.Should().ContainSingle(error =>
			error.PropertyName == "Password" && error.ErrorMessage == UserError.InvalidCredentials.Description);

		await refreshTokenRepository.DidNotReceiveWithAnyArgs().AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
		await unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(Arg.Any<CancellationToken>());
	}
}
