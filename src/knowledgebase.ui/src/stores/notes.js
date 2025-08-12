import { defineStore } from 'pinia'
import { notesAPI } from '@/services/notes'
import { useNotificationStore } from '@/stores/notification'
import { debounce } from 'lodash-es'

export const useNotesStore = defineStore('notes', {
  state: () => ({
    notes: [],
    currentNote: null,
    tags: [],
    isLoading: false,
    selectedTag: null,
    totalNotes: 0,
    currentPage: 1,
    perPage: 10,
    searchQuery: '',
    sortBy: 'updatedAt',
    sortOrder: 'desc',
    cache: new Map(),
    lastFetchTime: null,
    isOffline: false
  }),

  getters: {
    filteredNotes: (state) => {
      let filtered = state.notes

      if (state.selectedTag) {
        filtered = filtered.filter(note =>
          note.tags.some(tag => tag.name === state.selectedTag)
        )
      }

      if (state.searchQuery) {
        const query = state.searchQuery.toLowerCase()
        filtered = filtered.filter(note =>
          note.title.toLowerCase().includes(query) ||
          note.content.toLowerCase().includes(query) ||
          note.tags.some(tag => tag.name.toLowerCase().includes(query))
        )
      }

      return filtered.sort((a, b) => {
        const aVal = a[state.sortBy]
        const bVal = b[state.sortBy]
        
        if (state.sortOrder === 'desc') {
          return new Date(bVal) - new Date(aVal)
        } else {
          return new Date(aVal) - new Date(bVal)
        }
      })
    },

    recentNotes: (state) => {
      return state.notes
        .slice()
        .sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt))
        .slice(0, 5)
    },

    notesWithTag: (state) => (tagName) => {
      return state.notes.filter(note =>
        note.tags.some(tag => tag.name === tagName)
      )
    },

    favoriteNotes: (state) => {
      return state.notes.filter(note => note.isFavorite)
    },

    archivedNotes: (state) => {
      return state.notes.filter(note => note.isArchived)
    },

    noteStats: (state) => {
      return {
        total: state.notes.length,
        favorites: state.notes.filter(note => note.isFavorite).length,
        archived: state.notes.filter(note => note.isArchived).length,
        tags: state.tags.length
      }
    },

    hasUnsavedChanges: (state) => {
      return state.currentNote && state.currentNote.isDirty
    }
  },

  actions: {
    async fetchNotes(page = 1, limit = 10, tagFilter = null, forceRefresh = false) {
      const cacheKey = `notes-${page}-${limit}-${tagFilter}`
      
      if (!forceRefresh && this.cache.has(cacheKey)) {
        const cached = this.cache.get(cacheKey)
        if (Date.now() - cached.timestamp < 5 * 60 * 1000) { // 5分钟缓存
          this.notes = cached.data.items
          this.totalNotes = cached.data.total
          this.currentPage = page
          this.perPage = limit
          return
        }
      }

      this.isLoading = true
      const notificationStore = useNotificationStore()
      
      try {
        const response = await notesAPI.getNotes({ page, limit, tag: tagFilter })
        this.notes = response.data.items
        this.totalNotes = response.data.total
        this.currentPage = page
        this.perPage = limit
        this.lastFetchTime = Date.now()
        
        this.cache.set(cacheKey, {
          data: response.data,
          timestamp: Date.now()
        })
      } catch (error) {
        console.error('Failed to fetch notes:', error)
        notificationStore.error('加载失败', '无法加载笔记列表')
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async getNote(id, useCache = true) {
      const cacheKey = `note-${id}`
      
      if (useCache && this.cache.has(cacheKey)) {
        const cached = this.cache.get(cacheKey)
        if (Date.now() - cached.timestamp < 2 * 60 * 1000) { // 2分钟缓存
          this.currentNote = cached.data
          return cached.data
        }
      }

      try {
        const response = await notesAPI.getNote(id)
        this.currentNote = response.data
        
        this.cache.set(cacheKey, {
          data: response.data,
          timestamp: Date.now()
        })
        
        return response.data
      } catch (error) {
        console.error('Failed to get note:', error)
        const notificationStore = useNotificationStore()
        notificationStore.error('加载失败', '无法加载笔记详情')
        throw error
      }
    },

    async createNote(noteData, showNotification = false) {
      const notificationStore = useNotificationStore()
      
      try {
        const response = await notesAPI.createNote(noteData)
        this.notes.unshift(response.data)
        this.totalNotes++
        
        this.clearCache()
        
        if (showNotification) {
          notificationStore.success('创建成功', '笔记已创建')
        }
        
        return response.data
      } catch (error) {
        console.error('Failed to create note:', error)
        notificationStore.error('创建失败', '无法创建笔记')
        throw error
      }
    },

    async updateNote(id, noteData, showNotification = false) {
      const notificationStore = useNotificationStore()
      
      try {
        const response = await notesAPI.updateNote(id, noteData)
        const index = this.notes.findIndex(note => note.id === id)
        if (index !== -1) {
          this.notes[index] = response.data
        }
        
        if (this.currentNote && this.currentNote.id === id) {
          this.currentNote = response.data
        }
        
        this.clearCache()
        
        if (showNotification) {
          notificationStore.success('更新成功', '笔记已更新')
        }
        
        return response.data
      } catch (error) {
        console.error('Failed to update note:', error)
        notificationStore.error('更新失败', '无法更新笔记')
        throw error
      }
    },

    async deleteNote(id, showNotification = false) {
      const notificationStore = useNotificationStore()
      
      try {
        await notesAPI.deleteNote(id)
        this.notes = this.notes.filter(note => note.id !== id)
        this.totalNotes--
        
        if (this.currentNote && this.currentNote.id === id) {
          this.currentNote = null
        }
        
        this.clearCache()
        
        if (showNotification) {
          notificationStore.success('删除成功', '笔记已删除')
        }
      } catch (error) {
        console.error('Failed to delete note:', error)
        notificationStore.error('删除失败', '无法删除笔记')
        throw error
      }
    },

    async fetchTags() {
      const cacheKey = 'tags'
      
      if (this.cache.has(cacheKey)) {
        const cached = this.cache.get(cacheKey)
        if (Date.now() - cached.timestamp < 10 * 60 * 1000) { // 10分钟缓存
          this.tags = cached.data
          return
        }
      }

      try {
        const response = await notesAPI.getTags()
        this.tags = response.data
        
        this.cache.set(cacheKey, {
          data: response.data,
          timestamp: Date.now()
        })
      } catch (error) {
        console.error('Failed to fetch tags:', error)
        const notificationStore = useNotificationStore()
        notificationStore.error('加载失败', '无法加载标签')
        throw error
      }
    },

    setSelectedTag(tag) {
      this.selectedTag = tag
    },

    setSearchQuery: debounce(function(query) {
      this.searchQuery = query
    }, 300),

    setSortBy(field, order = 'desc') {
      this.sortBy = field
      this.sortOrder = order
    },

    async batchDeleteNotes(ids) {
      const notificationStore = useNotificationStore()
      
      try {
        await notesAPI.batchDelete(ids)
        this.notes = this.notes.filter(note => !ids.includes(note.id))
        this.totalNotes -= ids.length
        
        this.clearCache()
        notificationStore.success('批量删除成功', `已删除 ${ids.length} 个笔记`)
      } catch (error) {
        console.error('Failed to batch delete notes:', error)
        notificationStore.error('批量删除失败', '无法删除选中的笔记')
        throw error
      }
    },

    async exportNotes(format = 'json') {
      const notificationStore = useNotificationStore()
      
      try {
        const response = await notesAPI.exportNotes({ format })
        const blob = new Blob([response.data], { type: 'application/octet-stream' })
        const link = document.createElement('a')
        link.href = window.URL.createObjectURL(blob)
        link.download = `notes_export_${new Date().toISOString().slice(0, 10)}.${format}`
        link.click()
        
        notificationStore.success('导出成功', '笔记导出完成')
      } catch (error) {
        console.error('Failed to export notes:', error)
        notificationStore.error('导出失败', '无法导出笔记')
        throw error
      }
    },

    async importNotes(file) {
      const notificationStore = useNotificationStore()
      
      try {
        const response = await notesAPI.importNotes(file)
        await this.fetchNotes(1, this.perPage, null, true)
        
        notificationStore.success('导入成功', `成功导入 ${response.data.count} 个笔记`)
        return response.data
      } catch (error) {
        console.error('Failed to import notes:', error)
        notificationStore.error('导入失败', '无法导入笔记')
        throw error
      }
    },

    async toggleFavorite(id) {
      const note = this.notes.find(n => n.id === id)
      if (note) {
        await this.updateNote(id, { ...note, isFavorite: !note.isFavorite }, false)
      }
    },

    async toggleArchive(id) {
      const note = this.notes.find(n => n.id === id)
      if (note) {
        await this.updateNote(id, { ...note, isArchived: !note.isArchived }, false)
      }
    },

    clearCache() {
      this.cache.clear()
    },

    clearCurrentNote() {
      this.currentNote = null
    },

    markNoteDirty(isDirty = true) {
      if (this.currentNote) {
        this.currentNote.isDirty = isDirty
      }
    },

    async autoSave() {
      if (this.currentNote && this.currentNote.isDirty) {
        try {
          await this.updateNote(this.currentNote.id, this.currentNote, false)
          this.markNoteDirty(false)
        } catch (error) {
          console.error('Auto-save failed:', error)
        }
      }
    }
  }
})
