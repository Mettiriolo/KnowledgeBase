import { defineStore } from 'pinia'
import { notesAPI } from '@/services/notes'

export const useNotesStore = defineStore('notes', {
  state: () => ({
    notes: [],
    currentNote: null,
    tags: [],
    isLoading: false,
    selectedTag: null,
    totalNotes: 0,
    currentPage: 1,
    perPage: 10
  }),

  getters: {
    filteredNotes: (state) => {
      if (!state.selectedTag) return state.notes
      return state.notes.filter(note =>
        note.tags.some(tag => tag.name === state.selectedTag)
      )
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
    }
  },

  actions: {
    async fetchNotes(page = 1, limit = 10, tagFilter = null) {
      this.isLoading = true
      try {
        const response = await notesAPI.getNotes({ page, limit, tag: tagFilter })
        this.notes = response.data.notes
        this.totalNotes = response.data.total
        this.currentPage = page
        this.perPage = limit
      } catch (error) {
        console.error('Failed to fetch notes:', error)
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async getNote(id) {
      try {
        const response = await notesAPI.getNote(id)
        this.currentNote = response.data
        return response.data
      } catch (error) {
        console.error('Failed to get note:', error)
        throw error
      }
    },

    async createNote(noteData) {
      try {
        const response = await notesAPI.createNote(noteData)
        this.notes.unshift(response.data)
        return response.data
      } catch (error) {
        console.error('Failed to create note:', error)
        throw error
      }
    },

    async updateNote(id, noteData) {
      try {
        const response = await notesAPI.updateNote(id, noteData)
        const index = this.notes.findIndex(note => note.id === id)
        if (index !== -1) {
          this.notes[index] = response.data
        }
        return response.data
      } catch (error) {
        console.error('Failed to update note:', error)
        throw error
      }
    },

    async deleteNote(id) {
      try {
        await notesAPI.deleteNote(id)
        this.notes = this.notes.filter(note => note.id !== id)
      } catch (error) {
        console.error('Failed to delete note:', error)
        throw error
      }
    },

    async fetchTags() {
      try {
        const response = await notesAPI.getTags()
        this.tags = response.data
      } catch (error) {
        console.error('Failed to fetch tags:', error)
        throw error
      }
    },

    setSelectedTag(tag) {
      this.selectedTag = tag
    },

    async batchDeleteNotes(ids) {
      try {
        await notesAPI.batchDelete(ids)
        this.notes = this.notes.filter(note => !ids.includes(note.id))
      } catch (error) {
        console.error('Failed to batch delete notes:', error)
        throw error
      }
    },

    async exportNotes(format = 'json') {
      try {
        const response = await notesAPI.exportNotes({ format })
        const blob = new Blob([response.data], { type: 'application/octet-stream' })
        const link = document.createElement('a')
        link.href = window.URL.createObjectURL(blob)
        link.download = `notes_export_${new Date().toISOString()}.${format}`
        link.click()
      } catch (error) {
        console.error('Failed to export notes:', error)
        throw error
      }
    },

    async importNotes(file) {
      try {
        const response = await notesAPI.importNotes(file)
        await this.fetchNotes()
        return response.data
      } catch (error) {
        console.error('Failed to import notes:', error)
        throw error
      }
    }
  }
})
