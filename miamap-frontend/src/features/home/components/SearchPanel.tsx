type SearchType = 'origin' | 'destination'

type SearchPanelProps = {
  originText: string
  destinationText: string
  onOriginTextChange: (value: string) => void
  onDestinationTextChange: (value: string) => void
  onSearch: (type: SearchType) => void | Promise<void>
  onSwap: () => void | Promise<void>
  isLoading: boolean
  errorMessage: string
}

export function SearchPanel({
  originText,
  destinationText,
  onOriginTextChange,
  onDestinationTextChange,
  onSearch,
  onSwap,
  isLoading,
  errorMessage,
}: SearchPanelProps) {
  return (
    <aside className="absolute left-3 top-3 z-20 w-[400px] max-w-[calc(100%-24px)] rounded-2xl bg-white/96 p-2.5 shadow-[0_14px_40px_rgba(0,0,0,0.2)] md:left-5 md:top-5 md:max-w-[calc(100%-40px)] md:p-3.5">
      <div className="flex items-center justify-between">
        <button type="button" className="grid h-8 w-8 place-items-center rounded-full text-slate-700 transition hover:bg-slate-100 md:h-9 md:w-9" aria-label="Menu">
          <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2.4">
            <path d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        <h1 className="text-base font-bold leading-none tracking-tight text-slate-900 md:text-lg">Chỉ đường lái xe</h1>
        <span className="w-9" />
      </div>

      <div className="mt-3.5 flex gap-2.5">
        <div className="flex w-6 shrink-0 flex-col items-center pt-2 md:w-7">
          <span className="h-5 w-5 rounded-full border-[3px] border-sky-500 bg-white" />
          <span className="my-1.5 h-6 border-l-4 border-dotted border-slate-300" />
          <span className="grid h-5 w-5 place-items-center rounded-full border-[3px] border-red-500 bg-white">
            <span className="h-2 w-2 rounded-full bg-red-500" />
          </span>
        </div>

        <div className="min-w-0 flex-1 space-y-2.5">
          <div className="flex items-center gap-2 rounded-xl bg-slate-200/80 px-2.5 py-2 md:py-2.5">
            <input
              value={originText}
              onChange={(event) => onOriginTextChange(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Enter') {
                  void onSearch('origin')
                }
              }}
              className="w-full bg-transparent text-sm font-semibold text-slate-700 outline-none placeholder:text-slate-500 md:text-base"
              placeholder="Chọn điểm xuất phát"
            />
            <button type="button" className="ml-auto text-slate-500" aria-label="Search departure" onClick={() => void onSearch('origin')}>
              <svg viewBox="0 0 24 24" className="h-4 w-4 md:h-5 md:w-5" fill="none" stroke="currentColor" strokeWidth="2.4">
                <circle cx="11" cy="11" r="6" />
                <path d="M20 20l-4.1-4.1" />
              </svg>
            </button>
          </div>

          <div className="flex items-center gap-2 rounded-xl bg-slate-200/80 px-2.5 py-2 md:py-2.5">
            <input
              value={destinationText}
              onChange={(event) => onDestinationTextChange(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Enter') {
                  void onSearch('destination')
                }
              }}
              className="w-full bg-transparent text-sm font-semibold text-slate-700 outline-none placeholder:text-slate-500 md:text-base"
              placeholder="Chọn điểm đến"
            />
            <button type="button" className="ml-auto text-slate-500" aria-label="Search destination" onClick={() => void onSearch('destination')}>
              <svg viewBox="0 0 24 24" className="h-4 w-4 md:h-5 md:w-5" fill="none" stroke="currentColor" strokeWidth="2.4">
                <circle cx="11" cy="11" r="6" />
                <path d="M20 20l-4.1-4.1" />
              </svg>
            </button>
          </div>
        </div>

        <button type="button" className="self-center rounded-full p-1 text-slate-600 transition hover:bg-slate-100" aria-label="Swap direction" onClick={() => void onSwap()}>
          <svg viewBox="0 0 24 24" className="h-5 w-5 md:h-6 md:w-6" fill="none" stroke="currentColor" strokeWidth="2.2">
            <path d="M7 4v15" />
            <path d="M4 7l3-3 3 3" />
            <path d="M17 20V5" />
            <path d="M14 17l3 3 3-3" />
          </svg>
        </button>
      </div>

      <div className="mt-3.5 border-t border-slate-300 pt-2.5">
        <button type="button" className="flex items-center gap-2 rounded-lg px-1 py-1 text-sm font-medium text-slate-800 transition hover:bg-slate-100 md:gap-2 md:text-base">
          <svg viewBox="0 0 24 24" className="h-4 w-4 text-slate-500 md:h-5 md:w-5" fill="none" stroke="currentColor" strokeWidth="2.2">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 7v5l3 2" />
          </svg>
          Rời đi ngay
          <svg viewBox="0 0 24 24" className="h-3 w-3 md:h-4 md:w-4" fill="currentColor">
            <path d="M7 10l5 5 5-5" />
          </svg>
        </button>

        {isLoading && <p className="mt-1.5 text-xs text-sky-600">Dang tim duong di...</p>}
        {errorMessage && <p className="mt-1.5 text-xs text-red-600">{errorMessage}</p>}
      </div>
    </aside>
  )
}
