---
name: api-format-specification
description: Định nghĩa chuẩn định dạng dữ liệu API (Request/Response/Error) thống nhất giữa Frontend (Next.js) và Backend (.NET Minimal APIs). Sử dụng khi thiết kế API, viết Handler hoặc viết hàm gọi API phía Frontend.
license: MIT
allowed-tools: None
---

# API Format Specification & Guidelines

Tài liệu này định nghĩa cấu trúc dữ liệu trao đổi qua API giữa frontend và backend nhằm đảm bảo tính đồng nhất, dễ mở rộng và dễ xử lý lỗi.

## 1. Định dạng phản hồi khi Thành công (Success Responses)
- **HTTP Status Code**: Sử dụng các mã thành công tiêu chuẩn (200 OK, 201 Created, 204 No Content).
- **Direct Payload Structure**: Không bọc dữ liệu trong các đối tượng dạng `{ success: true, data: ... }`. Dữ liệu DTO được trả về trực tiếp ở cấp cao nhất của response body.
  - *Ví dụ*: Đối với API lấy danh sách, trả về trực tiếp mảng JSON `[ { "id": 1, ... } ]` hoặc đối tượng phân trang dạng `{ "items": [], "totalCount": 0 }`.
- **Property Casing**: Mọi key trong JSON trả về bắt buộc phải dùng **camelCase** (ví dụ: `placeId`, `latitude`). ASP.NET Core sẽ tự động serialize sang camelCase theo cấu hình mặc định.

## 2. Định dạng phản hồi khi Thất bại (Error Responses - RFC 7807)
Mọi lỗi từ backend (Validation, Business, System) đều phải được trả về theo định dạng chuẩn **RFC 7807 (Problem Details)**:

### 2.1 Cấu trúc cơ bản của một lỗi:
- `status` (int): HTTP Status Code tương ứng (ví dụ: 400, 401, 403, 404, 500).
- `title` (string): Tên ngắn gọn của loại lỗi (ví dụ: "Validation Error", "Internal Server Error").
- `type` (string): URI chỉ định loại lỗi.
- `detail` (string, optional): Chi tiết cụ thể về lỗi để người dùng hoặc lập trình viên hiểu được.
- `code` (string, optional): Mã lỗi nghiệp vụ cụ thể của hệ thống (ví dụ: `Users.InvalidCredentials`, `Places.NotFound`).
- `errors` (object, optional): Chứa danh sách các lỗi kiểm định dữ liệu (validation). Khóa là tên thuộc tính lỗi (dạng **camelCase** ở frontend) và giá trị là mảng các tin nhắn lỗi.

### 2.2 Ví dụ lỗi nghiệp vụ (Business Error):
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The provided credentials are invalid.",
  "code": "Users.InvalidCredentials"
}
```

### 2.3 Ví dụ lỗi kiểm định (Validation Error):
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "email": [
      "The Email field is required.",
      "The Email field must be a valid email address."
    ],
    "password": [
      "The Password field is required."
    ]
  }
}
```

## 3. Quy chuẩn đồng nhất ở Frontend (Next.js)
Frontend sử dụng `Axios` interceptor để chuẩn hóa tất cả các phản hồi lỗi từ Backend thành một cấu trúc chung `NormalizedError`:
- Khi nhận lỗi, frontend tự động ánh xạ cấu trúc `errors` hoặc `detail` từ RFC 7807 của backend sang trường `details` của `NormalizedError` để hiển thị trên UI.
- Mọi field name trong lỗi trả về của backend khi đưa lên UI cần được chuyển sang **camelCase** để phù hợp với định dạng JavaScript.
