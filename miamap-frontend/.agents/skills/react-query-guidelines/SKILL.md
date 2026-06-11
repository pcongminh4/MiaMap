---
name: react-query-guidelines
description: Quy chuẩn quản lý bất đồng bộ state & caching với React Query (@tanstack/react-query). Sử dụng khi viết hook, component, gọi API, cache dữ liệu từ API.
license: MIT
allowed-tools: None
---

# React Query Guidelines & Best Practices

Tài liệu này định nghĩa tiêu chuẩn quản lý state bất đồng bộ, caching và tương tác API sử dụng React Query trong dự án MiaMap.

## 1. Tổ chức Query Keys
- **Quy tắc đặt tên**: Tất cả các Query Keys phải được định nghĩa tập trung trong một file hoặc đối tượng để tránh viết sai (typo) và dễ quản lý.
- **Cấu trúc Key**: Sử dụng mảng với cấu trúc phân cấp: `[feature, action/type, parameters]`.
  - *Đúng*: `queryKeys.places.nearby(lat, lng, radius)` -> `['places', 'nearby', { lat, lng, radius }]`
  - *Sai*: `['getNearbyPlaces', lat, lng]`
- Định nghĩa tập trung tại `src/services/queries/query-keys.ts`.

## 2. Thiết lập Caching (staleTime & gcTime)
- **Dữ liệu tĩnh / Ít thay đổi** (ví dụ: thông tin địa điểm cố định, cấu hình bản đồ):
  - `staleTime`: `5 * 60 * 1000` (5 phút) hoặc lâu hơn.
  - `gcTime` (garbage collection): `10 * 60 * 1000` (10 phút).
- **Dữ liệu động / Thay đổi liên tục** (ví dụ: vị trí thời gian thực, trạng thái người dùng trực tuyến):
  - `staleTime`: `0` hoặc `1000` (1 giây).
  - Không cần thiết lập lại `gcTime` (mặc định 5 phút).

## 3. Quản lý custom hooks gọi API
- **Khuyến khích**: Luôn bọc `useQuery` và `useMutation` trong các custom hooks cụ thể của từng feature.
- UI components tuyệt đối không gọi trực tiếp `useQuery` từ axios hay gọi axios trực tiếp.
- *Ví dụ custom hook*:
  ```typescript
  export const useNearbyPlaces = (request: SearchNearbyPlacesRequest) => {
    return useQuery({
      queryKey: queryKeys.places.nearby(request.latitude, request.longitude, request.radiusInMeters),
      queryFn: () => mapService.searchNearbyPlaces(request),
      enabled: !!request.latitude && !!request.longitude,
      staleTime: 2 * 60 * 1000 // 2 phút
    })
  }
  ```

## 4. Xử lý Mutations & Cache Invalidation
- Khi thực hiện thay đổi dữ liệu (POST, PUT, DELETE), sử dụng `useMutation`.
- Sau khi Mutation thành công (`onSuccess`), phải thực hiện làm mới (invalidate) cache liên quan để đảm bảo UI hiển thị dữ liệu mới nhất.
  ```typescript
  const queryClient = useQueryClient();
  
  const mutation = useMutation({
    mutationFn: (newPlace) => placeService.create(newPlace),
    onSuccess: () => {
      // Refresh danh sách places
      queryClient.invalidateQueries({ queryKey: queryKeys.places.all });
    }
  });
  ```
- **Optimistic Updates**: Đối với trải nghiệm UI mượt mà (like, rating), khuyến khích sử dụng Optimistic Updates để cập nhật UI ngay lập tức trước khi server phản hồi.
