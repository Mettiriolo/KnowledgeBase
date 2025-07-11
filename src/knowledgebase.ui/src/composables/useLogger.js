import { logger } from '@/utils/logger'
import { errorReporting } from '@/services/errorReporting'

export function useLogger() {
  const isDev = import.meta.env.DEV

  const logDebug = (message, data = {}) => {
    if (isDev) {
      console.log(`[DEBUG] ${message}`, data)
    }
    logger.debug(message, data)
  }

  const logInfo = (message, data = {}) => {
    if (isDev) {
      console.info(`[INFO] ${message}`, data)
    }
    logger.info(message, data)
  }

  const logWarn = (message, data = {}) => {
    if (isDev) {
      console.warn(`[WARN] ${message}`, data)
    }
    logger.warn(message, data)
  }

  const logError = (message, error = null, context = {}) => {
    if (isDev) {
      console.error(`[ERROR] ${message}`, error, context)
    }
    
    logger.error(message, { error: error?.message, stack: error?.stack, ...context })
    
    // 在生产环境中，报告错误到监控服务
    if (!isDev && error) {
      errorReporting.reportError(error, { message, ...context })
    }
  }

  const logPerformance = (metric, value, threshold) => {
    if (isDev) {
      console.log(`[PERF] ${metric}: ${value}${threshold ? ` (threshold: ${threshold})` : ''}`)
    }
    
    logger.info(`Performance metric: ${metric}`, { metric, value, threshold })
    
    // 如果超过阈值，报告性能问题
    if (threshold && value > threshold) {
      if (!isDev) {
        errorReporting.reportPerformanceIssue(metric, value, threshold)
      }
    }
  }

  const logUserAction = (action, details = {}) => {
    if (isDev) {
      console.log(`[USER] ${action}`, details)
    }
    
    logger.info(`User action: ${action}`, details)
  }

  return {
    debug: logDebug,
    info: logInfo,
    warn: logWarn,
    error: logError,
    performance: logPerformance,
    userAction: logUserAction
  }
}