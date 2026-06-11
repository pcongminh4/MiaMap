---
name: spatial-data-rules
description: Quy chuẩn làm việc với dữ liệu không gian (Spatial Data) và các cấu trúc bản đồ địa lý. Sử dụng khi truy vấn tọa độ, lưu trữ tọa độ địa lý, thiết lập giải thuật tìm đường hoặc tính toán khoảng cách.
allowed-tools: None
---

# Spatial Data & Custom Routing Guidelines

Tài liệu này quy chuẩn cách làm việc với tọa độ địa lý và xây dựng các thuật toán bản đồ trong dự án MiaMap.

## 1. Nguyên tắc cốt lõi: Tự xây dựng Thuật toán (Custom-Built Logic)
- **Quy tắc quan trọng**: Hệ thống **CHỈ** sử dụng dịch vụ bản đồ bên ngoài cho việc lấy ảnh bản đồ nền (map tiles) và chuyển đổi địa chỉ (geocoding).
- **TẤT CẢ** các chức năng như: Tìm đường đi (Driving Route/Path Finding), xây dựng đồ thị giao thông (Road Graph), Dijkstra, và các thuật toán tìm kiếm bán kính/boundingBox địa lý đều được **tự xây dựng trực tiếp trong mã nguồn backend** (Ví dụ tại `RoutingRepository.cs`).
- Tuyệt đối KHÔNG gọi API ngoài (như Google Routes API, OSRM, GraphHopper) cho việc tính toán đường đi. Mọi logic tìm đường phải được thực thi trực tiếp trên tập dữ liệu Roads/Nodes được lưu trong cơ sở dữ liệu nội bộ.

## 2. Thứ tự Tọa độ trong NetTopologySuite
- Thư viện NetTopologySuite (NTS) sử dụng chuẩn GeoJSON với thứ tự tọa độ trong constructor `Point(x, y)` là **(Longitude, Latitude)** (Kinh độ trước, Vĩ độ sau).
- Tuy nhiên, API và người dùng nhập vào luôn là **(Latitude, Longitude)**. Bắt buộc phải ánh xạ chính xác để tránh lỗi định vị sang vùng biển hoặc quốc gia khác.
- *Ví dụ khởi tạo chuẩn*:
  ```csharp
  // startLongitude truyền vào X, startLatitude truyền vào Y
  var point = new Point(startLongitude, startLatitude)
  {
      SRID = 4326 // Hệ tọa độ GPS tiêu chuẩn WGS 84
  };
  ```

## 3. Quy chuẩn trả về DTO
- Khi trả dữ liệu tọa độ về cho Frontend, DTO phải đặt tên rõ ràng và trả về theo đúng thứ tự mà Frontend dễ dùng: `Latitude` (hoặc `lat`) và `Longitude` (hoặc `lng`) dưới dạng số thực `double`.
  ```csharp
  public sealed record GeoPoint(double Latitude, double Longitude);
  ```

## 4. Tối ưu hóa Truy vấn Không gian (Spatial Queries)
- Đối với tìm kiếm vị trí xung quanh (Nearby) hoặc trong một vùng (Bounding Box), sử dụng các hàm không gian của EF Core tích hợp như `Intersects`, `Distance` trên các cột hình học có chỉ mục không gian (Spatial Index).
- Không dùng công thức toán học tính khoảng cách thủ công (như Haversine) viết bằng code C# trong câu lệnh LINQ truy vấn SQL Database vì nó làm giảm hiệu năng và không tận dụng được index.
