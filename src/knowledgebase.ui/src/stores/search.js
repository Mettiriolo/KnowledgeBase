import { defineStore } from 'pinia'
import { searchAPI } from '@/services/search'

export const useSearchStore = defineStore('search', {
  state: () => ({
    searchResults: [],
    searchHistory: [],
    isSearching: false,
    lastQuery: '',
    searchType: 'semantic', // 'semantic' | 'fulltext'
    totalResults: 0,
    currentPage: 1,
    perPage: 10
  }),

  getters: {
    recentSearches: (state) => {
      return state.searchHistory.slice(0, 5)
    }
  },

  actions: {
    async searchNotes(query, type = 'semantic', page = 1, limit = 10) {
      if (!query.trim()) return

      this.isSearching = true
      this.lastQuery = query
      this.searchType = type

      try {
        let response
        if (type === 'semantic') {
          response = await searchAPI.semanticSearch({ query, page, limit })
        } else {
          response = await searchAPI.fullTextSearch({ query, page, limit })
        }

        this.searchResults = response.data.results
        this.totalResults = response.data.total
        this.currentPage = page
        this.perPage = limit
        this.addToHistory(query)
        
        return response.data.results
      } catch (error) {
        console.error('Search failed:', error)
        throw error
      } finally {
        this.isSearching = false
      }
    },

    async advancedSearch(params) {
      this.isSearching = true
      try {
        const response = await searchAPI.advancedSearch(params)
        this.searchResults = response.data.results
        this.totalResults = response.data.total
        return response.data
      } catch (error) {
        console.error('Advanced search failed:', error)
        throw error
      } finally {
        this.isSearching = false
      }
    },

    addToHistory(searchItem) {
      // 支持字符串或对象格式
      let item
      if (typeof searchItem === 'string') {
        item = { query: searchItem.trim(), type: 'fulltext' }
      } else {
        item = { 
          query: searchItem.query?.trim() || '', 
          type: searchItem.type || 'fulltext' 
        }
      }
      
      if (!item.query) return

      // 移除重复项
      this.searchHistory = this.searchHistory.filter(histItem => histItem.query !== item.query)

      // 添加到开头
      this.searchHistory.unshift(item)

      // 限制历史记录数量
      this.searchHistory = this.searchHistory.slice(0, 10)

      // 保存到本地存储
      this.saveHistoryToLocal()
    },

    clearHistory() {
      this.searchHistory = []
      localStorage.removeItem('searchHistory')
      // 同步清除服务器端历史
      searchAPI.clearSearchHistory().catch(console.error)
    },

    removeFromHistory(query) {
      this.searchHistory = this.searchHistory.filter(item => {
        if (typeof item === 'string') {
          return item !== query
        }
        return item.query !== query
      })
      this.saveHistoryToLocal()
    },

    clearResults() {
      this.searchResults = []
      this.lastQuery = ''
      this.totalResults = 0
    },

    loadHistoryFromLocal() {
      try {
        const saved = localStorage.getItem('searchHistory')
        if (saved) {
          this.searchHistory = JSON.parse(saved)
        }
      } catch (error) {
        console.error('Failed to load search history:', error)
      }
    },

    saveHistoryToLocal() {
      try {
        localStorage.setItem('searchHistory', JSON.stringify(this.searchHistory))
      } catch (error) {
        console.error('Failed to save search history:', error)
      }
    },

    async loadSearchHistory() {
      try {
        const response = await searchAPI.getSearchHistory()
        this.searchHistory = response.data
      } catch (error) {
        console.error('Failed to load search history from server:', error)
        // 退回到本地存储
        this.loadHistoryFromLocal()
      }
    },

    // 语义搜索
    async semanticSearch(query, page = 1, limit = 20) {
      return await this.searchNotes(query, 'semantic', page, limit)
    },

    // 全文搜索  
    async fulltextSearch(query, page = 1, limit = 20) {
      return await this.searchNotes(query, 'fulltext', page, limit)
    }
  }
})
