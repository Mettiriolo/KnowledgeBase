<template>
  <div id="app" class="min-h-screen bg-gray-50">
    <!-- 全局加载指示器 -->
    <div v-if="isGlobalLoading" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 shadow-xl">
        <div class="flex items-center space-x-3">
          <LoadingSpinner />
          <span class="text-gray-700">加载中...</span>
        </div>
      </div>
    </div>

    <!-- 主要内容 -->
    <router-view />

    <!-- 全局通知容器 -->
    <NotificationContainer />
  </div>
</template>
<script setup>
  import { computed, onMounted, onUnmounted } from 'vue'
  import { useAuthStore } from '@/stores/auth'
  import { useAppStore } from '@/stores/app'
  import NotificationContainer from '@/components/Common/NotificationContainer.vue'
  import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'

  const authStore = useAuthStore()
  const appStore = useAppStore()

  const isGlobalLoading = computed(() => appStore.isGlobalLoading)

  // 键盘快捷键
  const handleKeydown = (event) => {
    // Ctrl/Cmd + K 快速搜索
    if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
      event.preventDefault()
      // 触发快速搜索
    }

    // Ctrl/Cmd + N 新建笔记
    if ((event.ctrlKey || event.metaKey) && event.key === 'n') {
      event.preventDefault()
      // 跳转到新建笔记
    }
  }

  onMounted(async () => {
    // 初始化应用
    appStore.initializeApp()

    // 初始化认证状态
    await authStore.initializeAuth()

    // 注册键盘快捷键
    document.addEventListener('keydown', handleKeydown)
  })

  onUnmounted(() => {
    document.removeEventListener('keydown', handleKeydown)
  })
</script>
