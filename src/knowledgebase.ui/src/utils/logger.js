const LOG_LEVELS = {
  ERROR: 0,
  WARN: 1,
  INFO: 2,
  DEBUG: 3
}

class Logger {
  constructor() {
    this.level = import.meta.env.PROD ? LOG_LEVELS.ERROR : LOG_LEVELS.DEBUG
    this.logs = []
    this.maxLogs = 1000
  }

  setLevel(level) {
    this.level = level
  }

  log(level, message, data = null) {
    if (level > this.level) return

    const timestamp = new Date().toISOString()
    const logEntry = {
      timestamp,
      level: Object.keys(LOG_LEVELS)[level],
      message,
      data,
      stack: new Error().stack
    }

    this.logs.push(logEntry)
    
    if (this.logs.length > this.maxLogs) {
      this.logs.shift()
    }

    this.output(logEntry)
  }

  output(logEntry) {
    const { level, message, data, timestamp } = logEntry
    const prefix = `[${timestamp}] [${level}]`
    
    switch (level) {
      case 'ERROR':
        console.error(prefix, message, data)
        break
      case 'WARN':
        console.warn(prefix, message, data)
        break
      case 'INFO':
        console.info(prefix, message, data)
        break
      case 'DEBUG':
        console.debug(prefix, message, data)
        break
      default:
        console.log(prefix, message, data)
    }
  }

  error(message, data) {
    this.log(LOG_LEVELS.ERROR, message, data)
  }

  warn(message, data) {
    this.log(LOG_LEVELS.WARN, message, data)
  }

  info(message, data) {
    this.log(LOG_LEVELS.INFO, message, data)
  }

  debug(message, data) {
    this.log(LOG_LEVELS.DEBUG, message, data)
  }

  getLogs(level = null) {
    if (level === null) return this.logs
    return this.logs.filter(log => log.level === Object.keys(LOG_LEVELS)[level])
  }

  clearLogs() {
    this.logs = []
  }

  export() {
    const blob = new Blob([JSON.stringify(this.logs, null, 2)], { 
      type: 'application/json' 
    })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `logs-${new Date().toISOString()}.json`
    a.click()
    URL.revokeObjectURL(url)
  }
}

export const logger = new Logger()

export const errorHandler = {
  captureException: (error, context = {}) => {
    logger.error('Exception captured', {
      error: error.message,
      stack: error.stack,
      context,
      url: window.location.href,
      userAgent: navigator.userAgent,
      timestamp: new Date().toISOString()
    })

    if (import.meta.env.PROD) {
      // 在生产环境中，可以发送到错误监控服务
      // sendToErrorService(error, context)
    }
  },

  captureMessage: (message, level = 'info', context = {}) => {
    logger[level](message, context)
  },

  wrapAsync: (fn) => {
    return async (...args) => {
      try {
        return await fn(...args)
      } catch (error) {
        errorHandler.captureException(error, { 
          function: fn.name,
          args: args.slice(0, 3) // 只记录前3个参数避免日志过大
        })
        throw error
      }
    }
  },

  wrapVueError: (error, vm, info) => {
    logger.error('Vue error captured', {
      error: error.message,
      stack: error.stack,
      component: vm?.$options.name || 'Unknown',
      info,
      props: vm?.$props,
      route: vm?.$route?.path
    })
  }
}

export const performance = {
  mark: (name) => {
    if (window.performance && window.performance.mark) {
      window.performance.mark(name)
    }
  },

  measure: (name, startMark, endMark) => {
    if (window.performance && window.performance.measure) {
      try {
        window.performance.measure(name, startMark, endMark)
        const measure = window.performance.getEntriesByName(name)[0]
        logger.debug(`Performance: ${name}`, {
          duration: measure.duration,
          startTime: measure.startTime
        })
        return measure.duration
      } catch (error) {
        logger.warn('Performance measurement failed', { name, error })
      }
    }
  },

  time: (label) => {
    const start = performance.now()
    return () => {
      const duration = performance.now() - start
      logger.debug(`Timer: ${label}`, { duration })
      return duration
    }
  }
}

export const analytics = {
  track: (event, properties = {}) => {
    logger.info('Analytics event', { event, properties })
    
    // 在生产环境中，发送到分析服务
    if (import.meta.env.PROD) {
      try {
        // 这里可以集成第三方分析服务，如Google Analytics, Mixpanel等
        // 暂时保留给未来集成
      } catch (error) {
        console.error('Analytics reporting failed:', error)
      }
    }
  },

  pageView: (page) => {
    analytics.track('page_view', { page })
  },

  userAction: (action, target) => {
    analytics.track('user_action', { action, target })
  }
}

export const createContextualLogger = (context) => {
  return {
    error: (message, data) => logger.error(message, { ...context, ...data }),
    warn: (message, data) => logger.warn(message, { ...context, ...data }),
    info: (message, data) => logger.info(message, { ...context, ...data }),
    debug: (message, data) => logger.debug(message, { ...context, ...data })
  }
}

// 全局错误处理
if (typeof window !== 'undefined') {
  window.addEventListener('error', (event) => {
    errorHandler.captureException(event.error, {
      filename: event.filename,
      lineno: event.lineno,
      colno: event.colno
    })
  })

  window.addEventListener('unhandledrejection', (event) => {
    errorHandler.captureException(event.reason, {
      type: 'unhandledrejection'
    })
  })
}

export default {
  logger,
  errorHandler,
  performance,
  analytics,
  createContextualLogger
}