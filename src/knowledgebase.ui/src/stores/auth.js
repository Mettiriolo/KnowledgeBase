import { defineStore } from 'pinia'
import { authAPI } from '@/services/auth'
import router from '@/router'
import { setAuthToken, getAuthToken, removeAuthTokens, secureLocalStorage } from '@/utils/secureStorage'
import { useNotificationStore } from '@/stores/notification'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: secureLocalStorage.get('user') || null,
    token: getAuthToken(),
    isLoading: false,
    lastLoginTime: secureLocalStorage.get('lastLoginTime'),
    loginAttempts: 0,
    isLocked: false,
    lockUntil: null
  }),

  getters: {
    isAuthenticated: (state) => !!state.token && !!state.user && !state.isLocked,
    userName: (state) => state.user?.username || '',
    userEmail: (state) => state.user?.email || '',
    userId: (state) => state.user?.id || null,
    userRole: (state) => state.user?.role || 'user',
    isAdmin: (state) => state.user?.role === 'admin',
    canRetryLogin: (state) => state.loginAttempts < 5 && !state.isLocked
  },

  actions: {
    async login(email, password) {
      if (!this.canRetryLogin) {
        throw new Error('账户已被锁定，请稍后再试')
      }

      this.isLoading = true
      const notificationStore = useNotificationStore()
      
      try {
        const response = await authAPI.login({ email, password })
        const { token, refreshToken, user } = response.data

        this.token = token
        this.user = user
        this.lastLoginTime = new Date()
        this.loginAttempts = 0
        this.isLocked = false
        this.lockUntil = null

        setAuthToken(token, refreshToken)
        secureLocalStorage.set('user', user)
        secureLocalStorage.set('lastLoginTime', this.lastLoginTime)

        notificationStore.success('登录成功', `欢迎回来，${user.username}！`)
        return { success: true, data: response.data }
      } catch (error) {
        this.loginAttempts++
        
        if (this.loginAttempts >= 5) {
          this.isLocked = true
          this.lockUntil = new Date(Date.now() + 15 * 60 * 1000) // 15分钟锁定
          notificationStore.error('登录失败', '多次登录失败，账户已被锁定15分钟')
        } else {
          notificationStore.error('登录失败', `用户名或密码错误，还可尝试${5 - this.loginAttempts}次`)
        }
        
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async register(username, email, password) {
      this.isLoading = true
      const notificationStore = useNotificationStore()
      
      try {
        const response = await authAPI.register({ username, email, password })
        const { token, refreshToken, user } = response.data

        this.token = token
        this.user = user
        this.lastLoginTime = new Date()

        setAuthToken(token, refreshToken)
        secureLocalStorage.set('user', user)
        secureLocalStorage.set('lastLoginTime', this.lastLoginTime)

        notificationStore.success('注册成功', '欢迎加入AI知识库！')
        return { success: true, data: response.data }
      } catch (error) {
        notificationStore.error('注册失败', error.response?.data?.message || '注册过程中发生错误')
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async logout() {
      const notificationStore = useNotificationStore()
      
      try {
        await authAPI.logout()
        notificationStore.success('退出成功', '已安全退出系统')
      } catch (error) {
        console.error('Logout error:', error)
      } finally {
        this.clearAuthState()
        router.push('/login')
      }
    },

    clearAuthState() {
      this.user = null
      this.token = null
      this.lastLoginTime = null
      this.loginAttempts = 0
      this.isLocked = false
      this.lockUntil = null
      
      removeAuthTokens()
      secureLocalStorage.remove('user')
      secureLocalStorage.remove('lastLoginTime')
    },

    async validateToken() {
      if (!this.token) return false

      try {
        const response = await authAPI.validateToken()
        this.user = response.data.user
        secureLocalStorage.set('user', this.user)
        return true
      } catch (error) {
        console.error('Token validation failed:', error)
        this.clearAuthState()
        return false
      }
    },

    async refreshToken() {
      try {
        const response = await authAPI.refreshToken()
        const { token, refreshToken } = response.data
        
        this.token = token
        setAuthToken(token, refreshToken)
        return true
      } catch (error) {
        console.error('Token refresh failed:', error)
        this.clearAuthState()
        return false
      }
    },

    async updateUserInfo(userData) {
      const notificationStore = useNotificationStore()
      
      try {
        const response = await authAPI.updateUserInfo(userData)
        this.user = response.data
        secureLocalStorage.set('user', this.user)
        
        notificationStore.success('更新成功', '用户信息已更新')
        return { success: true, data: response.data }
      } catch (error) {
        notificationStore.error('更新失败', error.response?.data?.message || '更新用户信息时发生错误')
        throw error
      }
    },

    async changePassword(currentPassword, newPassword) {
      const notificationStore = useNotificationStore()
      
      try {
        await authAPI.changePassword({ currentPassword, newPassword })
        notificationStore.success('密码修改成功', '请使用新密码登录')
        return { success: true }
      } catch (error) {
        notificationStore.error('密码修改失败', error.response?.data?.message || '修改密码时发生错误')
        throw error
      }
    },

    async forgotPassword(email) {
      const notificationStore = useNotificationStore()
      
      try {
        await authAPI.forgotPassword({ email })
        notificationStore.success('邮件已发送', '请查收重置密码邮件')
        return { success: true }
      } catch (error) {
        notificationStore.error('发送失败', error.response?.data?.message || '发送重置邮件时发生错误')
        throw error
      }
    },

    async resetPassword(token, email, newPassword, confirmPassword) {
      const notificationStore = useNotificationStore()
      
      try {
        await authAPI.resetPassword({ token, email, newPassword, confirmPassword })
        notificationStore.success('密码重置成功', '请使用新密码登录')
        return { success: true }
      } catch (error) {
        notificationStore.error('重置失败', error.response?.data?.message || '重置密码时发生错误')
        throw error
      }
    },

    async verifyResetToken(token, email) {
      try {
        const response = await authAPI.verifyResetToken({ token, email })
        return { success: true, data: response.data }
      } catch (error) {
        throw error
      }
    },

    async initializeAuth() {
      if (this.isLocked && this.lockUntil && new Date() > this.lockUntil) {
        this.isLocked = false
        this.lockUntil = null
        this.loginAttempts = 0
      }

      if (this.token && !this.isLocked) {
        try {
          await this.validateToken()
        } catch (error) {
          console.error('Auth initialization failed:', error)
          this.clearAuthState()
        }
      }
    },

    checkAuthExpiry() {
      const lastLogin = new Date(this.lastLoginTime)
      const now = new Date()
      const hoursSinceLogin = (now - lastLogin) / (1000 * 60 * 60)
      
      if (hoursSinceLogin > 24) {
        this.logout()
        return false
      }
      return true
    }
  }
})
