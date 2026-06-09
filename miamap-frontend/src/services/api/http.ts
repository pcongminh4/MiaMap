import axios, { AxiosError } from 'axios'
import type { ApiErrorResponse, NormalizedError } from './types'

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
  (error: AxiosError<ApiErrorResponse>) => {
    let normalizedError: NormalizedError = {
      code: 'UNKNOWN_ERROR',
      message: 'Đã xảy ra lỗi không xác định. Vui lòng thử lại.',
      originalError: error,
    }

    if (error.response) {
      const status = error.response.status
      const data = error.response.data

      normalizedError = {
        code: data?.code || `HTTP_STATUS_${status}`,
        message: data?.message || error.message || `Lỗi hệ thống (${status})`,
        details: data?.details,
        originalError: error,
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
