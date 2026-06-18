"use client"

type MapControlsProps = {
  onLocateMe: () => void
  onZoomIn: () => void
  onZoomOut: () => void
  currentZoom: number
  maxMapZoom: number
}

export function MapControls({ onLocateMe, onZoomIn, onZoomOut, currentZoom, maxMapZoom }: MapControlsProps) {
  return (
    <div className="absolute bottom-4 right-3 z-20 flex flex-col items-center gap-2 md:bottom-7 md:right-6 md:gap-3">
      <button type="button" className="grid h-11 w-11 place-items-center rounded-full bg-white text-red-500 shadow-lg md:h-14 md:w-14" aria-label="Báo cáo">
        <svg viewBox="0 0 24 24" className="h-8 w-8" fill="none" stroke="currentColor" strokeWidth="2.2">
          <path d="M4 18l1.5-4.5L14.5 4.5a2.1 2.1 0 0 1 3 3L8.5 16.5 4 18z" />
          <path d="M13 6l5 5" />
        </svg>
      </button>

      <button type="button" className="grid h-11 w-11 place-items-center rounded-full bg-white text-slate-700 shadow-lg md:h-14 md:w-14" aria-label="Định vị tôi" onClick={onLocateMe}>
        <svg viewBox="0 0 24 24" className="h-8 w-8" fill="none" stroke="currentColor" strokeWidth="2.2">
          <circle cx="12" cy="12" r="6" />
          <path d="M12 2v3M12 19v3M2 12h3M19 12h3" />
        </svg>
      </button>

      <div className="overflow-hidden rounded-2xl bg-white shadow-lg">
        <button
          type="button"
          className="grid h-11 w-11 place-items-center text-4xl text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40 md:h-14 md:w-14 md:text-5xl"
          aria-label="Phóng to"
          onClick={onZoomIn}
          disabled={currentZoom >= maxMapZoom}
        >
          +
        </button>
        <div className="h-px w-full bg-slate-200" />
        <button type="button" className="grid h-11 w-11 place-items-center text-4xl text-slate-700 transition hover:bg-slate-100 md:h-14 md:w-14 md:text-5xl" aria-label="Thu nhỏ" onClick={onZoomOut}>
          -
        </button>
      </div>
    </div>
  )
}
