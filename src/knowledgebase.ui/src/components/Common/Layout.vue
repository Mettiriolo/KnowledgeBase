<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 导航栏 -->
    <nav class="bg-white shadow-sm border-b sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16">
          <!-- Logo和导航 -->
          <div class="flex items-center">
            <router-link to="/" class="flex items-center space-x-2">
              <div class="w-8 h-8 bg-primary-500 rounded-lg flex items-center justify-center">
                <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
              <span class="text-xl font-bold text-gray-900">AI知识库</span>
            </router-link>

            <!-- 桌面导航菜单 -->
            <div class="hidden md:ml-10 md:flex md:space-x-8">
              <router-link
                v-for="item in navigation"
                :key="item.name"
                :to="item.href"
                :class="[
                  'nav-link',
                  isActive(item.href) ? 'nav-link-active' : ''
                ]"
              >
                {{ item.name }}
              </router-link>
            </div>
          </div>

          <!-- 右侧菜单 -->
          <div class="flex items-center space-x-4">
            <!-- 快速搜索 -->
            <div class="hidden md:block relative">
              <input
                v-model="quickSearchQuery"
                @keyup.enter="performQuickSearch"
                placeholder="快速搜索 (Ctrl+K)"
                class="w-64 px-4 py-2 pl-10 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
              <svg class="absolute left-3 top-2.5 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>

            <!-- 创建笔记按钮 -->
            <router-link
              to="/notes/create"
              class="btn btn-primary flex items-center space-x-2"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              <span class="hidden sm:inline">新建笔记</span>
            </router-link>

            <!-- 用户菜单 -->
            <div class="relative" ref="userMenuRef">
              <button
                @click="toggleUserMenu"
                class="flex items-center space-x-2 p-2 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <div class="w-8 h-8 bg-primary-500 rounded-full flex items-center justify-center">
                  <span class="text-white text-sm font-medium">
                    {{ userInitial }}
                  </span>
                </div>
                <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                </svg>
              </button>

              <!-- 用户下拉菜单 -->
              <transition name="fade">
                <div
                  v-if="showUserMenu"
                  class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border py-1"
                >
                  <div class="px-4 py-2 border-b">
                    <p class="text-sm font-medium text-gray-900">{{ authStore.userName }}</p>
                    <p class="text-sm text-gray-500">{{ authStore.userEmail }}</p>
                  </div>
                  <router-link
                    to="/profile"
                    class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                    @click="showUserMenu = false"
                  >
                    个人设置
                  </router-link>
                  <button
                    @click="handleLogout"
                    class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    退出登录
                  </button>
                </div>
              </transition>
            </div>

            <!-- 移动端菜单按钮 -->
            <button
              @click="toggleMobileMenu"
              class="md:hidden p-2 rounded-lg hover:bg-gray-100"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- 移动端菜单 -->
      <transition name="slide">
        <div v-if="showMobileMenu" class="md:hidden border-t bg-gray-50">
          <div class="px-2 pt-2 pb-3 space-y-1">
            <router-link
              v-for="item in navigation"
              :key="item.name"
              :to="item.href"
              :class="[
                'mobile-nav-link',
                isActive(item.href) ? 'bg-primary-50 text-primary-600' : ''
              ]"
              @click="showMobileMenu = false"
            >
              {{ item.name }}
            </router-link>
          </div>
          <div class="px-4 py-3 border-t">
            <input
              v-model="quickSearchQuery"
              @keyup.enter="performQuickSearch"
              placeholder="搜索笔记..."
              class="w-full px-4 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>
        </div>
      </transition>
    </nav>

    <!-- 主要内容区域 -->
    <main class="flex-1">
      <slot />
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotificationStore } from '@/stores/notification'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const notificationStore = useNotificationStore()

const showUserMenu = ref(false)
const showMobileMenu = ref(false)
const quickSearchQuery = ref('')
const userMenuRef = ref(null)

const navigation = [
  { name: '仪表板', href: '/dashboard' },
  { name: '笔记', href: '/notes' },
  { name: '搜索', href: '/search' }
]

const userInitial = computed(() => {
  return authStore.userName?.charAt(0).toUpperCase() || 'U'
})

const isActive = (path) => {
  return route.path.startsWith(path)
}

const performQuickSearch = () => {
  if (quickSearchQuery.value.trim()) {
    router.push({
      name: 'Search',
      query: { q: quickSearchQuery.value }
    })
    quickSearchQuery.value = ''
    showMobileMenu.value = false
  }
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
}

const toggleMobileMenu = () => {
  showMobileMenu.value = !showMobileMenu.value
}

const handleLogout = async () => {
  try {
    await authStore.logout()
    notificationStore.success('已成功退出登录')
    showUserMenu.value = false
  } catch (error) {
    notificationStore.error('退出登录失败')
  }
}

// 点击外部关闭用户菜单
const handleClickOutside = (event) => {
  if (userMenuRef.value && !userMenuRef.value.contains(event.target)) {
    showUserMenu.value = false
  }
}

// 键盘快捷键
const handleKeydown = (event) => {
  if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
    event.preventDefault()
    const searchInput = document.querySelector('input[placeholder*="快速搜索"]')
    if (searchInput) searchInput.focus()
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  document.removeEventListener('keydown', handleKeydown)
})
</script>

<style scoped>
@import "tailwindcss";
@config "../../../tailwind.config.js";

.nav-link {
  @apply text-gray-500 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium transition-colors;
}

.nav-link-active {
  @apply text-primary-600 bg-primary-50;
}

.mobile-nav-link {
  @apply text-gray-500 hover:text-gray-900 hover:bg-gray-100 block px-3 py-2 rounded-md text-base font-medium;
}
</style>
