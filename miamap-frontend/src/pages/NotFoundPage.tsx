import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-6 text-center shadow-sm">
      <h1 className="text-3xl font-bold text-slate-900">404</h1>
      <p className="mt-2 text-slate-600">Trang ban tim khong ton tai.</p>
      <Link
        to="/"
        className="mt-5 inline-flex rounded-xl bg-slate-900 px-4 py-2.5 font-semibold text-white transition hover:bg-slate-700"
      >
        Về trang chủ
      </Link>
    </section>
  )
}
