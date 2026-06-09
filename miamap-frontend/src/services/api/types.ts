export interface ApiErrorDetail {
  field?: string
  message: string
}

export interface ApiErrorResponse {
  code?: string
  message?: string
  details?: ApiErrorDetail[]
  [key: string]: unknown
}

export interface NormalizedError {
  code: string
  message: string
  details?: ApiErrorDetail[]
  originalError?: unknown
}
