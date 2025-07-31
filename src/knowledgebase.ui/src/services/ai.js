import apiClient from './api'

// AI服务配置
const AI_CONFIG = {
  // 缓存设置
  CACHE_DURATION: 30 * 60 * 1000, // 30分钟
  // 搜索设置
  DEFAULT_SEARCH_LIMIT: 10,
  MAX_SEARCH_LIMIT: 50
}

// 精简版AI服务 - 只保留核心功能
export const aiAPI = {
  // 核心功能1: AI问答 - 基于上下文回答问题
  ask: (data) => {
    return apiClient.post('/ai/ask', data)
  },

  // 核心功能2: AI生成摘要
  summarize: (data) => {
    return apiClient.post('/ai/summarize', data)
  },

  // 核心功能3: 智能搜索 - 结合语义搜索和AI重排序
  smartSearch: (query, options = {}) => {
    return apiClient.post('/ai/smart-search', {
      query,
      limit: options.limit || 10,
      ...options
    })
  },

  // 流式回答 - 实时AI对话体验
  streamAnswer: (question, context) => {
    return apiClient.post('/ai/stream-answer', {
      question,
      context
    }, {
      responseType: 'stream'
    })
  },

  // 关于笔记的智能问答
  askAboutNote: (data) => {
    return apiClient.post('/ai/ask-about-note', data)
  }
}
