---
name: dotnet-test-guidelines
description: Quy chuẩn viết, tổ chức và chạy các bài kiểm thử tự động (Unit Test, Integration Test). Sử dụng khi được yêu cầu viết kiểm thử, sửa các bài test lỗi hoặc cải thiện độ bao phủ (coverage).
license: MIT
allowed-tools: Bash
---

# .NET Testing Guidelines

Tài liệu hướng dẫn Agent phát triển các bài kiểm thử (tests) đồng bộ, dễ đọc và đáng tin cậy.

## 1. Công nghệ sử dụng
- **Test Framework**: `xUnit`
- **Assertion**: `FluentAssertions` (cú pháp trực quan dạng `result.Should().BeEquivalentTo(...)`)
- **Mocking**: `NSubstitute` (để giả lập hành vi các service/repository phụ thuộc)

## 2. Cấu trúc và Đặt tên bài test
- **Tên lớp Test**: Trùng với lớp cần test và kết thúc bằng hậu tố `Tests` (ví dụ: `CreatePlaceCommandHandlerTests`).
- **Tên phương thức Test**: Tuân theo mẫu `Given_When_Then` để nêu rõ ngữ cảnh, hành động và kết quả kỳ vọng.
  - Ví dụ: `Handle_Should_CreatePlace_When_CommandIsValid`
  - Ví dụ: `Handle_Should_ThrowValidationException_When_NameIsEmpty`

## 3. Cú pháp viết Test (AAA Pattern)
Mỗi bài test cần có 3 phần rõ ràng: Arrange (Thiết lập), Act (Hành động), Assert (Kiểm tra).

```csharp
[Fact]
public async Task Handle_Should_ReturnPlaceId_When_CommandIsValid()
{
    // Arrange
    var placeRepository = Substitute.For<IPlaceRepository>();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    var handler = new CreatePlaceCommandHandler(placeRepository, unitOfWork);
    var command = new CreatePlaceCommand("Hồ Gươm", "Sightseeing", 21.0285, 105.8521, 4.8, 120, "Hà Nội");

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeGreaterThan(0);
    await placeRepository.Received(1).AddAsync(Arg.Any<Place>(), Arg.Any<CancellationToken>());
    await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
}
```

## 4. Thực thi kiểm thử
Dùng lệnh sau để chạy toàn bộ test trong dự án:
```bash
dotnet test
```
