import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import './style.css'
import toastui from './plugins/toastui'
import { errorHandler, logger } from './utils/logger'
import { setupGlobalErrorHandling } from './services/errorReporting'

const app = createApp(App)
const pinia = createPinia()

// 设置全局错误监控
setupGlobalErrorHandling()

// 全局错误处理
app.config.errorHandler = (err, instance, info) => {
  errorHandler.wrapVueError(err, instance, info)
}

// 全局属性
app.config.globalProperties.$appName = import.meta.env.VITE_APP_NAME || 'AI Knowledge Base'

// 性能监控
if (import.meta.env.DEV) {
  app.config.performance = true
}

app.use(pinia)
app.use(router)
app.use(toastui)

// 应用挂载
app.mount('#app')

// 开发环境下暴露调试工具
if (import.meta.env.DEV) {
  window.__APP__ = app
  window.__LOGGER__ = logger
  window.__ERROR_HANDLER__ = errorHandler
}
