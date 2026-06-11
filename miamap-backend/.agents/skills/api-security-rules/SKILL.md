---
name: api-security-rules
description: Các nguyên tắc và luật bảo mật API, xác thực (Authentication), phân quyền (Authorization) và kiểm tra dữ liệu đầu vào. Sử dụng khi viết Controller, Minimal API Endpoints hoặc cấu hình CORS/Middleware.
license: MIT
allowed-tools: None
---

# API Security & Authorization Guidelines

Tài liệu này quy chuẩn các nguyên tắc thiết kế bảo mật cho API, xác thực người dùng và phân quyền trong hệ thống MiaMap.

## 1. Xác thực (Authentication) & Quản lý Token
- **JWT (JSON Web Token)**: Sử dụng JWT cho xác thực không lưu trạng thái (stateless authentication).
- Token phải được truyền qua HTTP Header `Authorization: Bearer <token>`.
- Khóa bí mật (JWT Secret) tuyệt đối không được ghi cứng (hardcode) trong mã nguồn. Bắt buộc phải cấu hình thông qua Environment Variables hoặc User Secrets/Key Vault.
- Thiết lập thời gian sống (expiration time) của Access Token ngắn (ví dụ: 1 giờ) và sử dụng cơ chế Refresh Token nếu cần thiết lập phiên đăng nhập lâu dài.

## 2. Phân quyền (Authorization)
- Áp dụng nguyên tắc **Quyền hạn tối thiểu (Least Privilege)**: Mặc định mọi Endpoint mới đều phải yêu cầu xác thực (`RequireAuthorization()`), ngoại trừ các API công khai được chỉ định rõ ràng bằng `AllowAnonymous()`.
- Sử dụng phân quyền dựa trên Vai trò (Role-based) hoặc Quyền hạn cụ thể (Policy/Permission-based):
  ```csharp
  app.MapPost("/places", CreatePlace)
      .RequireAuthorization("AdminOnly"); // Yêu cầu chính sách bảo mật cụ thể
  ```

## 3. Kiểm soát CORS (Cross-Origin Resource Sharing)
- Cấu hình CORS chặt chẽ: Không sử dụng `.AllowAnyOrigin()` ở môi trường Production.
- Chỉ cho phép các domain frontend tin cậy được khai báo cụ thể trong cấu hình hệ thống kết nối tới API.

## 4. Kiểm định và Làm sạch dữ liệu đầu vào (Input Validation & Sanitization)
- Mọi dữ liệu do người dùng gửi lên từ client (Request Body, Query Parameters, Route Parameters) đều là không tin cậy.
- **FluentValidation**: Bắt buộc viết Validator cho mọi Command/Query thay đổi trạng thái của hệ thống.
- **SQL Injection**: Luôn viết truy vấn thông qua EF Core LINQ (tự động tham số hóa tham số) hoặc sử dụng Parameterized Query khi viết SQL thuần. Tuyệt đối không cộng chuỗi SQL.
- **XSS (Cross-Site Scripting)**: Làm sạch dữ liệu văn bản đầu vào trước khi lưu trữ vào Database hoặc trả về cho Client, đặc biệt đối với các trường văn bản hiển thị công khai trên bản đồ.
