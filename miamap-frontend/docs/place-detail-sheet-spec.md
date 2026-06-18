# Place Detail Sheet UI Spec

This document defines the UX and visual specification for the place detail sheet shown when a user taps a place marker on the map.

## Goal

When a user taps a place on the map, the app should show a clear, attractive, and fast-to-scan detail sheet without taking focus away from the map itself.

The sheet should:
- Preserve the map as the primary context
- Help the user make a quick decision
- Surface the most important actions first
- Remain visually complete even when some backend fields are missing

## Trigger

- User taps a place marker on the map
- The tapped marker becomes the selected marker
- The place detail sheet appears from the bottom with a short slide-up animation

## Placement

- Position: bottom-center of the map viewport
- Layer: above the map, below global blocking dialogs
- Desktop behavior: floating bottom sheet card
- Mobile behavior: bottom sheet anchored to the bottom edge

## Size

### Desktop

- Width: `420px`
- Min width: `380px`
- Max width: `460px`
- Height: auto
- Bottom offset: `20px`

### Mobile

- Width: `calc(100% - 24px)`
- Bottom offset: `12px`
- Border radius: keep rounded top corners and rounded outer shape

## Visual Style

- Background: white with slight translucency only if it still preserves readability
- Corner radius: `24px`
- Shadow:
  - soft outer shadow for elevation
  - one lighter inner highlight if needed
- Primary accent: use the same blue family as the current `Đăng nhập` button
- Typography tone: modern, clean, consumer-friendly
- Density: medium, optimized for quick reading

## Structure

Top-to-bottom content order:

1. Drag handle
2. Thumbnail
3. Main identity block
4. Metadata row
5. Status and address row
6. Primary actions
7. Secondary quick links

## Detailed Layout

### 1. Drag Handle

- Centered horizontally
- Width: `36px`
- Height: `4px`
- Radius: fully rounded
- Color: neutral gray
- Top margin: `10px`
- Bottom margin: `12px`

### 2. Thumbnail

- Shape: rounded rectangle
- Height: `104px`
- Width: full available width
- Radius: `18px`
- Content priority:
  - real image if available
  - category fallback art if image is missing
- Overlay: optional soft gradient to improve text contrast if text is ever placed on image

### 3. Main Identity Block

Contains:
- Place name
- Category badge

#### Place Name

- Example: `Nhà hàng Temple Club`
- Font size: `20px`
- Font weight: `700`
- Max lines: `2`
- Overflow: truncate after 2 lines

#### Category Badge

- Example: `Nhà hàng`
- Rounded pill
- Height: `28px`
- Horizontal padding: `12px`
- Background: light neutral or category-tinted surface
- Text size: `13px`
- Weight: `600`

### 4. Metadata Row

Show compact high-value info in a single row when possible:

- Rating
- Distance
- Drive time
- Price level

Example:

`4.8 ★   450 m   3 phút   $$$`

#### Style

- Font size: `14px`
- Weight: `600` for important values
- Secondary separators: subtle dots or spacing blocks
- Icon usage: optional small icons, but do not overload the row

### 5. Status and Address Row

#### Status

- Example: `Đang mở cửa`
- Pill or inline badge
- Green-tinted background when open
- Neutral style when unknown

#### Address

- Example: `29-31 Tôn Thất Thiệp, Quận 1`
- Font size: `13px`
- Color: muted slate
- Max lines: `1`
- Truncate when too long

### 6. Primary Actions

Three actions:

- `Chỉ đường`
- `Lưu`
- `Gọi`

#### Priority

- `Chỉ đường` is the main CTA
- `Lưu` is secondary
- `Gọi` is tertiary, but still visible if data exists

#### Button Specs

##### Chỉ đường

- Style: filled primary
- Height: `44px`
- Radius: `14px`
- Weight: `700`

##### Lưu

- Style: soft secondary
- Height: `44px`
- Radius: `14px`

##### Gọi

- Style: soft secondary
- Height: `44px`
- Radius: `14px`
- Disabled if phone number is missing

#### Layout

- Desktop:
  - 3 buttons in one row if width allows
- Mobile:
  - still prefer one row
  - if cramped, allow `Chỉ đường` on first row and `Lưu`, `Gọi` on second row

### 7. Secondary Quick Links

Quick links:
- `Đánh giá`
- `Ảnh`
- `Thực đơn`

#### Style

- Compact rounded chips or low-emphasis buttons
- Height: `36px`
- Text size: `13px`
- Weight: `600`

#### Availability Rules

- `Đánh giá`: always visible
- `Ảnh`: always visible
- `Thực đơn`: disabled or muted when unavailable

## Spacing

- Outer horizontal padding: `16px`
- Outer bottom padding: `16px`
- Gap between major sections: `12px`
- Gap between thumbnail and title: `12px`
- Gap between title and metadata: `8px`
- Gap between metadata and address/status: `8px`
- Gap between actions and quick links: `12px`

## Selected Marker Behavior

When a place is selected:
- The selected marker should visually stand out from other markers
- Recommended effects:
  - 1.1x to 1.2x scale bump
  - soft halo or glow
  - optional bounce-in micro animation

The selected marker must remain visible even while the sheet is open.

## Animation

### Open

- Duration: `180ms` to `220ms`
- Motion: slide up + fade in slightly
- Easing: smooth, non-bouncy

### Switch Between Places

- Do not fully close and reopen if the user taps another marker quickly
- Prefer content crossfade or direct content swap with subtle motion

### Close

- Tap blank map area to dismiss
- Swipe down on mobile to dismiss
- Duration: `160ms` to `200ms`

## Data Mapping

Expected fields:

- `name`
- `category`
- `rating`
- `distanceMeters`
- `driveTimeMinutes`
- `priceLevel`
- `isOpen`
- `address`
- `phoneNumber`
- `imageUrl`
- `menuAvailable`

## Fallback Rules

### Missing Image

Use a category-based fallback thumbnail instead of leaving the image area blank.

Suggested fallback direction:
- Restaurant: food plate or dining icon on a warm gradient
- Cafe: coffee cup or cafe icon on a soft brown gradient
- Hotel: bed or building icon on a calm neutral gradient
- ATM or bank: clean icon illustration
- Generic place: neutral illustrated map pin or category icon

### Missing Rating

Show:

`Chưa có đánh giá`

Do not show fake star values.

### Missing Distance

- Hide distance field
- Reflow remaining metadata cleanly

### Missing Drive Time

Show:

`Chưa ước tính`

or hide it if row density becomes awkward.

### Missing Opening Hours

Show:

`Chưa rõ giờ mở cửa`

Use a neutral badge style instead of green.

### Missing Phone Number

- Disable `Gọi`
- Keep the button visible for layout consistency

### Missing Menu

- Show `Thực đơn` in disabled state
- Do not remove the chip if consistency is preferred across places

### Missing Address

Show:

`Đang cập nhật địa chỉ`

## Content Language

All system-facing labels must use proper Vietnamese with diacritics:

- `Nhà hàng`
- `Quán cà phê`
- `Đang mở cửa`
- `Chưa rõ giờ mở cửa`
- `Chưa có đánh giá`
- `Chỉ đường`
- `Lưu`
- `Gọi`
- `Đánh giá`
- `Ảnh`
- `Thực đơn`
- `Đang cập nhật địa chỉ`

## Recommended States To Design

Minimum required states:

1. Full data state
2. Missing image fallback state
3. Partial metadata state
4. Loading state

## Loading State

Before place details finish loading:
- Show the bottom sheet container immediately
- Use skeleton blocks for:
  - thumbnail
  - title
  - metadata row
  - address
  - buttons

Loading should feel responsive and stable, without causing layout shifts.

## Accessibility Notes

- Buttons must maintain strong contrast
- Touch targets should be at least `44px` tall
- Important information should remain readable at a glance
- Avoid placing too much text inside the image area

## Final UX Principle

This sheet should feel like a decision card, not a full details page.

The user should be able to answer these questions within 2 to 3 seconds:
- What place is this?
- Is it worth opening?
- How far is it?
- Can I route there immediately?
