import { defineStore } from 'pinia'
import { settingsAPI } from '@/services/settings'
// import { useLogger } from '@/composables/useLogger'

export const useSettingsStore = defineStore('settings', {
  state: () => ({
    settings: {
      theme: 'light',
      language: 'zh-CN',
      autoSave: true,
      notifications: true,
      twoFactorEnabled: false,
      fontSize: 'medium',
      editorMode: 'wysiwyg',
      timezone: 'Asia/Shanghai'
    },
    profile: {
      name: '',
      email: '',
      bio: '',
      avatar: '',
      createdAt: null,
      updatedAt: null
    },
    loginHistory: [],
    isLoading: false
  }),

  getters: {
    isDarkMode: (state) => {
      if (state.settings.theme === 'auto') {
        return window.matchMedia('(prefers-color-scheme: dark)').matches
      }
      return state.settings.theme === 'dark'
    },

    formattedProfile: (state) => ({
      ...state.profile,
      initials: state.profile.name
        ? state.profile.name.split(' ').map(n => n[0]).join('').toUpperCase()
        : '?'
    }),

    hasUnsavedChanges: (state) => {
      // 这里可以添加逻辑来检测是否有未保存的更改
      return false
    }
  },

  actions: {
    // 加载用户设置
    async loadSettings() {
      this.isLoading = true
      try {
        // 优先从服务器加载
        const response = await settingsAPI.getSettings()
        this.settings = { ...this.settings, ...response.data }
        
        // 应用主题设置
        this.applyTheme()
        
        // 保存到本地存储作为备份
        this.saveToLocal()
        
      } catch (error) {
        console.error('Failed to load settings from server:', error)
        // 从本地存储加载
        this.loadFromLocal()
      } finally {
        this.isLoading = false
      }
    },

    // 更新设置
    async updateSettings(newSettings) {
      const oldSettings = { ...this.settings }
      
      try {
        // 乐观更新
        this.settings = { ...this.settings, ...newSettings }
        
        // 应用主题变更
        if (newSettings.theme) {
          this.applyTheme()
        }
        
        // 保存到服务器
        await settingsAPI.updateSettings(newSettings)
        
        // 保存到本地存储
        this.saveToLocal()
        
      } catch (error) {
        // 回滚更改
        this.settings = oldSettings
        this.applyTheme()
        throw error
      }
    },

    // 更新个人信息
    async updateProfile(profileData) {
      const oldProfile = { ...this.profile }
      
      try {
        // 乐观更新
        this.profile = { ...this.profile, ...profileData }
        
        // 保存到服务器
        const response = await settingsAPI.updateProfile(profileData)
        this.profile = response.data
        
      } catch (error) {
        // 回滚更改
        this.profile = oldProfile
        throw error
      }
    },

    // 更新密码
    async updatePassword(passwordData) {
      try {
        await settingsAPI.updatePassword(passwordData)
      } catch (error) {
        throw error
      }
    },

    // 启用两步验证
    async enableTwoFactor() {
      try {
        const response = await settingsAPI.enableTwoFactor()
        this.settings.twoFactorEnabled = true
        this.saveToLocal()
        return response.data // 可能包含QR码等信息
      } catch (error) {
        throw error
      }
    },

    // 禁用两步验证
    async disableTwoFactor() {
      try {
        await settingsAPI.disableTwoFactor()
        this.settings.twoFactorEnabled = false
        this.saveToLocal()
      } catch (error) {
        throw error
      }
    },

    // 获取登录历史
    async getLoginHistory() {
      try {
        const response = await settingsAPI.getLoginHistory()
        this.loginHistory = response.data
        return response.data
      } catch (error) {
        console.error('Failed to load login history:', error)
        return []
      }
    },

    // 删除账户
    async deleteAccount() {
      try {
        await settingsAPI.deleteAccount()
        this.clearLocal()
      } catch (error) {
        throw error
      }
    },

    // 应用主题设置
    applyTheme() {
      const html = document.documentElement
      
      if (this.isDarkMode) {
        html.classList.add('dark')
      } else {
        html.classList.remove('dark')
      }
      
      // 更新CSS变量
      if (this.settings.fontSize) {
        html.style.fontSize = this.getFontSizeValue(this.settings.fontSize)
      }
    },

    // 获取字体大小值
    getFontSizeValue(size) {
      const sizes = {
        small: '14px',
        medium: '16px',
        large: '18px',
        xlarge: '20px'
      }
      return sizes[size] || sizes.medium
    },

    // 保存到本地存储
    saveToLocal() {
      try {
        localStorage.setItem('userSettings', JSON.stringify(this.settings))
        localStorage.setItem('userProfile', JSON.stringify(this.profile))
      } catch (error) {
        console.error('Failed to save settings to localStorage:', error)
      }
    },

    // 从本地存储加载
    loadFromLocal() {
      try {
        const savedSettings = localStorage.getItem('userSettings')
        const savedProfile = localStorage.getItem('userProfile')
        
        if (savedSettings) {
          this.settings = { ...this.settings, ...JSON.parse(savedSettings) }
        }
        
        if (savedProfile) {
          this.profile = { ...this.profile, ...JSON.parse(savedProfile) }
        }
        
        this.applyTheme()
        
      } catch (error) {
        console.error('Failed to load settings from localStorage:', error)
      }
    },

    // 清理本地存储
    clearLocal() {
      try {
        localStorage.removeItem('userSettings')
        localStorage.removeItem('userProfile')
      } catch (error) {
        console.error('Failed to clear local settings:', error)
      }
    },

    // 重置设置到默认值
    resetToDefaults() {
      this.settings = {
        theme: 'light',
        language: 'zh-CN',
        autoSave: true,
        notifications: true,
        twoFactorEnabled: false,
        fontSize: 'medium',
        editorMode: 'wysiwyg',
        timezone: 'Asia/Shanghai'
      }
      this.applyTheme()
      this.saveToLocal()
    },

    // 导出设置
    exportSettings() {
      const exportData = {
        settings: this.settings,
        profile: {
          name: this.profile.name,
          bio: this.profile.bio
        },
        exportedAt: new Date().toISOString()
      }
      
      const blob = new Blob([JSON.stringify(exportData, null, 2)], {
        type: 'application/json'
      })
      
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `settings-${Date.now()}.json`
      link.click()
      
      URL.revokeObjectURL(url)
    },

    // 导入设置
    async importSettings(file) {
      try {
        const text = await file.text()
        const importData = JSON.parse(text)
        
        if (importData.settings) {
          await this.updateSettings(importData.settings)
        }
        
        if (importData.profile) {
          await this.updateProfile(importData.profile)
        }
        
        return true
      } catch (error) {
        console.error('Failed to import settings:', error)
        throw new Error('设置文件格式错误')
      }
    }
  }
})