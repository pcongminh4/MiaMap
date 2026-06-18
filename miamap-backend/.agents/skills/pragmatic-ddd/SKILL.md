---
name: pragmatic-ddd
description: Hướng dẫn thiết kế và lập trình theo mô hình Pragmatic Domain-Driven Design (DDD) thực tế cho dự án MiaMap. Giúp đơn giản hóa các thực thể, quản lý Aggregate qua Aggregate Root, loại bỏ ám ảnh kiểu nguyên thủy bằng Value Objects chọn lọc, và kiểm soát nghiệp vụ tập trung ở Domain mà không bị over-engineering.
license: MIT
allowed-tools: None
---

# Pragmatic Domain-Driven Design (DDD) Guidelines

Tài liệu này định nghĩa cách tiếp cận thực tế, tinh gọn đối với DDD (Pragmatic DDD) trong dự án MiaMap. Hướng tiếp cận này giúp dự án vừa giữ được tính chất nghiệp vụ tập trung, mã nguồn sạch sẽ dễ bảo trì, vừa tránh bị quá phức tạp hóa (over-engineering) cho nhà phát triển độc lập (solo developer).

## 1. Thiết kế Thực thể giàu nghiệp vụ (Rich Domain Model)
*   **Encapsulation (Đóng gói tuyệt đối)**: Tất cả thuộc tính của Entity phải dùng `private set`.
*   **Ubiquitous Language (Ngôn ngữ nghiệp vụ đồng nhất)**:
    *   Tránh các phương thức cập nhật dữ liệu chung chung như `SetX()`, `UpdateProperties()`.
    *   Sử dụng các phương thức có ý nghĩa nghiệp vụ thực tế như `Deactivate()`, `UpdateVotes()`, `LinkNearestNode()`.
*   **Invariants (Tính toàn vẹn của dữ liệu)**:
    *   Khởi tạo Entity qua constructor phải đảm bảo trạng thái ban đầu luôn hợp lệ.
    *   Cung cấp một constructor không tham số tầm vực `private` hoặc `protected` phục vụ riêng cho ORM (EF Core).

## 2. Xác định và quản lý Aggregate Boundaries
*   **Aggregate Root**: Là điểm truy cập duy nhất vào một nhóm các thực thể liên quan (Aggregate). Client không được tác động trực tiếp vào các thực thể con bên trong Aggregate.
    *   *Ví dụ*: `Report` là Aggregate Root của `ReportVote`.
*   **Quy tắc Repository**: Chỉ định nghĩa Repository cho Aggregate Root.
    *   *Đúng*: `IReportRepository` quản lý cả `Report` và `ReportVote`.
    *   *Sai*: Tạo `IReportVoteRepository` riêng biệt.
*   **Liên kết giữa các Aggregate**: Các Aggregate khác nhau chỉ được liên kết với nhau bằng ID kiểu nguyên thủy (ví dụ: `Report` lưu `CreatedByUserId` dạng `int` thay vì tham chiếu thực thể `User`).

## 3. Sử dụng Value Objects chọn lọc (Tránh Primitive Obsession)
*   Chỉ tạo Value Object cho các thuộc tính mang tính chất cấu trúc dữ liệu hoặc có logic validate đặc thù:
    *   `Email`: Tự validate định dạng.
    *   `PasswordHash`: Chứa logic mã hóa và kiểm tra.
    *   `GeoPoint` hoặc `Coordinates`: Chứa vĩ độ và kinh độ kèm validate giới hạn bản đồ.
*   Value Object bắt buộc phải **bất biến (Immutable)**. Trong C#, sử dụng kiểu `readonly record struct` hoặc `sealed record`.
*   Tận dụng EF Core `Value Converter` hoặc `OwnsOne` để lưu trữ Value Object xuống database một cách gọn gàng.

## 4. Xử lý Lỗi nghiệp vụ (Result / Error Pattern)
*   **Không dùng Exception để điều khiển luồng**: Việc ném exception (`throw new Exception()`) cho các lỗi nghiệp vụ thông thường (như Sai mật khẩu, Email đã tồn tại) làm giảm hiệu năng và ẩn đi các kịch bản lỗi của hệ thống.
*   **Định nghĩa Lỗi tường minh**: Tạo các lớp lỗi nghiệp vụ như `UserError`, `ReportError` dưới dạng các hằng số hoặc phương thức tĩnh tĩnh.
*   **Trả về Result Object**: Sử dụng kiểu trả về dạng `Result` hoặc `Result<T>` ở tầng Application để chuyển tải trạng thái Thành công / Thất bại cùng mã lỗi tương ứng về tầng API.

## 5. Mẹo giữ hệ thống đơn giản khi phát triển độc lập (Developer Productivity)
*   **Yêu cầu tối giản (YAGNI - You Aren't Gonna Need It)**: Không viết các cấu trúc đa hình phức tạp khi chỉ có 1 trường hợp sử dụng duy nhất.
*   **Tận dụng MediatR Pipeline**: Đẩy toàn bộ các nghiệp vụ chung (như Validation, Logging, UnitOfWork SaveChanges) vào các Pipeline Behaviors để giảm bớt code trùng lặp ở Handler.
*   **Tài liệu hóa bằng Unit Tests**: Mỗi khi thay đổi hoặc thêm nghiệp vụ trong Domain, phải viết các test cases tương ứng trong dự án `Application.UnitTests` và ghi chú đặc tả bằng tiếng Việt rõ ràng để phục vụ như một tài liệu sống (living documentation).
