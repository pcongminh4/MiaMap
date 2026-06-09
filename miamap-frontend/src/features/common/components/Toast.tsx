"use client"

import { useEffect, useState } from 'react'

interface ToastMessage {
  id: string
  message: string
  code?: string
  type: 'error' | 'success' | 'info'
}

export function Toast() {
  const [toasts, setToasts] = useState<ToastMessage[]>([])

  useEffect(() => {
    const handleApiError = (event: Event) => {
      const customEvent = event as CustomEvent<{ message: string; code?: string }>
      const { message, code } = customEvent.detail
      
      const newToast: ToastMessage = {
        id: Math.random().toString(36).substring(2, 9),
        message,
        code,
        type: 'error',
      }

      setToasts((prev) => [...prev, newToast])

      // Tự động ẩn sau 4 giây
      setTimeout(() => {
        setToasts((prev) => prev.filter((t) => t.id !== newToast.id))
      }, 4000)
    }

    window.addEventListener('api-error', handleApiError)
    return () => {
      window.removeEventListener('api-error', handleApiError)
    }
  }, [])

  const removeToast = (id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
  }

  if (toasts.length === 0) return null

  return (
    <div className="fixed bottom-5 right-5 z-50 flex flex-col gap-3 max-w-sm w-full">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className="flex items-start gap-3 rounded-xl border border-red-100 bg-white/95 p-4 shadow-xl backdrop-blur-sm transition-all duration-300 animate-in slide-in-from-bottom-5 fade-in-50"
        >
          {/* Error Icon */}
          <div className="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-red-100 text-red-600">
            <svg
              className="h-3 w-3"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </div>

          {/* Content */}
          <div className="flex-1">
            <p className="text-sm font-semibold text-slate-900">Đã xảy ra lỗi</p>
            <p className="mt-1 text-xs text-slate-500 line-clamp-3 leading-relaxed">
              {toast.message}
            </p>
            {toast.code && (
              <span className="mt-2 inline-block rounded bg-red-50 px-1.5 py-0.5 text-[10px] font-mono font-medium text-red-700">
                Code: {toast.code}
              </span>
            )}
          </div>

          {/* Close Button */}
          <button
            onClick={() => removeToast(toast.id)}
            className="text-slate-400 hover:text-slate-600 shrink-0 transition-colors cursor-pointer"
          >
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      ))}
    </div>
  )
}
