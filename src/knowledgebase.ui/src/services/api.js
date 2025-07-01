import axios from 'axios'
import router from '@/router'
//import { useAuthStore } from '@/stores/auth'
import { useNotificationStore } from '@/stores/notification'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

// 创建axios实例
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
apiClient.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    const notificationStore = useNotificationStore()

    if (error.response) {
      switch (error.response.status) {
        case 401:
          // 未授权，清除token并跳转到登录页
          //const authStore = useAuthStore()
          // authStore.logout()
          router.push('/login')
          notificationStore.error('认证失败', '请重新登录')
          break
        case 403:
          notificationStore.error('权限不足', '您没有权限执行此操作')
          break
        case 404:
          notificationStore.error('资源不存在', '请求的资源未找到')
          break
        case 500:
          notificationStore.error('服务器错误', '服务器处理请求时发生错误')
          break
        default:
          notificationStore.error('请求失败', error.response.data?.message || '发生未知错误')
      }
    } else if (error.request) {
      notificationStore.error('网络错误', '无法连接到服务器')
    } else {
      notificationStore.error('请求错误', error.message)
    }

    return Promise.reject(error)
  }
)

export default apiClient
