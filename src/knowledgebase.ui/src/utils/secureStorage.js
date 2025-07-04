import { useStorage } from '@vueuse/core'
import Cookies from 'js-cookie'

// 安全的存储键名前缀
const STORAGE_PREFIX = 'kb_secure_'
const TOKEN_KEY = `${STORAGE_PREFIX}auth_token`
const REFRESH_TOKEN_KEY = `${STORAGE_PREFIX}refresh_token`

// 使用 httpOnly cookie 存储 token
export const setAuthToken = (token, refreshToken) => {
  const expires = new Date()
  expires.setDate(expires.getDate() + 7) // 7天过期
  
  Cookies.set(TOKEN_KEY, token, {
    expires,
    sameSite: 'strict',
    secure: window.location.protocol === 'https:',
    httpOnly: false // 注意：在客户端无法设置 httpOnly，这需要后端配合设置
  })
  
  if (refreshToken) {
    Cookies.set(REFRESH_TOKEN_KEY, refreshToken, {
      expires: new Date(expires.getTime() + 7 * 24 * 60 * 60 * 1000), // 14天过期
      sameSite: 'strict',
      secure: window.location.protocol === 'https:'
    })
  }
}

export const getAuthToken = () => {
  return Cookies.get(TOKEN_KEY)
}

export const getRefreshToken = () => {
  return Cookies.get(REFRESH_TOKEN_KEY)
}

export const removeAuthTokens = () => {
  Cookies.remove(TOKEN_KEY)
  Cookies.remove(REFRESH_TOKEN_KEY)
}

// 安全的本地存储封装
export const secureLocalStorage = {
  set: (key, value) => {
    try {
      const secureKey = `${STORAGE_PREFIX}${key}`
      const data = {
        value,
        timestamp: Date.now()
      }
      localStorage.setItem(secureKey, JSON.stringify(data))
      return true
    } catch (error) {
      console.error('Error in secureLocalStorage.set:', error)
      return false
    }
  },
  
  get: (key, maxAge = 24 * 60 * 60 * 1000) => { // 默认24小时过期
    try {
      const secureKey = `${STORAGE_PREFIX}${key}`
      const item = localStorage.getItem(secureKey)
      if (!item) return null
      
      const data = JSON.parse(item)
      // 检查是否过期
      if (Date.now() - data.timestamp > maxAge) {
        localStorage.removeItem(secureKey)
        return null
      }
      
      return data.value
    } catch (error) {
      console.error('Error in secureLocalStorage.get:', error)
      return null
    }
  },
  
  remove: (key) => {
    try {
      const secureKey = `${STORAGE_PREFIX}${key}`
      localStorage.removeItem(secureKey)
      return true
    } catch (error) {
      console.error('Error in secureLocalStorage.remove:', error)
      return false
    }
  }
}

export default {
  setAuthToken,
  getAuthToken,
  getRefreshToken,
  removeAuthTokens,
  secureLocalStorage
}
