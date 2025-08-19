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

            <!-- 桌面导航菜单 - 精简 -->
            <div class="hidden lg:ml-10 lg:flex lg:space-x-6">
              <router-link
                v-for="item in navigation"
                :key="item.name"
                :to="item.href"
                :class="[
                  'nav-link',
                  isActive(item.href) ? 'nav-link-active' : ''
                ]"
              >
                <span class="flex items-center space-x-1">
                  <component :is="item.icon" class="w-4 h-4" />
                  <span class="hidden xl:inline">{{ item.name }}</span>
                </span>
              </router-link>
            </div>
          </div>

          <!-- 右侧菜单 - 移动端优化 -->
          <div class="flex items-center space-x-2 sm:space-x-3">
            <!-- 快速搜索 - 桌面端 -->
            <div class="hidden lg:block relative">
              <input
                v-model="quickSearchQuery"
                @keyup.enter="performQuickSearch"
                placeholder="搜索... (Ctrl+K)"
                class="w-48 xl:w-64 px-3 py-2 pl-9 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
              <svg class="absolute left-3 top-2.5 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            
            <!-- 快速搜索按钮 - 移动端 -->
            <router-link 
              to="/search"
              class="lg:hidden p-2 text-gray-400 hover:text-gray-600 transition-colors"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </router-link>

            <!-- 创建笔记按钮 -->
            <router-link
              to="/notes/create"
              class="inline-flex items-center px-3 sm:px-4 py-2 bg-primary-600 text-white text-sm font-medium rounded-lg hover:bg-primary-700 transition-colors"
            >
              <svg class="w-4 h-4 sm:mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              <span class="hidden sm:inline">新笔记</span>
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
                    to="/settings"
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
              class="lg:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors"
            >
              <svg 
                class="w-5 h-5 transition-transform" 
                :class="{ 'rotate-90': showMobileMenu }"
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path v-if="!showMobileMenu" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- 移动端菜单 - 优化 -->
      <transition 
        enter-active-class="transition-all duration-200 ease-out" 
        enter-from-class="opacity-0 -translate-y-2" 
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 -translate-y-2"
      >
        <div v-if="showMobileMenu" class="lg:hidden border-t bg-white shadow-lg">
          <!-- 导航菜单 -->
          <div class="px-4 py-4 space-y-2">
            <router-link
              v-for="item in navigation"
              :key="item.name"
              :to="item.href"
              :class="[
                'flex items-center space-x-3 px-4 py-3 rounded-xl text-base font-medium transition-all',
                isActive(item.href) 
                  ? 'bg-primary-50 text-primary-600 border border-primary-200' 
                  : 'text-gray-700 hover:bg-gray-50 hover:text-gray-900'
              ]"
              @click="showMobileMenu = false"
            >
              <component :is="item.icon" class="w-5 h-5" />
              <span>{{ item.name }}</span>
            </router-link>
          </div>
          
          <!-- 快速操作 -->
          <div class="px-4 py-4 border-t border-gray-100 bg-gray-50">
            <div class="space-y-3">
              <button
                @click="$router.push('/search'); showMobileMenu = false"
                class="w-full flex items-center space-x-3 px-4 py-3 bg-white border border-gray-200 rounded-xl text-left hover:bg-gray-50 transition-colors"
              >
                <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                <span class="text-gray-700">搜索所有笔记</span>
              </button>
              
              <button
                @click="$router.push('/notes/create'); showMobileMenu = false"
                class="w-full flex items-center space-x-3 px-4 py-3 bg-primary-600 text-white rounded-xl hover:bg-primary-700 transition-colors"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                <span class="font-medium">创建新笔记</span>
              </button>
            </div>
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

// 导航图标组件
const DashboardIcon = { template: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5a2 2 0 012-2h4a2 2 0 012 2v3H8V5z" /></svg>' }
const NotesIcon = { template: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>' }
const SearchIcon = { template: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>' }

const navigation = [
  { name: '仪表板', href: '/dashboard', icon: DashboardIcon },
  { name: '笔记', href: '/notes', icon: NotesIcon },
  { name: '搜索', href: '/search', icon: SearchIcon }
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
    showUserMenu.value = false
  } catch (error) {
    // Auth store handles success notifications, errors are logged in store
    console.error('Logout error:', error)
    showUserMenu.value = false
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
    const searchInput = document.querySelector('input[placeholder*="搜索"]')
    if (searchInput) {
      searchInput.focus()
    } else {
      // 如果搜索框不可见（移动端），导航到搜索页面
      router.push('/search')
    }
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
  @apply text-gray-500 hover:text-gray-900 px-3 py-2 rounded-lg text-sm font-medium transition-all duration-200 hover:bg-gray-50;
}

.nav-link-active {
  @apply text-primary-600 bg-primary-50 border border-primary-200;
}
</style>
