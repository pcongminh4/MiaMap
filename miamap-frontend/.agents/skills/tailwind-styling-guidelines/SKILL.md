---
name: tailwind-styling-guidelines
description: Quy chuẩn viết CSS, sử dụng Tailwind CSS, phối màu HSL, thiết kế Responsive & Animation trong dự án. Sử dụng khi viết giao diện UI component, chỉnh sửa CSS.
license: MIT
allowed-tools: None
---

# Tailwind CSS & Styling Guidelines

Tài liệu này quy chuẩn hóa thiết kế giao diện (UI), trải nghiệm người dùng (UX) và cách sử dụng Tailwind CSS trong dự án MiaMap.

## 1. Thiết lập màu sắc & Theme (Dark Mode)
- **Sử dụng HSL**: Ưu tiên sử dụng hệ màu HSL được khai báo tập trung trong `index.css` thông qua CSS Variables để dễ dàng hỗ trợ Dark Mode và chuyển đổi theme.
- **Dark Mode**: Sử dụng tiền tố `dark:` của Tailwind CSS để xử lý giao diện tối.
  - *Ví dụ*: `bg-white dark:bg-slate-900 text-slate-900 dark:text-white`
- Tránh sử dụng mã màu cứng như `bg-[#ff0000]`. Luôn sử dụng hệ màu có sẵn của Tailwind hoặc biến CSS đã định nghĩa trong config.

## 2. Thứ tự sắp xếp các class Tailwind
Để code CSS dễ đọc và dễ bảo trì, các class trong thuộc tính `className` nên được sắp xếp theo thứ tự ưu tiên sau:
1. **Layout & Positioning**: `flex`, `grid`, `absolute`, `relative`, `top-0`, `z-10`
2. **Box Model (Kích thước & Khoảng cách)**: `w-full`, `h-32`, `p-4`, `m-2`
3. **Typography (Chữ)**: `text-sm`, `font-bold`, `text-slate-800`
4. **Visuals (Nền, Viền, Bo góc)**: `bg-blue-500`, `border`, `rounded-lg`
5. **Interactive & Transitions (Hover, Focus, Animation)**: `hover:bg-blue-600`, `transition-all`, `duration-300`
6. **Responsive**: `md:p-6`, `lg:w-1/2`

## 3. Thiết kế Responsive
- Tuân thủ nguyên tắc **Mobile-First**. Mặc định viết class cho màn hình di động, sau đó dùng các breakpoint `md:`, `lg:`, `xl:` để ghi đè cho màn hình lớn hơn.
- Không viết code ẩn/hiện bừa bãi bằng `hidden md:block` nếu có thể dùng Flex/Grid linh hoạt.

## 4. Hiệu ứng động & Vi tương tác (Micro-animations)
- Tất cả các nút bấm, liên kết và thẻ tương tác bắt buộc phải có hiệu ứng `hover:` và `active:` rõ ràng.
- Sử dụng thuộc tính `transition` kết hợp với `duration-200` hoặc `duration-300` và `ease-in-out` để tạo cảm giác mượt mà khi hover.
- Đối với giao diện cao cấp, sử dụng hiệu ứng bóng đổ (`shadow-sm hover:shadow-md`) và chuyển dịch vị trí nhẹ (`hover:-translate-y-0.5`).
- *Ví dụ button chuẩn*:
  ```html
  <button className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-md transition-all duration-200 ease-in-out transform hover:-translate-y-0.5 active:translate-y-0 shadow-sm active:shadow-none">
    Tìm kiếm
  </button>
  ```
