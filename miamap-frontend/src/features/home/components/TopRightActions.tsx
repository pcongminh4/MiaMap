"use client"

export function TopRightActions() {
  return (
    <div className="absolute right-3 top-3 z-20 flex items-center gap-2 md:right-6 md:top-7 md:gap-3">
      <button type="button" className="grid h-10 w-10 place-items-center rounded-full bg-white text-slate-700 shadow-lg transition hover:scale-[1.02] md:h-14 md:w-14" aria-label="Apps">
        <span className="grid grid-cols-3 gap-1">
          {Array.from({ length: 9 }).map((_, index) => (
            <span key={index} className="h-1.5 w-1.5 rounded-full bg-slate-600" />
          ))}
        </span>
      </button>

      <button type="button" className="grid h-10 w-10 place-items-center rounded-full bg-white text-slate-700 shadow-lg transition hover:scale-[1.02] md:h-14 md:w-14" aria-label="More options">
        <span className="grid gap-1">
          <span className="h-1.5 w-1.5 rounded-full bg-slate-700" />
          <span className="h-1.5 w-1.5 rounded-full bg-slate-700" />
          <span className="h-1.5 w-1.5 rounded-full bg-slate-700" />
        </span>
      </button>

      <button type="button" className="rounded-full bg-sky-500 px-4 py-2 text-lg font-bold tracking-tight text-white shadow-lg transition hover:bg-sky-600 md:px-5 md:py-3 md:text-[18px]">
        Dang nhap
      </button>
    </div>
  )
}
