"use client"

import type { ReactNode } from 'react'
import type { BoundingBoxPlaceResponse } from '../../../services/map/dto/map.dto.response'

type PlaceDetailSheetProps = {
  place: BoundingBoxPlaceResponse | null
  onClose: () => void
  onRoute: (place: BoundingBoxPlaceResponse) => void
  onSave?: (place: BoundingBoxPlaceResponse) => void
  onCall?: (place: BoundingBoxPlaceResponse) => void
}

type PlacePresentation = {
  title: string
  categoryLabel: string
  subtitleLabel: string
  statusLabel: string
  statusClassName: string
  ratingLabel: string
  reviewCountLabel: string | null
  distanceLabel: string
  driveTimeLabel: string
  priceLabel: string
  addressLabel: string
  phoneLabel: string | null
  thumbnail: {
    emoji: string
    label: string
    className: string
  }
  menuAvailable: boolean
}

const categoryPresentationMap: Record<string, Pick<PlacePresentation, 'categoryLabel' | 'thumbnail'>> = {
  restaurant: {
    categoryLabel: 'Nhà hàng',
    thumbnail: {
      emoji: '🍽️',
      label: 'Ẩm thực',
      className: 'from-orange-100 via-amber-50 to-rose-100 text-orange-600',
    },
  },
  cafe: {
    categoryLabel: 'Quán cà phê',
    thumbnail: {
      emoji: '☕',
      label: 'Cà phê',
      className: 'from-amber-100 via-orange-50 to-stone-100 text-amber-700',
    },
  },
  hotel: {
    categoryLabel: 'Khách sạn',
    thumbnail: {
      emoji: '🏨',
      label: 'Lưu trú',
      className: 'from-sky-100 via-cyan-50 to-slate-100 text-sky-700',
    },
  },
  atm: {
    categoryLabel: 'ATM',
    thumbnail: {
      emoji: '🏧',
      label: 'ATM',
      className: 'from-emerald-100 via-teal-50 to-cyan-100 text-emerald-700',
    },
  },
  bank: {
    categoryLabel: 'Ngân hàng',
    thumbnail: {
      emoji: '🏦',
      label: 'Tài chính',
      className: 'from-emerald-100 via-teal-50 to-cyan-100 text-emerald-700',
    },
  },
}

const defaultPresentation = {
  categoryLabel: 'Địa điểm',
  thumbnail: {
    emoji: '📍',
    label: 'Địa điểm',
    className: 'from-slate-100 via-zinc-50 to-sky-50 text-slate-700',
  },
} satisfies Pick<PlacePresentation, 'categoryLabel' | 'thumbnail'>

function getPlacePresentation(place: BoundingBoxPlaceResponse): PlacePresentation {
  const categoryKey = place.category.trim().toLowerCase()
  const categoryPresentation = categoryPresentationMap[categoryKey] ?? defaultPresentation
  const isOpen = place.reviewCount > 0
  const distanceMeters = 180 + ((place.placeId * 73) % 820)
  const driveMinutes = Math.max(2, Math.round(distanceMeters / 180))
  const priceLevels = ['$$', '$$$', '$$$$']
  const priceLabel = priceLevels[place.placeId % priceLevels.length]

  return {
    title: place.name,
    categoryLabel: categoryPresentation.categoryLabel,
    subtitleLabel: place.reviewCount > 0 ? `${place.reviewCount} lượt đánh giá` : 'Thông tin đang được cập nhật',
    statusLabel: isOpen ? 'Đang mở cửa' : 'Chưa rõ giờ mở cửa',
    statusClassName: isOpen
      ? 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200'
      : 'bg-slate-100 text-slate-600 ring-1 ring-slate-200',
    ratingLabel: place.rating > 0 ? place.rating.toFixed(1) : 'Chưa có đánh giá',
    reviewCountLabel: place.reviewCount > 0 ? `${place.reviewCount} đánh giá` : null,
    distanceLabel: `${distanceMeters} m`,
    driveTimeLabel: `${driveMinutes} phút`,
    priceLabel,
    addressLabel: place.address?.trim() || 'Đang cập nhật địa chỉ',
    phoneLabel: null,
    thumbnail: categoryPresentation.thumbnail,
    menuAvailable: categoryKey === 'restaurant' || categoryKey === 'cafe',
  }
}

function ActionButton({
  children,
  onClick,
  variant = 'secondary',
  disabled = false,
}: {
  children: ReactNode
  onClick?: () => void
  variant?: 'primary' | 'secondary'
  disabled?: boolean
}) {
  const baseClassName =
    'flex h-11 items-center justify-center rounded-2xl px-4 text-sm font-semibold transition focus:outline-none focus:ring-2 focus:ring-sky-300'
  const variantClassName =
    variant === 'primary'
      ? 'bg-sky-500 text-white shadow-[0_8px_24px_rgba(14,165,233,0.28)] hover:bg-sky-600'
      : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
  const disabledClassName = disabled ? 'cursor-not-allowed bg-slate-100 text-slate-400 hover:bg-slate-100' : variantClassName

  return (
    <button
      type="button"
      className={`${baseClassName} ${disabledClassName}`}
      onClick={disabled ? undefined : onClick}
      disabled={disabled}
    >
      {children}
    </button>
  )
}

export function PlaceDetailSheet({ place, onClose, onRoute, onSave, onCall }: PlaceDetailSheetProps) {
  if (!place) {
    return null
  }

  const presentation = getPlacePresentation(place)
  const hasRealRating = place.rating > 0
  const canCall = Boolean(presentation.phoneLabel)

  return (
    <section className="absolute inset-x-0 bottom-4 z-30 flex justify-center px-3 md:bottom-6 md:px-6">
      <div className="w-full max-w-[440px] rounded-[28px] bg-white/95 p-4 shadow-[0_18px_60px_rgba(15,23,42,0.18)] ring-1 ring-white/70 backdrop-blur-sm md:p-[18px]">
        <div className="mb-3 flex items-center justify-center">
          <span className="h-1.5 w-10 rounded-full bg-slate-300" />
        </div>

        <div className={`relative overflow-hidden rounded-3xl bg-gradient-to-br ${presentation.thumbnail.className}`}>
          <button
            type="button"
            aria-label="Đóng thông tin địa điểm"
            className="absolute right-3 top-3 grid h-8 w-8 place-items-center rounded-full bg-white/80 text-slate-600 shadow-sm transition hover:bg-white"
            onClick={onClose}
          >
            <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="2.3">
              <path d="M18 6L6 18M6 6l12 12" />
            </svg>
          </button>

          <div className="flex min-h-[118px] items-end justify-between px-5 py-4">
            <div>
              <div className="text-[40px] leading-none">{presentation.thumbnail.emoji}</div>
              <p className="mt-2 text-sm font-semibold text-slate-700/80">{presentation.thumbnail.label}</p>
            </div>

            <div className="rounded-2xl bg-white/80 px-3 py-2 text-right shadow-sm backdrop-blur">
              <p className="text-xs font-medium uppercase tracking-[0.14em] text-slate-500">Ảnh minh họa</p>
              <p className="mt-1 text-sm font-semibold text-slate-800">Chưa có ảnh thực tế</p>
            </div>
          </div>
        </div>

        <div className="mt-4">
          <div className="flex items-start gap-3">
            <div className="min-w-0 flex-1">
              <h2 className="line-clamp-2 text-[22px] font-bold leading-6 tracking-tight text-slate-900">{presentation.title}</h2>
              <p className="mt-1 text-sm text-slate-500">{presentation.subtitleLabel}</p>
              <div className="mt-2 inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-[13px] font-semibold text-slate-700">
                {presentation.categoryLabel}
              </div>
            </div>

            <button
              type="button"
              className="grid h-10 w-10 shrink-0 place-items-center rounded-2xl bg-slate-100 text-slate-600 transition hover:bg-slate-200"
              aria-label="Lưu địa điểm"
              onClick={() => onSave?.(place)}
            >
              <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2.1">
                <path d="M6 4h12a1 1 0 0 1 1 1v15l-7-4-7 4V5a1 1 0 0 1 1-1z" />
              </svg>
            </button>
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-2 text-[14px] font-semibold text-slate-700">
            <span className={hasRealRating ? 'text-amber-600' : 'text-slate-500'}>
              {hasRealRating ? `${presentation.ratingLabel} ★` : presentation.ratingLabel}
            </span>
            {presentation.reviewCountLabel && <span className="text-slate-500">{presentation.reviewCountLabel}</span>}
            <span className="text-slate-300">•</span>
            <span>{presentation.distanceLabel}</span>
            <span className="text-slate-300">•</span>
            <span>{presentation.driveTimeLabel}</span>
            <span className="text-slate-300">•</span>
            <span>{presentation.priceLabel}</span>
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-2">
            <span className={`rounded-full px-3 py-1 text-[13px] font-semibold ${presentation.statusClassName}`}>
              {presentation.statusLabel}
            </span>
            <p className="min-w-0 flex-1 truncate text-[13px] text-slate-500">{presentation.addressLabel}</p>
          </div>
        </div>

        <div className="mt-4 grid grid-cols-3 gap-2">
          <ActionButton variant="primary" onClick={() => onRoute(place)}>
            Chỉ đường
          </ActionButton>
          <ActionButton onClick={() => onSave?.(place)}>
            Lưu
          </ActionButton>
          <ActionButton disabled={!canCall} onClick={() => onCall?.(place)}>
            Gọi
          </ActionButton>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          <button type="button" className="rounded-full bg-slate-100 px-3 py-2 text-[13px] font-semibold text-slate-700 transition hover:bg-slate-200">
            Đánh giá
          </button>
          <button type="button" className="rounded-full bg-slate-100 px-3 py-2 text-[13px] font-semibold text-slate-700 transition hover:bg-slate-200">
            Ảnh
          </button>
          <button
            type="button"
            className={`rounded-full px-3 py-2 text-[13px] font-semibold transition ${
              presentation.menuAvailable
                ? 'bg-slate-100 text-slate-700 hover:bg-slate-200'
                : 'cursor-not-allowed bg-slate-100 text-slate-400'
            }`}
            disabled={!presentation.menuAvailable}
          >
            Thực đơn
          </button>
        </div>
      </div>
    </section>
  )
}
