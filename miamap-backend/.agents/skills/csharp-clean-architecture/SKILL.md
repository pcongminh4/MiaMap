---
name: csharp-clean-architecture
description: Viết và review code C# theo chuẩn Clean Architecture, CQRS (MediatR), Domain-Driven Design (DDD), và FluentValidation. Sử dụng khi lập trình logic nghiệp vụ backend, tạo thực thể, handler hoặc validator.
license: MIT
allowed-tools: None
---

# C# Clean Architecture & DDD Guidelines

Tài liệu này định nghĩa các tiêu chuẩn viết code C# hiện đại và cấu trúc Clean Architecture cho dự án MiaMap.

## 1. C# Coding Style (C# 12+)
- **Primary Constructors**: Khuyến khích sử dụng cho các Class nhận Dependency qua Constructor (Dependency Injection) và các Record DTOs.
  ```csharp
  public sealed class CreatePlaceCommandHandler(IPlaceRepository placeRepository) : IRequestHandler<CreatePlaceCommand, int> { ... }
  ```
- **File-scoped Namespaces**: Giúp giảm cấp thụt lề cho code sạch hơn.
  ```csharp
  namespace Application.Places.CreatePlace;
  ```
- **Pattern Matching & Expressions**: Ưu tiên sử dụng cú pháp biểu thức (expression-bodied members) khi phương thức hoặc thuộc tính chỉ có 1 dòng xử lý.

## 2. Clean Architecture Boundaries
Dự án được chia thành 4 layer chính, chiều phụ thuộc đi từ ngoài vào trong:
- **Domain**: Chứa các Entity, Value Object, Domain Event, và Interfaces của Repository. Layer này tuyệt đối KHÔNG phụ thuộc vào bất kỳ thư viện ngoài nào (trừ thư viện toán học/địa lý như NetTopologySuite nếu cần).
- **Application**: Chứa logic luồng nghiệp vụ (CQRS Commands/Queries), MediatR Handlers, Validators, và Interfaces bên thứ 3. Chỉ phụ thuộc vào Domain.
- **Infrastructure**: Triển khai các Database Context (EF Core), cấu hình thực thể (Configurations), kết nối API ngoài, gửi mail, lưu cache. Phụ thuộc vào Application và Domain.
- **Api**: Điểm đầu vào (Minimal APIs Endpoints, Controllers), Middleware, Dependency Injection Setup. Phụ thuộc vào Application và Infrastructure.

## 3. Domain-Driven Design (DDD) Entities
- **Đóng gói trạng thái**: Mọi thuộc tính của Entity nên sử dụng `private set` để tránh việc thay đổi trạng thái tự do ngoài Domain.
- **Hành vi giàu ngữ nghĩa (Rich Domain Model)**: Cập nhật trạng thái thông qua các hàm có tên phản ánh nghiệp vụ thực tế (ví dụ: `UpdateLocation(...)`, `SetActive(bool)`), không dùng trực tiếp setter.
- **Constructors**:
  - Tạo constructor đầy đủ tham số để khởi tạo thực thể hợp lệ kèm theo kiểm tra dữ liệu đầu vào (Invariants).
  - Định nghĩa một constructor không tham số với tầm vực `private` hoặc `protected` để EF Core có thể map dữ liệu từ DB lên.
- **Mẫu Factory**: Sử dụng các phương thức `public static Entity Create(...)` để tạo mới thực thể có logic phức tạp.

## 4. CQRS với MediatR & FluentValidation
- Mỗi Request nghiệp vụ phải đi kèm một Command/Query và một Handler tương ứng.
- Command/Query nên là một `public sealed record`.
- Sử dụng `FluentValidation` để tự động kiểm tra tính hợp lệ của Request trước khi chuyển vào Handler thông qua MediatR Pipeline:
  ```csharp
  public sealed class CreatePlaceCommandValidator : AbstractValidator<CreatePlaceCommand>
  {
      public CreatePlaceCommandValidator()
      {
          RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
      }
  }
  ```
