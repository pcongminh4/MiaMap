---
name: api-documentation
description: Tiêu chuẩn thiết kế và tài liệu hóa Minimal API của backend bằng Swagger/OpenAPI. Sử dụng khi thêm mới hoặc chỉnh sửa các API Endpoints.
license: MIT
allowed-tools: None
---

# API Documentation & Design Guidelines

Quy chuẩn thiết kế RESTful API và mô tả dữ liệu API trên Swagger để frontend dễ dàng tích hợp.

## 1. Thiết kế REST Endpoints
- **URI Formats**: Sử dụng danh từ số nhiều, chữ thường, phân tách bằng dấu gạch ngang (kebab-case).
  - Đúng: `/places`, `/places/search-nearby`
  - Sai: `/GetPlaces`, `/places/searchNearby`
- **HTTP Methods**: Sử dụng đúng vai trò của HTTP Verbs:
  - `GET`: Lấy dữ liệu (không làm thay đổi tài nguyên).
  - `POST`: Tạo mới tài nguyên.
  - `PUT`: Cập nhật toàn bộ tài nguyên.
  - `PATCH`: Cập nhật một phần tài nguyên.
  - `DELETE`: Xóa tài nguyên.

## 2. Siêu dữ liệu cho Minimal API (Swagger Integration)
Tất cả các Endpoint khi khai báo trong `MapEndpoint(...)` cần phải khai báo siêu dữ liệu đầy đủ bằng cách sử dụng các Fluent API:

- `.WithTags(Tags.Places)`: Đưa endpoint vào đúng nhóm trên UI Swagger.
- `.WithName("CreatePlace")`: Gán ID định danh duy nhất cho endpoint.
- `.WithSummary("...")`: Tiêu đề ngắn gọn của API.
- `.WithDescription("...")`: Mô tả chi tiết hành vi của API.
- `.Produces<TResponse>(StatusCodes.Status201Created)`: Khai báo kiểu dữ liệu trả về khi thành công.
- `.ProducesProblem(StatusCodes.Status400BadRequest)`: Khai báo lỗi nghiệp vụ hoặc dữ liệu không hợp lệ.

Ví dụ chuẩn:
```csharp
app.MapPost("/places", async (CreatePlaceCommand request, ISender sender) =>
    {
        var placeId = await sender.Send(request);
        return Results.Created($"/places/{placeId}", placeId);
    })
    .WithTags(Tags.Places)
    .WithName("CreatePlace")
    .WithSummary("Creates a place")
    .WithDescription("Creates a new place for location search.")
    .Produces<int>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError);
```
