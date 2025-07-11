const config = {
  // 应用基础配置
  app: {
    name: import.meta.env.VITE_APP_NAME || 'AI Knowledge Base',
    version: import.meta.env.VITE_APP_VERSION || '1.0.0',
    environment: import.meta.env.MODE || 'development',
    baseUrl: import.meta.env.BASE_URL || '/',
    description: 'AI驱动的知识库管理系统'
  },

  // API配置
  api: {
    baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
    timeout: parseInt(import.meta.env.VITE_API_TIMEOUT) || 15000,
    retryAttempts: parseInt(import.meta.env.VITE_API_RETRY_ATTEMPTS) || 3,
    retryDelay: parseInt(import.meta.env.VITE_API_RETRY_DELAY) || 1000
  },

  // 认证配置
  auth: {
    tokenKey: 'auth_token',
    refreshTokenKey: 'refresh_token',
    sessionTimeout: parseInt(import.meta.env.VITE_SESSION_TIMEOUT) || 24 * 60 * 60 * 1000, // 24小时
    maxLoginAttempts: parseInt(import.meta.env.VITE_MAX_LOGIN_ATTEMPTS) || 5,
    lockoutDuration: parseInt(import.meta.env.VITE_LOCKOUT_DURATION) || 15 * 60 * 1000, // 15分钟
    passwordMinLength: parseInt(import.meta.env.VITE_PASSWORD_MIN_LENGTH) || 6,
    passwordRequirements: {
      uppercase: true,
      lowercase: true,
      numbers: true,
      symbols: false
    }
  },

  // 缓存配置
  cache: {
    defaultTTL: parseInt(import.meta.env.VITE_CACHE_TTL) || 5 * 60 * 1000, // 5分钟
    maxSize: parseInt(import.meta.env.VITE_CACHE_MAX_SIZE) || 100,
    storagePrefix: 'kb_cache_'
  },

  // 存储配置
  storage: {
    prefix: 'kb_',
    encryptionKey: import.meta.env.VITE_STORAGE_ENCRYPTION_KEY || 'default_key',
    maxAge: parseInt(import.meta.env.VITE_STORAGE_MAX_AGE) || 7 * 24 * 60 * 60 * 1000 // 7天
  },

  // 性能配置
  performance: {
    debounceTime: parseInt(import.meta.env.VITE_DEBOUNCE_TIME) || 300,
    throttleTime: parseInt(import.meta.env.VITE_THROTTLE_TIME) || 100,
    virtualScrollItemHeight: parseInt(import.meta.env.VITE_VIRTUAL_SCROLL_ITEM_HEIGHT) || 40,
    lazyLoadOffset: parseInt(import.meta.env.VITE_LAZY_LOAD_OFFSET) || 100,
    imageCompressionQuality: parseFloat(import.meta.env.VITE_IMAGE_COMPRESSION_QUALITY) || 0.8
  },

  // 功能开关
  features: {
    enableOfflineMode: import.meta.env.VITE_ENABLE_OFFLINE_MODE === 'true',
    enablePWA: import.meta.env.VITE_ENABLE_PWA === 'true',
    enableAnalytics: import.meta.env.VITE_ENABLE_ANALYTICS === 'true',
    enableErrorReporting: import.meta.env.VITE_ENABLE_ERROR_REPORTING === 'true',
    enableDebugMode: import.meta.env.VITE_ENABLE_DEBUG_MODE === 'true',
    enableAutoSave: import.meta.env.VITE_ENABLE_AUTO_SAVE !== 'false', // 默认开启
    enableDarkMode: import.meta.env.VITE_ENABLE_DARK_MODE !== 'false', // 默认开启
    enableExport: import.meta.env.VITE_ENABLE_EXPORT !== 'false', // 默认开启
    enableSearch: import.meta.env.VITE_ENABLE_SEARCH !== 'false' // 默认开启
  },

  // 编辑器配置
  editor: {
    autoSaveInterval: parseInt(import.meta.env.VITE_AUTO_SAVE_INTERVAL) || 30000, // 30秒
    maxContentLength: parseInt(import.meta.env.VITE_MAX_CONTENT_LENGTH) || 50000,
    enableSpellCheck: import.meta.env.VITE_ENABLE_SPELL_CHECK !== 'false',
    enableSyntaxHighlight: import.meta.env.VITE_ENABLE_SYNTAX_HIGHLIGHT !== 'false',
    theme: import.meta.env.VITE_EDITOR_THEME || 'default',
    plugins: {
      codeBlock: true,
      colorSyntax: true,
      chart: false,
      tableMergedCell: false,
      uml: false
    }
  },

  // 文件上传配置
  upload: {
    maxFileSize: parseInt(import.meta.env.VITE_MAX_FILE_SIZE) || 10 * 1024 * 1024, // 10MB
    allowedTypes: (import.meta.env.VITE_ALLOWED_FILE_TYPES || 'image/jpeg,image/png,image/gif,text/plain,application/pdf').split(','),
    maxFiles: parseInt(import.meta.env.VITE_MAX_FILES) || 5,
    compressionQuality: parseFloat(import.meta.env.VITE_COMPRESSION_QUALITY) || 0.8
  },

  // 分页配置
  pagination: {
    defaultPageSize: parseInt(import.meta.env.VITE_DEFAULT_PAGE_SIZE) || 10,
    maxPageSize: parseInt(import.meta.env.VITE_MAX_PAGE_SIZE) || 100,
    pageSizeOptions: [10, 20, 50, 100]
  },

  // 搜索配置
  search: {
    debounceTime: parseInt(import.meta.env.VITE_SEARCH_DEBOUNCE_TIME) || 300,
    minQueryLength: parseInt(import.meta.env.VITE_MIN_QUERY_LENGTH) || 2,
    maxResults: parseInt(import.meta.env.VITE_MAX_SEARCH_RESULTS) || 50,
    enableHighlight: import.meta.env.VITE_ENABLE_SEARCH_HIGHLIGHT !== 'false',
    enableAutoComplete: import.meta.env.VITE_ENABLE_AUTO_COMPLETE !== 'false'
  },

  // 通知配置
  notifications: {
    position: import.meta.env.VITE_NOTIFICATION_POSITION || 'top-right',
    duration: parseInt(import.meta.env.VITE_NOTIFICATION_DURATION) || 5000,
    maxNotifications: parseInt(import.meta.env.VITE_MAX_NOTIFICATIONS) || 5,
    enableSound: import.meta.env.VITE_ENABLE_NOTIFICATION_SOUND === 'true',
    enablePush: import.meta.env.VITE_ENABLE_PUSH_NOTIFICATIONS === 'true'
  },

  // 主题配置
  theme: {
    default: import.meta.env.VITE_DEFAULT_THEME || 'light',
    enableSystemTheme: import.meta.env.VITE_ENABLE_SYSTEM_THEME !== 'false',
    customColors: {
      primary: import.meta.env.VITE_PRIMARY_COLOR || '#3b82f6',
      secondary: import.meta.env.VITE_SECONDARY_COLOR || '#6b7280',
      success: import.meta.env.VITE_SUCCESS_COLOR || '#10b981',
      warning: import.meta.env.VITE_WARNING_COLOR || '#f59e0b',
      error: import.meta.env.VITE_ERROR_COLOR || '#ef4444'
    }
  },

  // 日志配置
  logging: {
    level: import.meta.env.VITE_LOG_LEVEL || 'info',
    enableConsole: import.meta.env.VITE_ENABLE_CONSOLE_LOG !== 'false',
    enableRemote: import.meta.env.VITE_ENABLE_REMOTE_LOG === 'true',
    remoteUrl: import.meta.env.VITE_REMOTE_LOG_URL || '',
    maxLogs: parseInt(import.meta.env.VITE_MAX_LOGS) || 1000,
    enablePerformanceLog: import.meta.env.VITE_ENABLE_PERFORMANCE_LOG === 'true'
  },

  // 安全配置
  security: {
    enableCSP: import.meta.env.VITE_ENABLE_CSP === 'true',
    enableXSRFProtection: import.meta.env.VITE_ENABLE_XSRF_PROTECTION !== 'false',
    maxRequestsPerMinute: parseInt(import.meta.env.VITE_MAX_REQUESTS_PER_MINUTE) || 60,
    enableInputSanitization: import.meta.env.VITE_ENABLE_INPUT_SANITIZATION !== 'false',
    allowedHosts: (import.meta.env.VITE_ALLOWED_HOSTS || '').split(',').filter(Boolean)
  },

  // 开发配置
  development: {
    enableMockData: import.meta.env.VITE_ENABLE_MOCK_DATA === 'true',
    enableDevtools: import.meta.env.VITE_ENABLE_DEVTOOLS !== 'false',
    enableHMR: import.meta.env.VITE_ENABLE_HMR !== 'false',
    showPerformanceMetrics: import.meta.env.VITE_SHOW_PERFORMANCE_METRICS === 'true'
  }
}

// 验证配置
const validateConfig = () => {
  const errors = []

  // 验证必需的配置
  if (!config.api.baseUrl) {
    errors.push('API基础URL未配置')
  }

  if (config.auth.passwordMinLength < 6) {
    errors.push('密码最小长度不能小于6位')
  }

  if (config.upload.maxFileSize > 100 * 1024 * 1024) {
    errors.push('最大文件大小不能超过100MB')
  }

  if (errors.length > 0) {
    console.error('配置验证失败:', errors)
    throw new Error('配置验证失败')
  }
}

// 获取配置值的辅助函数
const getConfig = (path, defaultValue = null) => {
  const keys = path.split('.')
  let value = config

  for (const key of keys) {
    if (value && typeof value === 'object' && key in value) {
      value = value[key]
    } else {
      return defaultValue
    }
  }

  return value
}

// 更新配置的辅助函数
const setConfig = (path, value) => {
  const keys = path.split('.')
  let current = config

  for (let i = 0; i < keys.length - 1; i++) {
    const key = keys[i]
    if (!(key in current) || typeof current[key] !== 'object') {
      current[key] = {}
    }
    current = current[key]
  }

  current[keys[keys.length - 1]] = value
}

// 在开发环境中验证配置
if (import.meta.env.DEV) {
  validateConfig()
}

export default config
export { getConfig, setConfig, validateConfig }