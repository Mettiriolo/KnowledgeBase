<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="max-w-md w-full bg-white rounded-lg shadow-lg p-6">
      <div class="text-center">
        <div class="mb-4">
          <svg class="mx-auto h-16 w-16 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.5 0L4.732 15.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
        </div>
        
        <h1 class="text-2xl font-bold text-gray-900 mb-2">
          {{ error.title }}
        </h1>
        
        <p class="text-gray-600 mb-6">
          {{ error.message }}
        </p>
        
        <div class="space-y-3">
          <button
            @click="retry"
            class="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition-colors"
          >
            重试
          </button>
          
          <button
            @click="goHome"
            class="w-full bg-gray-200 text-gray-800 py-2 px-4 rounded-md hover:bg-gray-300 transition-colors"
          >
            返回首页
          </button>
          
          <button
            @click="showDetails = !showDetails"
            class="w-full text-gray-600 hover:text-gray-800 py-2 px-4 transition-colors"
          >
            {{ showDetails ? '隐藏' : '显示' }}详细信息
          </button>
        </div>
        
        <div v-if="showDetails" class="mt-6 p-4 bg-gray-100 rounded-md text-left">
          <div class="text-sm text-gray-700">
            <div class="mb-2">
              <span class="font-semibold">错误类型:</span>
              {{ error.type }}
            </div>
            <div class="mb-2">
              <span class="font-semibold">发生时间:</span>
              {{ error.timestamp }}
            </div>
            <div class="mb-2">
              <span class="font-semibold">页面路径:</span>
              {{ error.path }}
            </div>
            <div v-if="error.stack" class="mb-2">
              <span class="font-semibold">堆栈信息:</span>
              <pre class="mt-1 text-xs bg-gray-200 p-2 rounded overflow-x-auto">{{ error.stack }}</pre>
            </div>
            <div class="flex space-x-2 mt-4">
              <button
                @click="copyError"
                class="px-3 py-1 bg-blue-500 text-white text-xs rounded hover:bg-blue-600"
              >
                复制错误信息
              </button>
              <button
                @click="reportError"
                class="px-3 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600"
              >
                报告错误
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useNotificationStore } from '@/stores/notification'
import { logger, errorHandler } from '@/utils/logger'

const props = defineProps({
  error: {
    type: Error,
    required: true
  },
  errorInfo: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['retry'])

const router = useRouter()
const notificationStore = useNotificationStore()
const showDetails = ref(false)

const error = computed(() => {
  const baseError = {
    title: '出现了一个错误',
    message: '很抱歉，应用程序遇到了一个错误。请尝试刷新页面或返回首页。',
    type: props.error.constructor.name,
    timestamp: new Date().toLocaleString(),
    path: window.location.pathname,
    stack: props.error.stack
  }

  // 根据错误类型定制消息
  if (props.error.name === 'ChunkLoadError') {
    return {
      ...baseError,
      title: '加载失败',
      message: '应用程序更新了，请刷新页面以获取最新版本。'
    }
  }

  if (props.error.name === 'NetworkError') {
    return {
      ...baseError,
      title: '网络错误',
      message: '网络连接出现问题，请检查您的网络连接并重试。'
    }
  }

  if (props.error.message.includes('Loading chunk')) {
    return {
      ...baseError,
      title: '资源加载失败',
      message: '页面资源加载失败，请刷新页面重试。'
    }
  }

  return baseError
})

const retry = () => {
  emit('retry')
  window.location.reload()
}

const goHome = () => {
  router.push('/').catch(() => {
    window.location.href = '/'
  })
}

const copyError = async () => {
  try {
    const errorText = JSON.stringify({
      message: props.error.message,
      stack: props.error.stack,
      type: props.error.constructor.name,
      timestamp: new Date().toISOString(),
      path: window.location.pathname,
      userAgent: navigator.userAgent
    }, null, 2)

    await navigator.clipboard.writeText(errorText)
    notificationStore.success('复制成功', '错误信息已复制到剪贴板')
  } catch (err) {
    notificationStore.error('复制失败', '无法复制错误信息')
  }
}

const reportError = () => {
  errorHandler.captureException(props.error, {
    ...props.errorInfo,
    reported: true,
    reportedAt: new Date().toISOString()
  })
  
  notificationStore.success('报告已发送', '错误报告已发送，感谢您的反馈')
}

// 记录错误到日志
logger.error('Error boundary caught error', {
  error: props.error.message,
  stack: props.error.stack,
  errorInfo: props.errorInfo
})
</script>