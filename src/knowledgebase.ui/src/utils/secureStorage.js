// 安全存储工具类 - 不依赖外部库

// 安全的存储键名前缀
const STORAGE_PREFIX = 'kb_secure_'
const TOKEN_KEY = `${STORAGE_PREFIX}auth_token`
const REFRESH_TOKEN_KEY = `${STORAGE_PREFIX}refresh_token`

// Cookie操作工具函数
const setCookie = (name, value, options = {}) => {
  let cookieString = `${name}=${encodeURIComponent(value)}`
  
  if (options.expires) {
    cookieString += `; expires=${options.expires.toUTCString()}`
  }
  
  if (options.maxAge) {
    cookieString += `; max-age=${options.maxAge}`
  }
  
  if (options.path) {
    cookieString += `; path=${options.path}`
  } else {
    cookieString += `; path=/`
  }
  
  if (options.domain) {
    cookieString += `; domain=${options.domain}`
  }
  
  if (options.secure) {
    cookieString += `; secure`
  }
  
  if (options.sameSite) {
    cookieString += `; samesite=${options.sameSite}`
  }
  
  document.cookie = cookieString
}

const getCookie = (name) => {
  const cookies = document.cookie.split(';')
  for (let cookie of cookies) {
    const [cookieName, cookieValue] = cookie.trim().split('=')
    if (cookieName === name) {
      return decodeURIComponent(cookieValue)
    }
  }
  return null
}

const removeCookie = (name, options = {}) => {
  setCookie(name, '', {
    ...options,
    expires: new Date(0)
  })
}

// 使用 cookie 存储 token
export const setAuthToken = (token, refreshToken) => {
  const expires = new Date()
  expires.setDate(expires.getDate() + 7) // 7天过期
  
  setCookie(TOKEN_KEY, token, {
    expires,
    sameSite: 'strict',
    secure: window.location.protocol === 'https:'
  })
  
  if (refreshToken) {
    const refreshExpires = new Date()
    refreshExpires.setDate(refreshExpires.getDate() + 14) // 14天过期
    setCookie(REFRESH_TOKEN_KEY, refreshToken, {
      expires: refreshExpires,
      sameSite: 'strict',
      secure: window.location.protocol === 'https:'
    })
  }
}

export const getAuthToken = () => {
  return getCookie(TOKEN_KEY) || localStorage.getItem('token')
}

export const getRefreshToken = () => {
  return getCookie(REFRESH_TOKEN_KEY)
}

export const removeAuthTokens = () => {
  removeCookie(TOKEN_KEY)
  removeCookie(REFRESH_TOKEN_KEY)
  localStorage.removeItem('token')
  localStorage.removeItem('user')
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
