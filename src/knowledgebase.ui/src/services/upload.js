import apiClient from './api'

export const uploadAPI = {
  // 上传图片
  uploadImage: async (file, onProgress = null) => {
    const formData = new FormData()
    formData.append('image', file)
    
    const config = {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    }
    
    if (onProgress) {
      config.onUploadProgress = (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total)
        onProgress(percentCompleted)
      }
    }
    
    try {
      const response = await apiClient.post('/upload/image', formData, config)
      return response.data
    } catch (error) {
      if (error.response?.status === 413) {
        throw new Error('图片文件过大，请选择小于5MB的图片')
      } else if (error.response?.status === 400) {
        throw new Error('不支持的图片格式，请选择JPG、PNG或WebP格式')
      }
      throw new Error(error.response?.data?.message || '图片上传失败')
    }
  },

  // 上传文件（通用）
  uploadFile: async (file, onProgress = null) => {
    const formData = new FormData()
    formData.append('file', file)
    
    const config = {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    }
    
    if (onProgress) {
      config.onUploadProgress = (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total)
        onProgress(percentCompleted)
      }
    }
    
    try {
      const response = await apiClient.post('/upload/file', formData, config)
      return response.data
    } catch (error) {
      if (error.response?.status === 413) {
        throw new Error('文件过大，请选择小于10MB的文件')
      }
      throw new Error(error.response?.data?.message || '文件上传失败')
    }
  },

  // 删除上传的文件
  deleteFile: async (fileId) => {
    try {
      await apiClient.delete(`/upload/file/${fileId}`)
    } catch (error) {
      throw new Error(error.response?.data?.message || '文件删除失败')
    }
  },

  // 获取文件信息
  getFileInfo: async (fileId) => {
    try {
      const response = await apiClient.get(`/upload/file/${fileId}`)
      return response.data
    } catch (error) {
      throw new Error(error.response?.data?.message || '获取文件信息失败')
    }
  }
}

// 验证图片文件
export const validateImageFile = (file) => {
  const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif']
  const maxSize = 5 * 1024 * 1024 // 5MB
  
  if (!allowedTypes.includes(file.type)) {
    throw new Error('不支持的图片格式，请选择JPG、PNG、WebP或GIF格式')
  }
  
  if (file.size > maxSize) {
    throw new Error('图片文件过大，请选择小于5MB的图片')
  }
  
  return true
}

// 生成图片预览URL
export const createImagePreview = (file) => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = (e) => resolve(e.target.result)
    reader.onerror = () => reject(new Error('无法读取图片文件'))
    reader.readAsDataURL(file)
  })
}