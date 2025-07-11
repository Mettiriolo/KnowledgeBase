import { defineStore } from 'pinia'
import { aiAPI } from '@/services/ai'

export const useAIStore = defineStore('ai', {
  state: () => ({
    conversations: [],
    currentConversation: null,
    isThinking: false,
    suggestions: [],
    lastResponse: null
  }),

  getters: {
    recentConversations: (state) => {
      return state.conversations
        .sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt))
        .slice(0, 5)
    },

    hasConversations: (state) => state.conversations.length > 0
  },

  actions: {
    async askQuestion(question, context = null) {
      if (!question.trim()) return

      this.isThinking = true
      this.lastResponse = null

      try {
        const response = await aiAPI.ask({
          question: question.trim(),
          context
        })

        const conversation = {
          id: Date.now(),
          question: question.trim(),
          answer: response.data.answer,
          relevantNotes: response.data.relevantNotes || [],
          timestamp: new Date(),
          updatedAt: new Date()
        }

        this.conversations.push(conversation)
        this.currentConversation = conversation
        this.lastResponse = response.data

        // 保存到本地存储（限制数量）
        this.saveConversationsToLocal()

        return conversation
      } catch (error) {
        console.error('AI question failed:', error)
        throw error
      } finally {
        this.isThinking = false
      }
    },

    async generateSummary(content) {
      try {
        const response = await aiAPI.summarize({ content })
        return response.data.summary
      } catch (error) {
        console.error('Summary generation failed:', error)
        throw error
      }
    },

    async extractKeywords(content) {
      try {
        const response = await aiAPI.extractKeywords({ content })
        return response.data.keywords
      } catch (error) {
        console.error('Keywords extraction failed:', error)
        throw error
      }
    },

    async generateTags(content) {
      try {
        const response = await aiAPI.generateTags({ content })
        return response.data.tags
      } catch (error) {
        console.error('Tags generation failed:', error)
        throw error
      }
    },

    async suggestImprovements(content) {
      try {
        const response = await aiAPI.suggestImprovements({ content })
        return response.data.suggestions
      } catch (error) {
        console.error('Improvement suggestions failed:', error)
        throw error
      }
    },

    clearConversations() {
      this.conversations = []
      this.currentConversation = null
      localStorage.removeItem('aiConversations')
    },

    deleteConversation(id) {
      this.conversations = this.conversations.filter(conv => conv.id !== id)
      if (this.currentConversation?.id === id) {
        this.currentConversation = null
      }
      this.saveConversationsToLocal()
    },

    saveConversationsToLocal() {
      try {
        // 只保存最近的20条对话
        const toSave = this.conversations
          .sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt))
          .slice(0, 20)
        localStorage.setItem('aiConversations', JSON.stringify(toSave))
      } catch (error) {
        console.error('Failed to save conversations:', error)
      }
    },

    loadConversationsFromLocal() {
      try {
        const saved = localStorage.getItem('aiConversations')
        if (saved) {
          this.conversations = JSON.parse(saved)
        }
      } catch (error) {
        console.error('Failed to load conversations:', error)
      }
    },

    async loadConversationsFromServer() {
      try {
        const response = await aiAPI.getConversations()
        this.conversations = response.data
      } catch (error) {
        console.error('Failed to load conversations from server:', error)
        // 退回到本地存储
        this.loadConversationsFromLocal()
      }
    },

    async improveWriting(content) {
      if (!content.trim()) return content
      
      this.isThinking = true
      try {
        const response = await aiAPI.improveWriting({ content: content.trim() })
        return response.data.improvedContent
      } catch (error) {
        console.error('Writing improvement failed:', error)
        throw error
      } finally {
        this.isThinking = false
      }
    },

    async expandContent(content) {
      if (!content.trim()) return content
      
      this.isThinking = true
      try {
        const response = await aiAPI.expandContent({ content: content.trim() })
        return response.data.expandedContent
      } catch (error) {
        console.error('Content expansion failed:', error)
        throw error
      } finally {
        this.isThinking = false
      }
    },

    async suggestOutline(topic) {
      if (!topic.trim()) return []
      
      this.isThinking = true
      try {
        const response = await aiAPI.suggestOutline({ topic: topic.trim() })
        return response.data.outline
      } catch (error) {
        console.error('Outline suggestion failed:', error)
        throw error
      } finally {
        this.isThinking = false
      }
    },

    async checkGrammar(content) {
      if (!content.trim()) return { corrections: [], hasErrors: false }
      
      this.isThinking = true
      try {
        const response = await aiAPI.checkGrammar({ content: content.trim() })
        return response.data
      } catch (error) {
        console.error('Grammar check failed:', error)
        throw error
      } finally {
        this.isThinking = false
      }
    },

    async askAboutNote(note, question) {
      if (!note || !question.trim()) return null
      
      this.isThinking = true
      try {
        const response = await aiAPI.askAboutNote({ 
          note: {
            id: note.id,
            title: note.title,
            content: note.content,
            tags: note.tags
          },
          question: question.trim()
        })
        return response.data.answer
      } catch (error) {
        console.error('Note question failed:', error)
        throw error
      } finally {
        this.isThinking = false
      }
    }
  }
})
