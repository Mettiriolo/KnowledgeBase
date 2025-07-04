import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import './style.css'
import toastui from './plugins/toastui'

const app = createApp(App)
const pinia = createPinia()

// 全局错误处理
app.config.errorHandler = (err, instance, info) => {
  console.error('Global error:', err)
  console.error('Error info:', info)
  // 这里可以添加错误上报逻辑
}

// 全局属性
app.config.globalProperties.$appName = import.meta.env.VITE_APP_NAME || 'AI Knowledge Base'

app.use(pinia)
app.use(router)
app.use(toastui)

app.mount('#app')
