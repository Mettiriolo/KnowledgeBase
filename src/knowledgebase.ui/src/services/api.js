import axios from 'axios'
import router from '@/router'
import { useNotificationStore } from '@/stores/notification'
import { getAuthToken, removeAuthTokens } from '@/utils/secureStorage'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

class ApiService {
  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      timeout: 15000,
      headers: {
        'Content-Type': 'application/json'
      }
    })

    this.setupInterceptors()
  }

  setupInterceptors() {
    this.client.interceptors.request.use(
      (config) => {
        const token = getAuthToken()
        if (token) {
          config.headers.Authorization = `Bearer ${token}`
        }
        
        config.metadata = { startTime: new Date() }
        return config
      },
      (error) => {
        console.error('Request interceptor error:', error)
        return Promise.reject(error)
      }
    )

    this.client.interceptors.response.use(
      (response) => {
        const duration = new Date() - response.config.metadata.startTime
        console.log(`API Call: ${response.config.method?.toUpperCase()} ${response.config.url} - ${duration}ms`)
        return response
      },
      async (error) => {
        const notificationStore = useNotificationStore()
        
        if (error.response) {
          const { status, data } = error.response
          
          switch (status) {
            case 401:
              await this.handleUnauthorized()
              break
            case 403:
              notificationStore.error('权限不足', '您没有权限执行此操作')
              break
            case 404:
              notificationStore.error('资源不存在', '请求的资源未找到')
              break
            case 422:
              this.handleValidationErrors(data.errors)
              break
            case 429:
              notificationStore.error('请求过于频繁', '请稍后再试')
              break
            case 500:
              notificationStore.error('服务器错误', '服务器处理请求时发生错误')
              break
            default:
              notificationStore.error('请求失败', data?.message || '发生未知错误')
          }
        } else if (error.request) {
          notificationStore.error('网络错误', '无法连接到服务器，请检查网络连接')
        } else {
          notificationStore.error('请求错误', error.message)
        }

        // 在生产环境中，发送错误到监控服务
        if (import.meta.env.PROD) {
          try {
            const { errorReporting } = await import('./errorReporting')
            errorReporting.reportAPIError(error, {
              url: error.config?.url,
              method: error.config?.method,
              status: error.response?.status,
              data: error.response?.data
            })
          } catch (reportingError) {
            console.error('Error reporting failed:', reportingError)
          }
        }

        return Promise.reject(error)
      }
    )
  }

  async handleUnauthorized() {
    const notificationStore = useNotificationStore()
    
    try {
      const refreshResponse = await this.client.post('/auth/refresh')
      const { token } = refreshResponse.data
      
      if (token) {
        localStorage.setItem('token', token)
        return true
      }
    } catch (refreshError) {
      console.error('Token refresh failed:', refreshError)
    }
    
    removeAuthTokens()
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    
    router.push('/login')
    notificationStore.error('认证失败', '请重新登录')
    return false
  }

  handleValidationErrors(errors) {
    const notificationStore = useNotificationStore()
    
    if (Array.isArray(errors)) {
      errors.forEach(error => {
        notificationStore.error('验证错误', error.message)
      })
    } else if (typeof errors === 'object') {
      Object.values(errors).forEach(error => {
        notificationStore.error('验证错误', error)
      })
    }
  }

  async get(url, config = {}) {
    return this.client.get(url, config)
  }

  async post(url, data = {}, config = {}) {
    return this.client.post(url, data, config)
  }

  async put(url, data = {}, config = {}) {
    return this.client.put(url, data, config)
  }

  async patch(url, data = {}, config = {}) {
    return this.client.patch(url, data, config)
  }

  async delete(url, config = {}) {
    return this.client.delete(url, config)
  }

  setAuthToken(token) {
    if (token) {
      this.client.defaults.headers.common['Authorization'] = `Bearer ${token}`
    } else {
      delete this.client.defaults.headers.common['Authorization']
    }
  }

  getBaseURL() {
    return this.client.defaults.baseURL
  }
}

const apiService = new ApiService()

export default apiService
