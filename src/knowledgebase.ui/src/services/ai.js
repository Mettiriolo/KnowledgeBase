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
  },

  // AI写作改进
  improveWriting: (data) => {
    return apiClient.post('/ai/improve-writing', data)
  },

  // AI内容扩展
  expandContent: (data) => {
    return apiClient.post('/ai/expand-content', data)
  },

  // AI建议大纲
  suggestOutline: (data) => {
    return apiClient.post('/ai/suggest-outline', data)
  },

  // AI语法检查
  checkGrammar: (data) => {
    return apiClient.post('/ai/check-grammar', data)
  },

  // AI回答关于笔记的问题
  askAboutNote: (data) => {
    return apiClient.post('/ai/ask-about-note', data)
  }
}
