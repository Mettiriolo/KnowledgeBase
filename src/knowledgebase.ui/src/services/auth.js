import apiClient from './api'

export const authAPI = {
  // 用户登录
  login: (credentials) => {
    return apiClient.post('/auth/login', credentials)
  },

  // 用户注册
  register: (userData) => {
    return apiClient.post('/auth/register', userData)
  },

  // 验证token
  validateToken: () => {
    return apiClient.get('/auth/validate')
  },

  // 刷新token
  refreshToken: () => {
    return apiClient.post('/auth/refresh')
  },

  // 退出登录
  logout: () => {
    return apiClient.post('/auth/logout')
  },

  // 获取用户信息
  getUserInfo: () => {
    return apiClient.get('/auth/user')
  },

  // 更新用户信息
  updateUserInfo: (data) => {
    return apiClient.put('/auth/user', data)
  },

  // 修改密码
  changePassword: (data) => {
    return apiClient.post('/auth/change-password', data)
  }
}
