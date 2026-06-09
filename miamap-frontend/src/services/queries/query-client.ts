import { QueryClient, QueryCache, MutationCache } from '@tanstack/react-query'
import type { NormalizedError } from '../api/types'

export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      const apiError = error as unknown as NormalizedError
      if (typeof window !== 'undefined') {
        const event = new CustomEvent('api-error', {
          detail: {
            message: apiError.message || 'Đã xảy ra lỗi hệ thống.',
            code: apiError.code,
          },
        })
        window.dispatchEvent(event)
      }
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      const apiError = error as unknown as NormalizedError
      if (typeof window !== 'undefined') {
        const event = new CustomEvent('api-error', {
          detail: {
            message: apiError.message || 'Đã xảy ra lỗi hệ thống.',
            code: apiError.code,
          },
        })
        window.dispatchEvent(event)
      }
    },
  }),
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})
