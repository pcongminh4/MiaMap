---
name: leaflet-map-guidelines
description: Quy chuẩn tích hợp bản đồ Leaflet & React Leaflet. Sử dụng khi vẽ bản đồ, markers, căn chỉnh kích thước bản đồ, hiển thị đường đi (route path) vẽ thủ công.
allowed-tools: None
---

# Leaflet Map & Frontend Visualization Guidelines

Tài liệu này định nghĩa các quy tắc tích hợp bản đồ Leaflet trên Frontend Next.js cho dự án MiaMap.

## 1. Thiết lập cốt lõi: Bản đồ nền và Vẽ đường đi tùy chỉnh
- **Dữ liệu Bản đồ nền**: Ứng dụng chỉ sử dụng các dịch vụ bản đồ công cộng ngoài (như OpenStreetMap, CartoDB tiles) để làm hình nền hiển thị.
- **Không sử dụng dịch vụ định tuyến ngoài**: Tuyệt đối **KHÔNG** sử dụng các dịch vụ tìm đường ngoài (như Google Directions, Leaflet Routing Machine nối API ngoài) để vẽ đường.
- **Vẽ đường đi tự xây dựng**: Mọi đường đi hiển thị trên bản đồ đều dựa vào mảng tọa độ DTO (`pathPoints` chứa danh sách vĩ độ/kinh độ) trả về từ Minimal API tự phát triển của Backend. Frontend nhận danh sách điểm này và vẽ lên bản đồ bằng các component như `<Polyline>` của `react-leaflet`.

## 2. Quản lý Tọa độ (Coordinates System)
- Leaflet sử dụng định dạng mảng tọa độ hoặc đối tượng theo thứ tự **[Latitude, Longitude]** (Vĩ độ trước, Kinh độ sau).
  - *Ví dụ*: `[21.0285, 105.8542]` (Hà Nội).
- Cần chú ý chuyển đổi định dạng nếu nhận dữ liệu từ các nguồn khác (như GeoJSON có thể trả về Kinh độ trước). Luôn đảm bảo dữ liệu đưa vào component Leaflet tuân thủ thứ tự `[lat, lng]`.

## 3. Khắc phục lỗi hiển thị & Hiệu năng
- **Map Container Sizing**: Thẻ div chứa bản đồ Leaflet bắt buộc phải có chiều cao xác định rõ ràng (ví dụ: `h-[500px]` hoặc `h-screen`). Nếu bản đồ bị xám hoặc hiển thị lỗi mảnh (tiles mismatch) khi resize màn hình, hãy gọi `map.invalidateSize()` bằng cách lắng nghe sự kiện hoặc dùng React Ref.
- **Memory Cleanup**: Để tránh rò rỉ bộ nhớ khi chuyển trang, đảm bảo hủy mọi bản đồ nền hoặc sự kiện lắng nghe (event listeners) đăng ký trực tiếp trên đối tượng `L.Map` khi component unmount.
- **Tối ưu hóa Markers**: Khi hiển thị hàng trăm địa điểm (places) trên bản đồ, tránh render lại (re-render) markers không cần thiết bằng cách sử dụng `React.memo` cho các custom markers hoặc sử dụng tính năng **Marker Clustering** để gộp các điểm gần nhau.
- **Khởi tạo SSR**: Leaflet yêu cầu truy cập trực tiếp vào đối tượng `window` của trình duyệt. Do đó, component Bản đồ trong Next.js bắt buộc phải được load động thông qua `dynamic` của Next.js với thuộc tính `ssr: false`:
  ```typescript
  import dynamic from 'next/dynamic'
  
  const HomeMap = dynamic(() => import('./components/HomeMap'), {
    ssr: false,
  })
  ```
