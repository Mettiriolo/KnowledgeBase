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
  },

  // 忘记密码
  forgotPassword(data) {
    return apiClient.post('/auth/forgot-password', data)
  },
  // 验证重置密码的token
  verifyResetToken(data) {
    return apiClient.post('/auth/verify-reset-token', data)
  },
  // 重置密码
  resetPassword(data) {
    return apiClient.post('/auth/reset-password', data)
  }
}
