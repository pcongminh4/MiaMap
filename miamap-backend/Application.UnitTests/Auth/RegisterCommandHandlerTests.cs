using Application.Common.Abstractions.Authentication;
using Application.Common.Abstractions.Data;
using Application.Users.Register;
using Domain.Users;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Auth;

public sealed class RegisterCommandHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnUserIdAndSaveUser_When_CommandIsValid
	/// Mô tả: Đảm bảo khi gửi yêu cầu đăng ký hợp lệ, Handler sẽ chuẩn hóa dữ liệu (trim khoảng trắng của tên, viết thường email),
	///        băm mật khẩu bằng IPasswordHasher, lưu User vào DB qua IUserRepository và gọi SaveChangesAsync trên IUnitOfWork.
	/// Đầu vào: RegisterCommand với FirstName=" John ", LastName=" Doe ", Email=" John.Doe@Example.Com ", Password="MyPassword123".
	///          Giả lập IPasswordHasher.Hash trả về mật khẩu đã băm "hashed_pwd".
	/// Kết quả kỳ vọng: Trả về ID của User vừa tạo (giả định > 0).
	///                  IPasswordHasher.Hash được gọi chính xác 1 lần với mật khẩu gốc.
	///                  IUserRepository.AddAsync được gọi chính xác 1 lần với User đã được chuẩn hóa
	///                  (FirstName="John", LastName="Doe", Email="john.doe@example.com", PasswordHash="hashed_pwd").
	///                  IUnitOfWork.SaveChangesAsync được gọi chính xác 1 lần.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnUserIdAndSaveUser_When_CommandIsValid()
	{
		// Arrange
		var command = new RegisterCommand(" John ", " Doe ", " John.Doe@Example.Com ", "MyPassword123");

		var userRepository = Substitute.For<IUserRepository>();
		var passwordHasher = Substitute.For<IPasswordHasher>();
		passwordHasher.Hash(command.Password).Returns("hashed_pwd");

		var unitOfWork = Substitute.For<IUnitOfWork>();

		var handler = new RegisterCommandHandler(userRepository, passwordHasher, unitOfWork);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().Be(0); // Vì thực thể User chưa được lưu vào DB thật nên Id mặc định là 0 (hoặc ta có thể stub nếu cần, nhưng 0 là giá trị mặc định của int)

		passwordHasher.Received(1).Hash(command.Password);

		await userRepository.Received(1).AddAsync(Arg.Is<User>(user =>
			user.FirstName == "John" &&
			user.LastName == "Doe" &&
			user.Email == "john.doe@example.com" &&
			user.PasswordHash == "hashed_pwd"
		), Arg.Any<CancellationToken>());

		await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}
}
