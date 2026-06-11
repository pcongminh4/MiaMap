---
name: logging-observability
description: Quy chuẩn ghi log có cấu trúc (Structured Logging) và tích hợp giám sát hệ thống (Observability). Sử dụng khi ghi log lỗi, log sự kiện nghiệp vụ hoặc xử lý middleware giám sát.
license: MIT
allowed-tools: None
---

# Structured Logging & Observability Guidelines

Tài liệu này hướng dẫn cách ghi log có cấu trúc và thiết lập hệ thống quan sát trạng thái (Observability) cho MiaMap.

## 1. Ghi log có cấu trúc (Structured Logging)
- Không cộng chuỗi (string concatenation) hoặc định dạng chuỗi nội suy (`$""`) khi viết log. Sử dụng cú pháp **Message Template** để thư viện log (như Serilog) có thể lưu các giá trị dưới dạng các trường thuộc tính (properties) riêng biệt, giúp dễ tìm kiếm và lọc trên các hệ thống ElasticSearch/Seq/Loki.
  - *Đúng*: `_logger.LogInformation("Đã tìm thấy place {PlaceId} với tên {PlaceName}", placeId, placeName);`
  - *Sai*: `_logger.LogInformation($"Đã tìm thấy place {placeId} với tên {placeName}");`

## 2. Phân loại Log Level phù hợp
Sử dụng đúng mức độ quan trọng (Log Level) của thông tin:
- **Debug**: Ghi các thông tin chi tiết kỹ thuật phục vụ quá trình phát triển (ví dụ: các bước xử lý nội bộ của thuật toán dẫn đường).
- **Information**: Ghi nhận các sự kiện nghiệp vụ quan trọng hoặc luồng chạy bình thường của hệ thống (ví dụ: "Người dùng đăng nhập thành công", "Bản đồ được cập nhật").
- **Warning**: Các trường hợp hệ thống gặp lỗi nhẹ hoặc hành vi bất thường nhưng vẫn tự hồi phục được (ví dụ: API ngoài phản hồi chậm, kết nối DB tạm thời thất bại và được thử lại).
- **Error**: Các lỗi làm hỏng luồng nghiệp vụ hiện tại nhưng không làm sập ứng dụng (ví dụ: lỗi lưu dữ liệu thất bại, ném ra Exception trong Handler). Bắt buộc phải truyền kèm Exception đối tượng vào log.
  - *Đúng*: `_logger.LogError(ex, "Lỗi xảy ra khi cập nhật tọa độ");`
- **Critical**: Các lỗi nghiêm trọng đe dọa trực tiếp đến tính hoạt động của ứng dụng (ví dụ: mất kết nối hoàn toàn Database, hết bộ nhớ RAM).

## 3. Quản lý Trace ID / Correlation ID
- Mỗi yêu cầu HTTP gửi đến hệ thống phải được đính kèm một `Trace ID` duy nhất (ASP.NET Core sử dụng `HttpContext.TraceIdentifier`).
- Khi xảy ra lỗi hoặc ngoại lệ (Exception), mã lỗi trả về cho client (RFC 7807) phải chứa trường `traceId` này. Người dùng hoặc admin có thể dùng `traceId` để tra cứu nhanh toàn bộ log nghiệp vụ liên quan trên server.
- Đối với các dịch vụ background hoặc hàng đợi (background workers), cần tự khởi tạo `Activity` hoặc `Correlation ID` mới và chuyển tiếp (propagate) nó xuyên suốt các tác vụ con.
