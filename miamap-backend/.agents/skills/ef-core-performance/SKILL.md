---
name: ef-core-performance
description: Quy chuẩn tối ưu hóa truy vấn Database và sử dụng Entity Framework Core (EF Core). Sử dụng khi tạo DbContext, Entity Configuration, viết LINQ query, hoặc viết migration.
license: MIT
allowed-tools: None
---

# EF Core & Database Performance Guidelines

Tài liệu này định nghĩa các tiêu chuẩn tối ưu hóa truy vấn cơ sở dữ liệu và sử dụng Entity Framework Core hiệu quả trong dự án MiaMap.

## 1. Sử dụng AsNoTracking cho các truy vấn Chỉ đọc (Read-only Queries)
- Đối với tất cả các truy vấn LINQ không có mục đích cập nhật hoặc xóa thực thể (ví dụ: các API GET lấy thông tin hiển thị lên bản đồ), bắt buộc phải sử dụng `.AsNoTracking()` hoặc `.AsNoTrackingWithIdentityResolution()`.
- Việc này giúp EF Core bỏ qua bước theo dõi trạng thái thực thể (change tracker), tiết kiệm bộ nhớ và tăng tốc độ xử lý đáng kể.
  - *Ví dụ*:
    ```csharp
    var places = await dbContext.Places
        .AsNoTracking()
        .Where(p => p.IsActive)
        .ToListAsync(cancellationToken);
    ```

## 2. Tránh lỗi truy vấn N+1 (N+1 Query Problem)
- Sử dụng `.Include()` (Eager Loading) để tải trước các thực thể quan hệ liên quan khi cần thiết.
- Đối với các quan hệ phức tạp và lượng dữ liệu lớn, cân nhắc sử dụng **Query Splitting** bằng cách thêm `.AsSplitQuery()` để EF Core tách câu lệnh thành các truy vấn đơn lẻ thay vì dùng JOIN khổng lồ làm chậm DB.
  ```csharp
  var routeDetails = await dbContext.Routes
      .AsNoTracking()
      .Include(r => r.Waypoints)
      .AsSplitQuery()
      .FirstOrDefaultAsync(r => r.Id == routeId, cancellationToken);
  ```

## 3. Chỉ lấy các trường dữ liệu cần thiết (Projections)
- Tránh việc tải toàn bộ thực thể từ Database nếu chỉ cần hiển thị một vài trường thông tin lên UI. Sử dụng `.Select()` để map trực tiếp dữ liệu sang DTOs.
- Việc này giảm tải băng thông mạng giữa Database Server và Application Server.
  ```csharp
  var placeNames = await dbContext.Places
      .AsNoTracking()
      .Where(p => p.Rating > 4.5)
      .Select(p => new PlaceSummaryDto(p.Id, p.Name, p.Rating))
      .ToListAsync(cancellationToken);
  ```

## 4. Tối ưu hóa truy vấn Địa lý (Spatial Queries)
- Dự án MiaMap xử lý tọa độ địa lý (Vĩ độ/Kinh độ).
- Luôn sử dụng thư viện Spatial chuyên dụng (như NetTopologySuite tích hợp trong EF Core) thay vì thực hiện tính toán khoảng cách bằng công thức toán học thủ công trong câu lệnh LINQ (vì câu lệnh thủ công sẽ không thể chuyển dịch xuống SQL tối ưu và không tận dụng được Spatial Index).
- Đảm bảo các cột tọa độ (Point) được cấu hình **Spatial Index** phù hợp trong phần cấu hình thực thể (EntityTypeConfiguration).
