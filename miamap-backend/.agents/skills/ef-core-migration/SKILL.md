---
name: ef-core-migration
description: Hướng dẫn quản lý, kiểm tra và chạy Entity Framework Core Migrations. Sử dụng khi thêm/sửa/xóa bảng, trường dữ liệu hoặc thay đổi cấu hình EF Core.
license: MIT
allowed-tools: Bash
---

# Entity Framework Core Migrations Guidelines

Hướng dẫn cập nhật database và tạo migrations an toàn, tránh mất mát dữ liệu trên môi trường phát triển và sản phẩm.

## 1. Các lệnh CLI chính (Thực thi tại thư mục gốc backend)

Khi làm việc với Entity Framework Core, các dự án dùng cấu trúc dự án riêng biệt cho Context (Infrastructure) và Startup (Api). Cần chỉ định tham số `-p` (project) và `-s` (startup-project) rõ ràng:

- **Tạo một Migration mới**:
  ```bash
  dotnet ef migrations add <Tên_Migration> -p Infrastructure -s Api -o Database/Migrations
  ```
- **Cập nhật database lên bản mới nhất**:
  ```bash
  dotnet ef database update -p Infrastructure -s Api
  ```
- **Xóa Migration cuối cùng (chưa được apply hoặc đã rollback)**:
  ```bash
  dotnet ef migrations remove -p Infrastructure -s Api
  ```
- **Tạo file SQL Script từ các thay đổi**:
  ```bash
  dotnet ef migrations script -p Infrastructure -s Api -o migration.sql
  ```

## 2. Quy trình kiểm tra an toàn
1. **Kiểm tra File Generated**: Sau khi chạy lệnh `migrations add`, Agent phải xem xét nội dung file `.cs` vừa được sinh ra trong thư mục `Database/Migrations`.
2. **Tránh mất dữ liệu**: Đặc biệt lưu ý nếu file migration có chứa lệnh `DropColumn` hoặc `DropTable`. Nếu có, hãy hỏi lại người dùng hoặc tìm giải pháp bảo toàn dữ liệu (như rename hoặc chạy script backup trước).
3. **Rollback Test**: Hãy chạy thử nghiệm cập nhật và hạ cấp (Up/Down) trên DB local trước khi commit code để đảm bảo code rollback hoạt động đúng.
