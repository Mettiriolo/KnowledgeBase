import { defineStore } from 'pinia'

export const useAppStore = defineStore('app', {
  state: () => ({
    isGlobalLoading: false,
    sidebar: {
      isOpen: true,
      isPinned: false
    },
    theme: localStorage.getItem('theme') || 'light',
    language: localStorage.getItem('language') || 'zh-CN',
    settings: {
      autoSave: true,
      autoSaveInterval: 30000, // 30秒
      enableNotifications: true,
      enableSounds: false
    }
  }),

  getters: {
    isDarkMode: (state) => state.theme === 'dark'
  },

  actions: {
    setGlobalLoading(loading) {
      this.isGlobalLoading = loading
    },

    toggleSidebar() {
      this.sidebar.isOpen = !this.sidebar.isOpen
    },

    setSidebarOpen(open) {
      this.sidebar.isOpen = open
    },

    toggleSidebarPin() {
      this.sidebar.isPinned = !this.sidebar.isPinned
      localStorage.setItem('sidebarPinned', this.sidebar.isPinned)
    },

    setTheme(theme) {
      this.theme = theme
      localStorage.setItem('theme', theme)
      document.documentElement.classList.toggle('dark', theme === 'dark')
    },

    setLanguage(language) {
      this.language = language
      localStorage.setItem('language', language)
    },

    updateSettings(settings) {
      this.settings = { ...this.settings, ...settings }
      localStorage.setItem('appSettings', JSON.stringify(this.settings))
    },

    initializeApp() {
      // 恢复设置
      const savedSettings = localStorage.getItem('appSettings')
      if (savedSettings) {
        this.settings = { ...this.settings, ...JSON.parse(savedSettings) }
      }

      // 恢复侧边栏状态
      const sidebarPinned = localStorage.getItem('sidebarPinned')
      if (sidebarPinned !== null) {
        this.sidebar.isPinned = sidebarPinned === 'true'
      }

      // 应用主题
      document.documentElement.classList.toggle('dark', this.theme === 'dark')
    }
  }
})
