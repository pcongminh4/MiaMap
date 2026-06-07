import axios from 'axios'

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL

export const http = axios.create({
  baseURL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})
