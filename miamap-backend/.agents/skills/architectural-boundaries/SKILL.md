---
name: architectural-boundaries
description: Nguyên tắc và luật phân ranh giới các Module nghiệp vụ (Bounded Contexts) để giữ hệ thống lỏng (loose coupling), chuẩn bị cho việc chia tách thành Microservices sau này. Sử dụng khi thêm tính năng mới, kết nối giữa các Domain.
license: MIT
allowed-tools: None
---

# Modular Architectural Boundaries Guidelines

Tài liệu này định nghĩa các quy tắc thiết kế kiến trúc dạng Mô-đun hóa (Modular Monolith) cho hệ thống MiaMap. Việc tuân thủ chặt chẽ các ranh giới này giúp mã nguồn dễ bảo trì và có thể tách thành các Microservices độc lập trong tương lai mà không cần refactor lớn.

## 1. Phân ranh giới Bounded Contexts
- Dự án được chia thành các Domain độc lập logic (ví dụ: `Users` / `Auth`, `Places`).
- Mỗi Domain phải tự quản lý thực thể (Entities), Logic nghiệp vụ, Repository, và Database Tables của riêng mình.
- **Quy tắc tuyệt đối**: Entity của Domain này **KHÔNG** được tham chiếu trực tiếp (Navigation Property) đến Entity của Domain khác. Chỉ được liên kết thông qua khóa ngoại kiểu dữ liệu cơ bản (ví dụ: `UserId` kiểu `Guid` hoặc `int`), không dùng đối tượng lớp Entity.
  - *Đúng*:
    ```csharp
    public class Place {
        public int Id { get; set; }
        public Guid CreatedByUserId { get; set; } // Liên kết qua ID thuần túy
    }
    ```
  - *Sai*:
    ```csharp
    public class Place {
        public int Id { get; set; }
        public User CreatedByUser { get; set; } // KHÔNG tham chiếu trực tiếp thực thể User
    }
    ```

## 2. Giao tiếp giữa các Mô-đun (Cross-Module Communication)
- Các Mô-đun nghiệp vụ không được gọi trực tiếp Repository, Service hoặc Entity Handler của nhau.
- **Truy vấn đồng bộ (Synchronous Queries)**: Nếu Mô-đun A cần thông tin từ Mô-đun B, nó phải sử dụng `MediatR` để gửi một Request Query sang Mô-đun B. Việc này đảm bảo điểm giao tiếp duy nhất là qua các DTO Contract được định nghĩa rõ ràng.
- **Xử lý bất đồng bộ (Asynchronous Events)**: Khuyến khích sử dụng Domain Events hoặc **Integration Events** để truyền tin bất đồng bộ khi có thay đổi trạng thái (ví dụ: khi User bị xóa, phát ra `UserDeletedIntegrationEvent` để Mô-đun Places tự dọn dẹp các địa điểm liên quan).

## 3. Tách biệt Cơ sở dữ liệu (Database Decoupling)
- Mặc dù hiện tại hệ thống có thể chạy chung 1 Database vật lý, nhưng cấu trúc dữ liệu phải sẵn sàng cho việc tách Database (Database-per-service).
- Sử dụng **Schema DB** riêng biệt cho từng mô-đun (ví dụ: schema `users` cho dữ liệu User/Auth, schema `places` cho dữ liệu địa điểm).
- Tránh thực hiện các câu lệnh SQL JOIN chéo giữa các bảng thuộc các schema khác nhau. Nếu cần dữ liệu kết hợp, hãy thực hiện truy vấn riêng biệt ở tầng Application hoặc tổng hợp dữ liệu bất đồng bộ.

## 4. Kiểm soát Shared Kernel (Thư viện dùng chung)
- Hạn chế tối đa việc đưa logic nghiệp vụ vào các thư viện dùng chung (Shared/Common project).
- Dự án Shared chỉ nên chứa các kiểu dữ liệu cơ bản, các helper thuần túy (không chứa trạng thái), các interface chung không thay đổi (như `IEndpoint`, `IResult`) hoặc định nghĩa định dạng API.
- Nếu một logic nghiệp vụ bị lặp lại ở 2 mô-đun, hãy cân nhắc sao chép nó (duplication tốt hơn là dependency sai mục đích) hoặc thiết kế lại ranh giới nghiệp vụ của các mô-đun.
