import axios, { AxiosError } from 'axios'
import type { ApiErrorResponse, NormalizedError, ApiErrorDetail } from './types'

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL

export const http = axios.create({
  baseURL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request Interceptor: Tự động đính kèm Token nếu có
http.interceptors.request.use(
  (config) => {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token')
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response Interceptor: Chuẩn hóa lỗi từ API
http.interceptors.response.use(
  (response) => {
    return response
  },
  (error: AxiosError<any>) => {
    let normalizedError: NormalizedError = {
      code: 'UNKNOWN_ERROR',
      message: 'Đã xảy ra lỗi không xác định. Vui lòng thử lại.',
      originalError: error,
    }

    if (error.response) {
      const status = error.response.status
      const data = error.response.data

      // Check if it's a standard RFC 7807 ProblemDetails / ValidationProblemDetails
      const isProblemDetails = data && typeof data === 'object' && ('status' in data || 'title' in data)

      if (isProblemDetails) {
        let details: ApiErrorDetail[] | undefined = undefined

        // Parse validation errors from standard 'errors' object (RFC 7807)
        if (data.errors && typeof data.errors === 'object') {
          details = Object.entries(data.errors).flatMap(([field, msgs]) => {
            const messages = Array.isArray(msgs) ? msgs : [msgs]
            // Standardize field name to camelCase for frontend (e.g. Email -> email)
            const camelField = field.charAt(0).toLowerCase() + field.slice(1)
            return messages.map((msg: any) => ({
              field: camelField,
              message: String(msg),
            }))
          })
        }

        normalizedError = {
          code: data.code || `HTTP_STATUS_${status}`,
          message: data.detail || data.title || error.message || `Lỗi hệ thống (${status})`,
          details,
          originalError: error,
        }
      } else {
        normalizedError = {
          code: data?.code || `HTTP_STATUS_${status}`,
          message: data?.message || error.message || `Lỗi hệ thống (${status})`,
          details: data?.details,
          originalError: error,
        }
      }

      if (status === 401) {
        if (typeof window !== 'undefined') {
          localStorage.removeItem('token')
        }
      }
    } else if (error.request) {
      normalizedError = {
        code: 'NETWORK_ERROR',
        message: 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.',
        originalError: error,
      }
    } else {
      normalizedError = {
        code: 'REQUEST_SETUP_ERROR',
        message: error.message || 'Lỗi cấu hình yêu cầu mạng.',
        originalError: error,
      }
    }

    return Promise.reject(normalizedError)
  }
)
