import { defineStore } from 'pinia'

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [],
    nextId: 1
  }),

  getters: {
    hasNotifications: (state) => state.notifications.length > 0,
    activeNotifications: (state) => state.notifications.filter(n => !n.dismissed)
  },

  actions: {
    add(notification) {
      const id = this.nextId++
      const newNotification = {
        id,
        type: 'info',
        title: '',
        message: '',
        duration: 5000,
        dismissible: true,
        timestamp: new Date(),
        dismissed: false,
        ...notification
      }

      this.notifications.push(newNotification)

      // 自动删除
      if (newNotification.duration > 0) {
        setTimeout(() => {
          this.remove(id)
        }, newNotification.duration)
      }

      return id
    },

    remove(id) {
      const index = this.notifications.findIndex(n => n.id === id)
      if (index > -1) {
        this.notifications.splice(index, 1)
      }
    },

    dismiss(id) {
      const notification = this.notifications.find(n => n.id === id)
      if (notification) {
        notification.dismissed = true
      }
    },

    clear() {
      this.notifications = []
    },

    success(title, message = '', options = {}) {
      return this.add({
        type: 'success',
        title,
        message,
        ...options
      })
    },

    error(title, message = '', options = {}) {
      return this.add({
        type: 'error',
        title,
        message,
        duration: 8000,
        ...options
      })
    },

    warning(title, message = '', options = {}) {
      return this.add({
        type: 'warning',
        title,
        message,
        duration: 6000,
        ...options
      })
    },

    info(title, message = '', options = {}) {
      return this.add({
        type: 'info',
        title,
        message,
        ...options
      })
    }
  }
})
