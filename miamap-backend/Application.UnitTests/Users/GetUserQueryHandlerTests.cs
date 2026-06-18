using Application.Common.Abstractions.Data;
using Application.Users.GetUser;
using Domain.Users;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Users;

public sealed class GetUserQueryHandlerTests
{
	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ReturnGetUserResult_When_UserExists
	/// Mô tả: Đảm bảo khi gửi GetUserQuery với ID hợp lệ và User tồn tại, Handler sẽ trả về đúng DTO chứa thông tin của User đó.
	/// Đầu vào: GetUserQuery có UserId = 1. Giả lập IUserRepository.GetByIdAsync(1) trả về đối tượng User mẫu.
	/// Kết quả kỳ vọng: Kết quả trả về không null, có Id = 1, và thông tin (FirstName, LastName, Email) khớp với đối tượng mẫu.
	/// </summary>
	[Fact]
	public async Task Handle_Should_ReturnGetUserResult_When_UserExists()
	{
		// Arrange
		var userId = 1;
		var query = new GetUserQuery(userId);

		var user = new User("John", "Doe", "john.doe@example.com", "hashedpassword");
		// Sử dụng reflection để gán giá trị Id cho entity vì Id được cấu hình tự tăng và có private setter
		var idProperty = typeof(User).GetProperty("Id");
		idProperty?.SetValue(user, userId);

		var userRepository = Substitute.For<IUserRepository>();
		userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

		var handler = new GetUserQueryHandler(userRepository);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(userId);
		result.FirstName.Should().Be("John");
		result.LastName.Should().Be("Doe");
		result.Email.Should().Be("john.doe@example.com");

		await userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// ĐẶC TẢ KIỂM THỬ (VIETNAMESE SPECIFICATION):
	/// Tên test: Handle_Should_ThrowValidationException_When_UserDoesNotExist
	/// Mô tả: Đảm bảo khi gửi GetUserQuery với ID không tồn tại trong DB, Handler sẽ ném lỗi ValidationException.
	/// Đầu vào: GetUserQuery có UserId = 999. Giả lập IUserRepository.GetByIdAsync(999) trả về null.
	/// Kết quả kỳ vọng: Ném ra ValidationException, và lỗi trả về có chứa key "UserId" và nội dung "User not found.".
	/// </summary>
	[Fact]
	public async Task Handle_Should_ThrowValidationException_When_UserDoesNotExist()
	{
		// Arrange
		var userId = 999;
		var query = new GetUserQuery(userId);

		var userRepository = Substitute.For<IUserRepository>();
		userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

		var handler = new GetUserQueryHandler(userRepository);

		// Act
		Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

		// Assert
		var exception = await act.Should().ThrowAsync<ValidationException>();
		exception.Which.Errors.Should().ContainSingle(error =>
			error.PropertyName == "UserId" && error.ErrorMessage == "User not found.");

		await userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
	}
}
