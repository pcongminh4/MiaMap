"use client"

import { useAIAgent } from '../hooks/useAIAgent'
import type { MapPoint } from '../../../services/map/dto/map.dto.request'
import type { BoundingBoxPlaceResponse } from '../../../services/map/dto/map.dto.response'

type AIAgentPanelProps = {
  mapCenter: MapPoint
  onSelectPlace: (place: BoundingBoxPlaceResponse) => void
  onDrawRoute: (place: BoundingBoxPlaceResponse) => void
}

export function AIAgentPanel({ mapCenter, onSelectPlace, onDrawRoute }: AIAgentPanelProps) {
  const {
    isOpen,
    setIsOpen,
    messages,
    inputValue,
    setInputValue,
    isSending,
    fileInputRef,
    messagesEndRef,
    handleSendText,
    handleImageUpload,
  } = useAIAgent({ mapCenter, onSelectPlace, onDrawRoute })

  if (!isOpen) {
    return (
      <button
        type="button"
        onClick={() => setIsOpen(true)}
        className="absolute bottom-5 right-5 z-20 flex h-14 w-14 items-center justify-center rounded-full bg-gradient-to-tr from-sky-500 to-indigo-600 shadow-[0_4px_20px_rgba(14,165,233,0.4)] text-white transition hover:scale-105"
        aria-label="Mở Trợ lý AI"
      >
        <svg viewBox="0 0 24 24" className="h-6 w-6" fill="none" stroke="currentColor" strokeWidth="2.2">
          <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
          <circle cx="12" cy="10" r="1.5" fill="currentColor" />
          <circle cx="8" cy="10" r="1.5" fill="currentColor" />
          <circle cx="16" cy="10" r="1.5" fill="currentColor" />
        </svg>
      </button>
    )
  }

  return (
    <div className="absolute bottom-20 right-5 z-20 flex h-[480px] w-[360px] max-w-[calc(100%-40px)] flex-col rounded-2xl bg-white/96 shadow-[0_14px_40px_rgba(0,0,0,0.25)] border border-slate-100/50 backdrop-blur-md">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-slate-100 p-4 bg-gradient-to-r from-sky-50 to-indigo-50/50 rounded-t-2xl">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-sky-500 text-white font-bold text-xs animate-pulse">
            AI
          </div>
          <div>
            <h2 className="text-sm font-bold text-slate-800">Trợ lý Bản đồ AI</h2>
            <span className="text-[10px] text-sky-600 font-semibold">Gemini Assistant</span>
          </div>
        </div>
        <button
          type="button"
          onClick={() => setIsOpen(false)}
          className="rounded-full p-1 text-slate-400 hover:bg-slate-200 transition"
          aria-label="Đóng"
        >
          <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="2.5">
            <path d="M18 6L6 18M6 6l12 12" />
          </svg>
        </button>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3.5 select-text">
        {messages.map((msg) => (
          <div
            key={msg.id}
            className={`flex flex-col ${
              msg.sender === 'user'
                ? 'items-end'
                : msg.sender === 'system'
                  ? 'items-center'
                  : 'items-start'
            }`}
          >
            {msg.imageUrl && (
              <img
                src={msg.imageUrl}
                alt="Uploaded"
                className="mb-1 max-h-32 rounded-lg object-cover shadow"
              />
            )}
            <div
              className={`max-w-[85%] rounded-2xl px-3.5 py-2 text-sm leading-relaxed ${
                msg.sender === 'user'
                  ? 'bg-sky-500 text-white rounded-br-none font-medium'
                  : msg.sender === 'system'
                    ? 'bg-slate-100 text-slate-500 text-xs text-center rounded-md py-1 px-2.5'
                    : 'bg-slate-100 text-slate-800 rounded-bl-none border border-slate-200/50'
              }`}
            >
              {msg.text}
            </div>

            {/* Suggested Place Action Buttons */}
            {msg.places && msg.places.map((place) => (
              <div key={place.placeId} className="mt-1.5 flex flex-wrap gap-1.5">
                <button
                  type="button"
                  onClick={() => onSelectPlace(place)}
                  className="flex items-center gap-1 rounded-full bg-sky-50 border border-sky-200 px-3 py-1 text-xs font-semibold text-sky-600 hover:bg-sky-100 transition"
                >
                  📍 {place.name}
                </button>
                <button
                  type="button"
                  onClick={() => onDrawRoute(place)}
                  className="flex items-center gap-1 rounded-full bg-indigo-50 border border-indigo-200 px-3 py-1 text-xs font-semibold text-indigo-600 hover:bg-indigo-100 transition"
                >
                  🚗 Chỉ đường
                </button>
              </div>
            ))}
          </div>
        ))}
        {isSending && (
          <div className="flex items-center gap-1.5 text-xs text-slate-400 pl-1">
            <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-slate-400" />
            <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-slate-400 delay-75" />
            <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-slate-400 delay-150" />
            AI đang phân tích...
          </div>
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* Input Form */}
      <form
        onSubmit={(e) => {
          e.preventDefault()
          handleSendText()
        }}
        className="flex items-center gap-2 border-t border-slate-100 p-3 bg-slate-50/50 rounded-b-2xl"
      >
        {/* Attachment Button */}
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-slate-200 text-slate-600 hover:bg-slate-300 transition"
          aria-label="Tải ảnh lên"
          disabled={isSending}
        >
          <svg viewBox="0 0 24 24" className="h-4.5 w-4.5" fill="none" stroke="currentColor" strokeWidth="2.2">
            <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            <path d="M12 9v6m-3-3h6" />
          </svg>
        </button>
        <input
          type="file"
          ref={fileInputRef}
          onChange={handleImageUpload}
          accept="image/*"
          className="hidden"
        />

        {/* Text Input */}
        <input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="Tìm quán xá, chỉ đường..."
          className="w-full rounded-xl border border-slate-200 bg-white px-3 py-1.5 text-sm font-semibold text-slate-700 outline-none focus:border-sky-400 placeholder:text-slate-400"
          disabled={isSending}
        />

        {/* Send Button */}
        <button
          type="submit"
          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-sky-500 text-white hover:bg-sky-600 transition disabled:opacity-50"
          disabled={!inputValue.trim() || isSending}
          aria-label="Gửi"
        >
          <svg viewBox="0 0 24 24" className="h-4.5 w-4.5" fill="currentColor">
            <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
          </svg>
        </button>
      </form>
    </div>
  )
}
