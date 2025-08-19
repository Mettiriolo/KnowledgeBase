import apiClient from './api'

// 精简版搜索服务 - 统一搜索入口
export const searchAPI = {
  // 智能搜索 - 主要搜索方法，使用AI进行智能搜索
  smartSearch: (query, options = {}) => {
    return apiClient.post('/ai/smart-search', {
      Query: query,
      Limit: options.limit || 10,
      UserId: options.userId,
      IncludeContent: options.includeContent || false
    })
  },

  // 语义搜索 - 备用方法
  semanticSearch: (params) => {
    return apiClient.post('/search/semantic', params)
  },

  // 全文搜索 - 备用方法
  fulltextSearch: (params) => {
    return apiClient.post('/search/fulltext', params)
  },

  // 搜索建议 - 用户体验优化
  getSuggestions: (query) => {
    return apiClient.get('/search/suggestions', {
      params: { q: query }
    })
  },

  // 搜索历史 - 用户体验优化
  getSearchHistory: () => {
    return apiClient.get('/search/history')
  },

  // 清除搜索历史
  clearSearchHistory: () => {
    return apiClient.delete('/search/history')
  }
}
