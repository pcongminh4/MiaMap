---
name: planning-and-documentation
description: Quy chuẩn viết Kế hoạch phát triển (Implementation Plan), tài liệu kiến trúc, mô tả tính năng và hướng dẫn viết tệp Markdown (.md) chất lượng cao. Sử dụng khi lập kế hoạch, viết README hoặc tài liệu kỹ thuật.
license: MIT
allowed-tools: None
---

# Planning & Documentation Guidelines

Tài liệu này quy chuẩn hóa cách thức viết các tài liệu kế hoạch phát triển (Roadmaps/Plans), theo dõi công việc (Tasks), tài liệu kỹ thuật và định dạng Markdown (`.md`) trong dự án MiaMap.

## 1. Định dạng Markdown (.md) chuẩn
- **Clickable File Links**: Bắt buộc tạo các liên kết nhấp chuột được (clickable links) tới các tệp mã nguồn liên quan trong tài liệu sử dụng lược đồ `file://` với đường dẫn tuyệt đối (trên Windows dùng dấu gạch chéo `/`). Không bọc liên kết trong dấu backticks `` ` ``.
  - *Đúng*: `Xem cấu hình tại [UserConfiguration.cs](file:///c:/github-projects/MiaMap/miamap-backend/Infrastructure/Database/Configurations/UserConfiguration.cs).`
  - *Sai*: `Xem cấu hình tại [``UserConfiguration.cs``](file:///c:/...).`
- **Sử dụng GitHub-style Alerts**: Sử dụng strategic alert blocks để nhấn mạnh thông tin quan trọng:
  - `> [!NOTE]`: Thông tin bổ sung hoặc ngữ cảnh phụ.
  - `> [!TIP]`: Mẹo tối ưu hóa, gợi ý viết code thông minh.
  - `> [!IMPORTANT]`: Thông tin bắt buộc phải biết hoặc quyết định quan trọng.
  - `> [!WARNING]`: Cảnh báo rủi ro cao hoặc thay đổi lớn (breaking changes).
- **Mermaid Diagrams**: Khi mô tả các luồng dữ liệu, vòng đời thực thể hoặc sơ đồ kiến trúc, bắt buộc vẽ biểu đồ Mermaid trực quan:
  ```mermaid
  graph TD
      A[Client Request] --> B[MediatR Command]
      B --> C[FluentValidation Check]
      C --> D[CommandHandler Execution]
  ```

## 2. Quy chuẩn viết Kế hoạch Phát triển (Implementation Plan)
Mỗi khi bắt đầu một tính năng mới hoặc thay đổi kiến trúc lớn, tài liệu `implementation_plan.md` cần tuân thủ cấu trúc sau:
1. **Goal Description**: Mô tả ngắn gọn vấn đề cần giải quyết và mục tiêu đạt được.
2. **User Review Required**: Nêu bật các quyết định cần người dùng duyệt trước khi làm (dùng Alert WARNING/IMPORTANT).
3. **Open Questions**: Các câu hỏi chưa rõ ràng cần làm rõ.
4. **Proposed Changes**: Liệt kê các tệp sẽ được thêm mới, sửa đổi hoặc xóa, gom nhóm theo phân hệ (Backend/Frontend) kèm link file.
5. **Verification Plan**: Cách kiểm thử tự động (lệnh chạy test) và kiểm thử thủ công để xác nhận kết quả.

## 3. Quản lý Tác vụ (Task Tracking)
Tập tin theo dõi công việc `task.md` phải sử dụng các hộp kiểm để phân định rõ ràng trạng thái:
- `[ ]` Tác vụ chưa thực hiện.
- `[/]` Tác vụ đang được triển khai (in progress).
- `[x]` Tác vụ đã hoàn thành.

## 4. Tài liệu Tổng kết (Walkthrough)
Tập tin `walkthrough.md` được tạo sau khi hoàn thành công việc để bàn giao cho người dùng:
- Liệt kê ngắn gọn các thay đổi thực tế đã thực hiện.
- Minh họa bằng hình ảnh/video clip nếu có thay đổi giao diện (UI) hoặc luồng trải nghiệm.
- Kết quả chạy kiểm thử thành công (output log, số lỗi/cảnh báo).
