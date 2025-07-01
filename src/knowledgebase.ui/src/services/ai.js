import apiClient from './api'

export const aiAPI = {
  // AI问答
  ask: (data) => {
    return apiClient.post('/ai/ask', data)
  },

  // AI生成摘要
  summarize: (data) => {
    return apiClient.post('/ai/summarize', data)
  },

  // AI提取关键词
  extractKeywords: (data) => {
    return apiClient.post('/ai/keywords', data)
  },

  // AI生成标签
  generateTags: (data) => {
    return apiClient.post('/ai/tags', data)
  },

  // AI内容优化建议
  suggestImprovements: (data) => {
    return apiClient.post('/ai/improve', data)
  },

  // 获取AI对话历史
  getConversations: () => {
    return apiClient.get('/ai/conversations')
  },

  // 删除AI对话
  deleteConversation: (id) => {
    return apiClient.delete(`/ai/conversations/${id}`)
  }
}
