// 错误监控和报告服务
class ErrorReportingService {
  constructor() {
    this.isProduction = import.meta.env.PROD
    this.apiEndpoint = import.meta.env.VITE_ERROR_REPORTING_ENDPOINT
    this.enabled = this.isProduction && this.apiEndpoint
  }

  // 发送错误到监控服务
  async sendToService(error, context = {}) {
    if (!this.enabled) return

    try {
      const errorData = {
        message: error.message,
        stack: error.stack,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        url: window.location.href,
        userId: context.userId || 'anonymous',
        sessionId: this.getSessionId(),
        context: {
          ...context,
          appVersion: import.meta.env.VITE_APP_VERSION || 'unknown',
          environment: import.meta.env.MODE
        }
      }

      await fetch(this.apiEndpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(errorData)
      })
    } catch (reportingError) {
      // 静默失败，不要因为错误报告本身失败而影响用户体验
      console.error('Error reporting failed:', reportingError)
    }
  }

  // 获取会话ID
  getSessionId() {
    let sessionId = sessionStorage.getItem('sessionId')
    if (!sessionId) {
      sessionId = 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
      sessionStorage.setItem('sessionId', sessionId)
    }
    return sessionId
  }

  // 报告JavaScript错误
  reportError(error, context = {}) {
    // 开发环境继续使用console.error
    if (!this.isProduction) {
      console.error('Error:', error, context)
      return
    }

    // 生产环境发送到监控服务
    this.sendToService(error, context)
  }

  // 报告API错误
  reportAPIError(error, apiContext = {}) {
    const context = {
      type: 'api_error',
      endpoint: apiContext.url,
      method: apiContext.method,
      status: apiContext.status,
      ...apiContext
    }
    
    this.reportError(error, context)
  }

  // 报告用户操作错误
  reportUserActionError(error, action, details = {}) {
    const context = {
      type: 'user_action_error',
      action,
      ...details
    }
    
    this.reportError(error, context)
  }

  // 报告性能问题
  reportPerformanceIssue(metric, value, threshold) {
    if (!this.enabled) return

    const context = {
      type: 'performance_issue',
      metric,
      value,
      threshold,
      timestamp: new Date().toISOString()
    }

    this.sendToService(new Error(`Performance issue: ${metric} (${value}) exceeded threshold (${threshold})`), context)
  }
}

// 创建全局实例
export const errorReporting = new ErrorReportingService()

// 设置全局错误处理
export const setupGlobalErrorHandling = () => {
  // 捕获未处理的Promise rejection
  window.addEventListener('unhandledrejection', (event) => {
    errorReporting.reportError(event.reason, {
      type: 'unhandled_promise_rejection'
    })
  })

  // 捕获全局JavaScript错误
  window.addEventListener('error', (event) => {
    errorReporting.reportError(event.error || new Error(event.message), {
      type: 'global_javascript_error',
      filename: event.filename,
      lineno: event.lineno,
      colno: event.colno
    })
  })

  // 捕获资源加载错误
  window.addEventListener('error', (event) => {
    if (event.target !== window) {
      errorReporting.reportError(new Error(`Resource loading failed: ${event.target.src || event.target.href}`), {
        type: 'resource_loading_error',
        element: event.target.tagName,
        source: event.target.src || event.target.href
      })
    }
  }, true)
}

export default errorReporting