import apiClient from './api'

export const notesAPI = {
  // 获取笔记列表
  getNotes: (params = {}) => {
    return apiClient.get('/notes', { params })
  },

  // 获取单个笔记
  getNote: (id) => {
    return apiClient.get(`/notes/${id}`)
  },

  // 创建笔记
  createNote: (noteData) => {
    return apiClient.post('/notes', noteData)
  },

  // 更新笔记
  updateNote: (id, noteData) => {
    return apiClient.put(`/notes/${id}`, noteData)
  },

  // 删除笔记
  deleteNote: (id) => {
    return apiClient.delete(`/notes/${id}`)
  },

  // 获取标签列表
  getTags: () => {
    return apiClient.get('/notes/tags')
  },

  // 创建标签
  createTag: (tagData) => {
    return apiClient.post('/notes/tags', tagData)
  },

  // 更新标签
  updateTag: (id, tagData) => {
    return apiClient.put(`/notes/tags/${id}`, tagData)
  },

  // 删除标签
  deleteTag: (id) => {
    return apiClient.delete(`/notes/tags/${id}`)
  },

  // 批量操作
  batchDelete: (ids) => {
    return apiClient.post('/notes/batch-delete', { ids })
  },

  // 导出笔记
  exportNotes: (params = {}) => {
    return apiClient.get('/notes/export', {
      params,
      responseType: 'blob'
    })
  },

  // 导入笔记
  importNotes: (file) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient.post('/notes/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
  }
}
