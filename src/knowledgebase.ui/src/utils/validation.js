export const validators = {
  email: (value) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!value) return '邮箱地址不能为空'
    if (!emailRegex.test(value)) return '请输入有效的邮箱地址'
    return null
  },

  password: (value) => {
    if (!value) return '密码不能为空'
    if (value.length < 6) return '密码至少需要6个字符'
    if (value.length > 128) return '密码最多128个字符'
    if (!/(?=.*[a-z])/.test(value)) return '密码必须包含小写字母'
    if (!/(?=.*[A-Z])/.test(value)) return '密码必须包含大写字母'
    if (!/(?=.*\d)/.test(value)) return '密码必须包含数字'
    return null
  },

  username: (value) => {
    if (!value) return '用户名不能为空'
    if (value.length < 2) return '用户名至少需要2个字符'
    if (value.length > 50) return '用户名最多50个字符'
    if (!/^[a-zA-Z0-9_\u4e00-\u9fa5]+$/.test(value)) return '用户名只能包含字母、数字、下划线和中文'
    return null
  },

  noteTitle: (value) => {
    if (!value) return '标题不能为空'
    if (value.length < 1) return '标题至少需要1个字符'
    if (value.length > 200) return '标题最多200个字符'
    return null
  },

  noteContent: (value) => {
    if (!value) return '内容不能为空'
    if (value.length < 1) return '内容至少需要1个字符'
    if (value.length > 50000) return '内容最多50000个字符'
    return null
  },

  tagName: (value) => {
    if (!value) return '标签名不能为空'
    if (value.length < 1) return '标签名至少需要1个字符'
    if (value.length > 30) return '标签名最多30个字符'
    if (!/^[a-zA-Z0-9_\u4e00-\u9fa5]+$/.test(value)) return '标签名只能包含字母、数字、下划线和中文'
    return null
  },

  required: (value) => {
    if (!value || (typeof value === 'string' && value.trim() === '')) {
      return '此字段不能为空'
    }
    return null
  },

  minLength: (min) => (value) => {
    if (!value || value.length < min) {
      return `至少需要${min}个字符`
    }
    return null
  },

  maxLength: (max) => (value) => {
    if (value && value.length > max) {
      return `最多${max}个字符`
    }
    return null
  },

  pattern: (regex, message) => (value) => {
    if (value && !regex.test(value)) {
      return message
    }
    return null
  },

  confirmPassword: (originalPassword) => (value) => {
    if (!value) return '确认密码不能为空'
    if (value !== originalPassword) return '两次输入的密码不一致'
    return null
  },

  url: (value) => {
    if (!value) return null
    try {
      new URL(value)
      return null
    } catch {
      return '请输入有效的URL'
    }
  },

  phoneNumber: (value) => {
    if (!value) return null
    const phoneRegex = /^1[3456789]\d{9}$/
    if (!phoneRegex.test(value)) return '请输入有效的手机号码'
    return null
  }
}

export const validateForm = (formData, rules) => {
  const errors = {}
  
  Object.keys(rules).forEach(field => {
    const fieldRules = Array.isArray(rules[field]) ? rules[field] : [rules[field]]
    const value = formData[field]
    
    for (const rule of fieldRules) {
      const error = rule(value)
      if (error) {
        errors[field] = error
        break
      }
    }
  })
  
  return {
    isValid: Object.keys(errors).length === 0,
    errors
  }
}

export const createValidator = (rules) => {
  return (formData) => validateForm(formData, rules)
}

export const sanitizeInput = (input) => {
  if (typeof input !== 'string') return input
  
  return input
    .replace(/[<>]/g, '')
    .replace(/javascript:/gi, '')
    .replace(/on\w+=/gi, '')
    .trim()
}

export const validateFileUpload = (file, options = {}) => {
  const {
    maxSize = 10 * 1024 * 1024, // 10MB
    allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'text/plain', 'application/pdf'],
    maxFiles = 5
  } = options

  const errors = []

  if (!file) {
    errors.push('请选择文件')
    return { isValid: false, errors }
  }

  if (file.size > maxSize) {
    errors.push(`文件大小不能超过 ${Math.round(maxSize / 1024 / 1024)}MB`)
  }

  if (!allowedTypes.includes(file.type)) {
    errors.push('不支持的文件类型')
  }

  return {
    isValid: errors.length === 0,
    errors
  }
}