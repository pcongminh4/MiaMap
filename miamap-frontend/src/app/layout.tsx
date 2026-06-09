import type { Metadata } from 'next'
import type { ReactNode } from 'react'
import './globals.css'
import { Providers } from './providers'
import { Toast } from '../features/common/components/Toast'

export const metadata: Metadata = {
  title: 'MiaMap',
  description: 'MiaMap frontend migrated to Next.js and TanStack Query.',
}

type RootLayoutProps = {
  children: ReactNode
}

export default function RootLayout({ children }: RootLayoutProps) {
  return (
    <html lang="vi">
      <body>
        <Providers>
          {children}
          <Toast />
        </Providers>
      </body>
    </html>
  )
}
