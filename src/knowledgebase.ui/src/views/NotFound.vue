<template>
  <Layout>
    <div class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div class="sm:mx-auto sm:w-full sm:max-w-md">
        <div class="text-center">
          <!-- 404 图标 -->
          <div class="mx-auto flex items-center justify-center h-32 w-32 rounded-full bg-primary-100 mb-8">
            <svg class="h-16 w-16 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" 
                d="M9.172 16.172a4 4 0 015.656 0M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          
          <!-- 404 标题 -->
          <h1 class="text-6xl font-bold text-gray-900 mb-4">404</h1>
          <h2 class="text-2xl font-semibold text-gray-700 mb-4">页面未找到</h2>
          <p class="text-lg text-gray-500 mb-8 max-w-md mx-auto">
            抱歉，您访问的页面不存在。页面可能已被删除、移动或者您输入的网址有误。
          </p>
          
          <!-- 操作按钮 -->
          <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
            <router-link 
              to="/dashboard"
              class="inline-flex items-center px-6 py-3 bg-primary-600 text-white text-sm font-medium rounded-lg hover:bg-primary-700 transition-colors"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
              返回首页
            </router-link>
            
            <button 
              @click="goBack"
              class="inline-flex items-center px-6 py-3 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
              </svg>
              返回上页
            </button>
          </div>
          
          <!-- 快捷链接 -->
          <div class="mt-12">
            <h3 class="text-sm font-medium text-gray-900 mb-4">常用功能</h3>
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 max-w-lg mx-auto">
              <router-link 
                to="/notes"
                class="flex items-center justify-center px-4 py-3 text-sm text-gray-600 bg-white rounded-lg border border-gray-200 hover:bg-gray-50 hover:text-gray-900 transition-colors"
              >
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                笔记
              </router-link>
              
              <router-link 
                to="/search"
                class="flex items-center justify-center px-4 py-3 text-sm text-gray-600 bg-white rounded-lg border border-gray-200 hover:bg-gray-50 hover:text-gray-900 transition-colors"
              >
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                搜索
              </router-link>
              
              <router-link 
                to="/notes/create"
                class="flex items-center justify-center px-4 py-3 text-sm text-gray-600 bg-white rounded-lg border border-gray-200 hover:bg-gray-50 hover:text-gray-900 transition-colors"
              >
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                创建
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Layout>
</template>

<script setup>
import Layout from '@/components/Common/Layout.vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const goBack = () => {
  if (window.history.length > 1) {
    router.go(-1)
  } else {
    router.push('/dashboard')
  }
}
</script>