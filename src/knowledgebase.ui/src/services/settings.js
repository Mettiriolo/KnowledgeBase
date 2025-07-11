import apiClient from './api'

export const settingsAPI = {
  // 获取用户设置
  getSettings: () => {
    return apiClient.get('/user/settings')
  },

  // 更新用户设置
  updateSettings: (settings) => {
    return apiClient.put('/user/settings', settings)
  },

  // 获取个人信息
  getProfile: () => {
    return apiClient.get('/user/profile')
  },

  // 更新个人信息
  updateProfile: (profileData) => {
    return apiClient.put('/user/profile', profileData)
  },

  // 更新密码
  updatePassword: (passwordData) => {
    return apiClient.put('/user/password', passwordData)
  },

  // 启用两步验证
  enableTwoFactor: () => {
    return apiClient.post('/user/two-factor/enable')
  },

  // 禁用两步验证
  disableTwoFactor: () => {
    return apiClient.post('/user/two-factor/disable')
  },

  // 验证两步验证码
  verifyTwoFactor: (code) => {
    return apiClient.post('/user/two-factor/verify', { code })
  },

  // 获取登录历史
  getLoginHistory: () => {
    return apiClient.get('/user/login-history')
  },

  // 删除账户
  deleteAccount: () => {
    return apiClient.delete('/user/account')
  },

  // 获取账户统计信息
  getAccountStats: () => {
    return apiClient.get('/user/stats')
  },

  // 获取存储使用情况
  getStorageUsage: () => {
    return apiClient.get('/user/storage')
  },

  // 导出数据
  exportData: () => {
    return apiClient.get('/user/export', {
      responseType: 'blob'
    })
  },

  // 上传头像
  uploadAvatar: (file) => {
    const formData = new FormData()
    formData.append('avatar', file)
    
    return apiClient.post('/user/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  },

  // 删除头像
  deleteAvatar: () => {
    return apiClient.delete('/user/avatar')
  },

  // 更新通知设置
  updateNotificationSettings: (settings) => {
    return apiClient.put('/user/notifications', settings)
  },

  // 获取通知设置
  getNotificationSettings: () => {
    return apiClient.get('/user/notifications')
  },

  // 更新隐私设置
  updatePrivacySettings: (settings) => {
    return apiClient.put('/user/privacy', settings)
  },

  // 获取隐私设置
  getPrivacySettings: () => {
    return apiClient.get('/user/privacy')
  },

  // 清理缓存
  clearCache: () => {
    return apiClient.post('/user/cache/clear')
  },

  // 获取API使用情况
  getApiUsage: () => {
    return apiClient.get('/user/api-usage')
  },

  // 生成API密钥
  generateApiKey: () => {
    return apiClient.post('/user/api-key')
  },

  // 撤销API密钥
  revokeApiKey: (keyId) => {
    return apiClient.delete(`/user/api-key/${keyId}`)
  },

  // 获取会话列表
  getSessions: () => {
    return apiClient.get('/user/sessions')
  },

  // 终止会话
  terminateSession: (sessionId) => {
    return apiClient.delete(`/user/sessions/${sessionId}`)
  },

  // 终止所有其他会话
  terminateOtherSessions: () => {
    return apiClient.post('/user/sessions/terminate-others')
  }
}