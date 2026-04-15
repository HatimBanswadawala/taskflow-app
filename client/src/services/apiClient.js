import axios from 'axios'

/**
 * Axios instance configured for TaskFlow API.
 * Base URL is empty because Vite's proxy forwards /api to the .NET backend.
 */
const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

/**
 * Request interceptor — runs BEFORE every request.
 * Attaches the JWT token if we have one in localStorage.
 */
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('taskflow-token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

/**
 * Response interceptor — runs AFTER every response.
 * On 401 (unauthorized), clear token and redirect to login.
 */
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('taskflow-token')
      localStorage.removeItem('taskflow-user')
      // Only redirect if not already on /login
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export default apiClient
