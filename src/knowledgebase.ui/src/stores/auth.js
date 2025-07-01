import { defineStore } from 'pinia'
import { authAPI } from '@/services/auth'
import router from '@/router'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('token'),
    isLoading: false,
    lastLoginTime: null
  }),

  getters: {
    isAuthenticated: (state) => !!state.token && !!state.user,
    userName: (state) => state.user?.username || '',
    userEmail: (state) => state.user?.email || '',
    userId: (state) => state.user?.id || null
  },

  actions: {
    async login(email, password) {
      this.isLoading = true
      try {
        const response = await authAPI.login({ email, password })
        const { token, user } = response.data

        this.token = token
        this.user = user
        this.lastLoginTime = new Date()

        localStorage.setItem('token', token)

        return { success: true, data: response.data }
      } catch (error) {
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async register(username, email, password) {
      this.isLoading = true
      try {
        const response = await authAPI.register({ username, email, password })
        const { token, user } = response.data

        this.token = token
        this.user = user
        this.lastLoginTime = new Date()

        localStorage.setItem('token', token)

        return { success: true, data: response.data }
      } catch (error) {
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async logout() {
      try {
        await authAPI.logout()
      } catch (error) {
        console.error('Logout error:', error)
      } finally {
        this.user = null
        this.token = null
        localStorage.removeItem('token')
        router.push('/login')
      }
    },

    async validateToken() {
      if (!this.token) return false

      try {
        const response = await authAPI.validateToken()
        this.user = response.data.user
        return true
      } catch (error) {
        this.logout()
        return false
      }
    },

    async refreshToken() {
      try {
        const response = await authAPI.refreshToken()
        this.token = response.data.token
        localStorage.setItem('token', this.token)
        return true
      } catch (error) {
        this.logout()
        return false
      }
    },

    async updateUserInfo(userData) {
      try {
        const response = await authAPI.updateUserInfo(userData)
        this.user = response.data
        return { success: true, data: response.data }
      } catch (error) {
        throw error
      }
    },

    async changePassword(currentPassword, newPassword) {
      try {
        await authAPI.changePassword({ currentPassword, newPassword })
        return { success: true }
      } catch (error) {
        throw error
      }
    },

    async initializeAuth() {
      if (this.token) {
        try {
          await this.validateToken()
        } catch (error) {
          this.logout()
        }
      }
    }
  }
})
