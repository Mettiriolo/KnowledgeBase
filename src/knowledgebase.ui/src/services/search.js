import apiClient from './api'

export const searchAPI = {
  // 语义搜索
  semanticSearch: (params) => {
    return apiClient.post('/search/semantic', params)
  },

  // 全文搜索
  fullTextSearch: (params) => {
    return apiClient.post('/search/fulltext', params)
  },

  // 高级搜索
  advancedSearch: (params) => {
    return apiClient.post('/search/advanced', params)
  },

  // 搜索建议
  getSuggestions: (query) => {
    return apiClient.get('/search/suggestions', {
      params: { q: query }
    })
  },

  // 搜索历史
  getSearchHistory: () => {
    return apiClient.get('/search/history')
  },

  // 清除搜索历史
  clearSearchHistory: () => {
    return apiClient.delete('/search/history')
  }
}
