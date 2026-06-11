---
name: domain-events
description: Hướng dẫn thiết kế và sử dụng Sự kiện Nghiệp vụ (Domain Events) trong kiến trúc Domain-Driven Design (DDD). Sử dụng khi phát sinh side-effect nghiệp vụ, cần đồng bộ hóa dữ liệu bất đồng bộ giữa các thực thể hoặc các mô-đun.
allowed-tools: None
---

# Domain Events Design Guidelines

Tài liệu này định nghĩa cấu trúc, cách khai báo, phát sinh và xuất bản (publish) các sự kiện nghiệp vụ (Domain Events) trong hệ thống MiaMap.

## 1. Định nghĩa Domain Event
- Domain Event đại diện cho **một sự kiện nghiệp vụ đã xảy ra trong quá khứ** của hệ thống (ví dụ: `UserRegisteredDomainEvent`, `PlaceCreatedDomainEvent`).
- **Quy chuẩn đặt tên**: Tên của Domain Event bắt buộc phải ở **thì quá khứ** và kết thúc bằng hậu tố `DomainEvent`.
- Domain Event phải là các `record` bất biến (immutable) và kế thừa từ giao diện `IDomainEvent` (định nghĩa nội bộ kế thừa từ `INotification` của MediatR).
  ```csharp
  public sealed record UserRegisteredDomainEvent(
      int UserId,
      string Email) : IDomainEvent;
  ```

## 2. Lưu trữ sự kiện trong Entity
- Thực thể (Entity) là nơi duy nhất có thẩm quyền phát sinh Domain Event.
- Các thực thể trong Domain muốn sử dụng sự kiện nghiệp vụ cần kế thừa từ lớp trừu tượng `Entity` (lớp cơ sở quản lý danh sách sự kiện).
- Không tự ý gửi trực tiếp sự kiện ra ngoài từ Entity. Sự kiện phải được tích trữ vào danh sách sự kiện cục bộ thông qua hàm `RaiseDomainEvent`.
  ```csharp
  public abstract class Entity
  {
      private readonly List<IDomainEvent> _domainEvents = [];
      public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
      public void ClearDomainEvents() => _domainEvents.Clear();
      protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
  }
  ```

## 3. Tự động xuất bản sự kiện (Publishing Events)
- Để đảm bảo tính toàn vẹn dữ liệu (transaction integrity), các Domain Events chỉ được gửi đi **sau khi giao dịch lưu cơ sở dữ liệu đã thành công** hoặc **trong quá trình lưu cơ sở dữ liệu (Outbox Pattern)**.
- Ghi đè phương thức `SaveChangesAsync` trong `ApplicationDbContext` để tự động quét toàn bộ các thực thể đang được theo dõi có chứa sự kiện, gửi chúng qua `IPublisher` của MediatR và dọn dẹp danh sách sự kiện sau khi hoàn tất.
  ```csharp
  // Ví dụ quét và publish trong SaveChangesAsync
  var domainEntities = ChangeTracker
      .Entries<Entity>()
      .Where(x => x.Entity.GetDomainEvents().Any())
      .Select(x => x.Entity)
      .ToList();

  var domainEvents = domainEntities
      .SelectMany(x => x.GetDomainEvents())
      .ToList();

  domainEntities.ForEach(x => x.ClearDomainEvents());

  foreach (var domainEvent in domainEvents)
  {
      await _publisher.Publish(domainEvent, cancellationToken);
  }
  ```

## 4. Xử lý Sự kiện (Event Handlers)
- Mỗi Domain Event được xử lý bởi một hoặc nhiều lớp Handler tương ứng kế thừa từ `INotificationHandler<TEvent>`.
- Các Handler phải nằm ở tầng **Application** vì chúng đại diện cho các tác vụ phụ hoặc các luồng nghiệp vụ liên đới (ví dụ: Gửi email chào mừng khi nhận được `UserRegisteredDomainEvent`).
- Xử lý lỗi trong Event Handler phải độc lập và không làm gián đoạn luồng chính của transaction gốc trừ khi đó là yêu cầu nghiệp vụ bắt buộc (Strong Consistency).
