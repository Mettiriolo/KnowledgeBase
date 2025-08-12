<template>
  <Layout>
    <div class="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50/30 to-indigo-50/50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 sm:py-12">
        <!-- 欢迎区域 - 重新设计 -->
        <div class="relative mb-12">
          <div class="bg-gradient-to-r from-blue-600 via-purple-600 to-indigo-600 rounded-3xl p-8 sm:p-12 text-white overflow-hidden">
            <div class="absolute inset-0 bg-gradient-to-r from-blue-600/90 via-purple-600/90 to-indigo-600/90"></div>
            <div class="absolute top-4 right-4 opacity-20">
              <svg class="w-32 h-32" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
              </svg>
            </div>
            <div class="relative">
              <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between">
                <div class="mb-6 lg:mb-0">
                  <h1 class="text-3xl sm:text-4xl lg:text-5xl font-bold mb-4 bg-gradient-to-r from-white to-blue-100 bg-clip-text">
                    {{ getGreeting() }}，{{ authStore.userName }}！
                  </h1>
                  <p class="text-xl text-blue-100 mb-2">{{ getMotivationalMessage() }}</p>
                  <div class="flex items-center text-blue-200 text-sm">
                    <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3a2 2 0 012-2h4a2 2 0 012 2v4m-6 0V7a2 2 0 012-2h4a2 2 0 012 2v0M8 7v8a2 2 0 002 2h4a2 2 0 002-2V7M8 7H6a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V9a2 2 0 00-2-2h-2" />
                    </svg>
                    {{ getCurrentDate() }}
                  </div>
                </div>
                <div class="flex flex-col items-end space-y-4">
                  <div class="flex items-center bg-white/10 backdrop-blur-sm rounded-2xl px-6 py-3">
                    <div class="text-right">
                      <div class="text-sm text-blue-100">{{ getWeatherInfo() }}</div>
                      <div class="text-2xl font-bold">{{ new Date().getHours() }}:{{ String(new Date().getMinutes()).padStart(2, '0') }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 数据概览仪表盘 - 改进版 -->
        <div class="grid grid-cols-2 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6 gap-6 mb-12">
          <div 
            v-for="(stat, index) in enhancedStats" 
            :key="stat.title"
            class="group bg-white/80 backdrop-blur-sm rounded-2xl p-6 shadow-lg border border-white/50 hover:shadow-2xl hover:bg-white transition-all duration-500 cursor-pointer transform hover:-translate-y-2"
            :class="stat.bgClass"
            @click="handleStatClick(stat)"
          >
            <div class="flex items-center justify-between mb-4">
              <div :class="stat.iconBg" class="p-3 rounded-xl group-hover:scale-110 transition-transform">
                <component :is="stat.icon" :class="stat.iconColor" class="w-6 h-6" />
              </div>
              <div v-if="stat.trend" class="flex items-center space-x-1 text-xs">
                <svg v-if="stat.trend > 0" class="w-3 h-3 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 17l9.2-9.2M17 17V7H7" />
                </svg>
                <svg v-else class="w-3 h-3 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 7l-9.2 9.2M7 7v10h10" />
                </svg>
                <span :class="stat.trend > 0 ? 'text-green-600' : 'text-red-600'" class="font-medium">
                  {{ Math.abs(stat.trend) }}%
                </span>
              </div>
            </div>
            <div class="space-y-1">
              <div class="text-2xl font-bold text-gray-900 group-hover:text-primary-600 transition-colors">
                {{ stat.value }}
              </div>
              <div class="text-sm font-medium text-gray-600">{{ stat.title }}</div>
              <div v-if="stat.subtitle" class="text-xs text-gray-500">{{ stat.subtitle }}</div>
            </div>
          </div>
        </div>

        <!-- 智能工作台 -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
          <!-- 快速操作区 -->
          <div class="lg:col-span-2 space-y-6">
            <!-- 快速搜索 -->
            <div class="bg-white rounded-2xl shadow-sm border p-6">
              <div class="flex items-center justify-between mb-4">
                <h2 class="text-lg font-semibold text-gray-900">快速搜索</h2>
                <div class="flex items-center space-x-2">
                  <span class="px-2 py-1 bg-gradient-to-r from-primary-50 to-purple-50 text-primary-700 text-xs font-medium rounded-full">
                    AI助力
                  </span>
                </div>
              </div>
              <div class="relative">
                <input
                  v-model="quickSearchQuery"
                  @keyup.enter="performQuickSearch"
                  @focus="showSearchSuggestions = true"
                  type="text"
                  placeholder="试试'上周的会议记录'或'关于JavaScript的笔记'..."
                  class="w-full pl-12 pr-20 py-4 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-gray-50 focus:bg-white transition-colors"
                />
                <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <svg class="h-5 w-5 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <button
                  @click="performQuickSearch"
                  :disabled="!quickSearchQuery.trim()"
                  class="absolute inset-y-0 right-0 pr-3 flex items-center px-4 bg-primary-600 text-white rounded-r-xl hover:bg-primary-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
                >
                  搜索
                </button>
              </div>
              
              <!-- 搜索建议 -->
              <div v-if="showSearchSuggestions" class="mt-4 grid grid-cols-2 gap-2">
                <button
                  v-for="suggestion in searchSuggestions"
                  :key="suggestion"
                  @click="quickSearchQuery = suggestion; performQuickSearch()"
                  class="text-left p-2 text-sm text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                >
                  {{ suggestion }}
                </button>
              </div>
            </div>
            
            <!-- 快速操作卡片 -->
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              <router-link
                to="/notes/create"
                class="group bg-gradient-to-br from-primary-500 to-primary-600 text-white rounded-2xl p-6 hover:from-primary-600 hover:to-primary-700 transition-all duration-300 transform hover:scale-[1.02] hover:shadow-xl"
              >
                <div class="flex items-center justify-between mb-3">
                  <div class="p-3 bg-white/20 rounded-xl group-hover:bg-white/30 transition-colors">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                    </svg>
                  </div>
                  <svg class="w-5 h-5 opacity-60 group-hover:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                  </svg>
                </div>
                <div>
                  <h3 class="font-bold mb-1">创建笔记</h3>
                  <p class="text-sm text-white/80">记录您的灵感和想法</p>
                </div>
              </router-link>

              <button
                @click="showAIChat = true"
                class="group bg-gradient-to-br from-purple-500 to-purple-600 text-white rounded-2xl p-6 hover:from-purple-600 hover:to-purple-700 transition-all duration-300 transform hover:scale-[1.02] hover:shadow-xl"
              >
                <div class="flex items-center justify-between mb-3">
                  <div class="p-3 bg-white/20 rounded-xl group-hover:bg-white/30 transition-colors">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                  </div>
                  <div class="w-2 h-2 bg-green-400 rounded-full animate-pulse"></div>
                </div>
                <div>
                  <h3 class="font-bold mb-1">AI助手</h3>
                  <p class="text-sm text-white/80">智能问答和建议</p>
                </div>
              </button>
            </div>
            
            <!-- 智能推荐 -->
            <div class="bg-white rounded-2xl shadow-sm border p-6">
              <div class="flex items-center justify-between mb-4">
                <h2 class="text-lg font-semibold text-gray-900">为您推荐</h2>
                <button @click="refreshRecommendations" class="text-primary-600 hover:text-primary-800 text-sm font-medium">
                  刷新
                </button>
              </div>
              <div class="space-y-3">
                <div v-for="recommendation in recommendations" :key="recommendation.id" 
                     class="p-4 bg-gradient-to-r from-gray-50 to-primary-50/30 rounded-xl hover:from-primary-50/50 hover:to-purple-50/50 transition-all cursor-pointer group"
                     @click="handleRecommendationClick(recommendation)">
                  <div class="flex items-start space-x-3">
                    <div class="flex-shrink-0">
                      <div :class="recommendation.iconBg" class="p-2 rounded-lg">
                        <component :is="recommendation.icon" :class="recommendation.iconColor" class="w-4 h-4" />
                      </div>
                    </div>
                    <div class="flex-1 min-w-0">
                      <h3 class="font-medium text-gray-900 group-hover:text-primary-700 transition-colors">{{ recommendation.title }}</h3>
                      <p class="text-sm text-gray-600 mt-1">{{ recommendation.description }}</p>
                    </div>
                    <svg class="w-4 h-4 text-gray-400 group-hover:text-primary-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- 右侧信息栏 -->
          <div class="space-y-6">
            <!-- 今日活动 -->
            <div class="bg-white rounded-2xl shadow-sm border p-6">
              <h3 class="font-semibold text-gray-900 mb-4">今日活动</h3>
              <div class="space-y-4">
                <div v-for="activity in todayActivities" :key="activity.id" class="flex items-center space-x-3">
                  <div :class="activity.dotColor" class="w-2 h-2 rounded-full"></div>
                  <div class="flex-1">
                    <p class="text-sm font-medium text-gray-900">{{ activity.action }}</p>
                    <p class="text-xs text-gray-500">{{ activity.time }}</p>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- 快捷访问 -->
            <div class="bg-white rounded-2xl shadow-sm border p-6">
              <h3 class="font-semibold text-gray-900 mb-4">快捷访问</h3>
              <div class="space-y-3">
                <!-- 热门标签 -->
                <div v-if="popularTags.length > 0">
                  <p class="text-sm text-gray-600 mb-2">热门标签</p>
                  <div class="flex flex-wrap gap-2">
                    <button
                      v-for="tag in popularTags.slice(0, 6)"
                      :key="tag.id"
                      @click="searchByTag(tag.name)"
                      :style="{ backgroundColor: tag.color + '20', color: tag.color }"
                      class="px-3 py-1 rounded-full text-xs font-medium hover:opacity-80 transition-all hover:scale-105"
                    >
                      {{ tag.name }}
                    </button>
                  </div>
                </div>
                
                <!-- 最近搜索 -->
                <div v-if="searchStore.recentSearches.length > 0" class="pt-3 border-t border-gray-100">
                  <p class="text-sm text-gray-600 mb-2">最近搜索</p>
                  <div class="space-y-1">
                    <button
                      v-for="(query, index) in searchStore.recentSearches.slice(0, 3)"
                      :key="index"
                      @click="performSearch(query)"
                      class="block w-full text-left p-2 text-sm text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
                    >
                      <div class="flex items-center space-x-2">
                        <svg class="w-3 h-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <span class="truncate">{{ query }}</span>
                      </div>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- 数据洞察 -->
            <div class="bg-gradient-to-br from-indigo-50 to-purple-50 rounded-2xl p-6 border">
              <h3 class="font-semibold text-gray-900 mb-4">数据洞察</h3>
              <div class="space-y-4">
                <div class="flex items-center justify-between">
                  <span class="text-sm text-gray-600">本周写作时间</span>
                  <span class="font-semibold text-indigo-600">{{ weeklyWritingTime }}</span>
                </div>
                <div class="flex items-center justify-between">
                  <span class="text-sm text-gray-600">最佳写作时段</span>
                  <span class="font-semibold text-purple-600">{{ bestWritingTime }}</span>
                </div>
                <div class="flex items-center justify-between">
                  <span class="text-sm text-gray-600">平均笔记长度</span>
                  <span class="font-semibold text-pink-600">{{ averageNoteLength }} 字</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 最近笔记区域 -->
        <div class="bg-white rounded-2xl shadow-sm border">
          <div class="p-6 border-b border-gray-100">
            <div class="flex items-center justify-between">
              <h2 class="text-xl font-bold text-gray-900">最近笔记</h2>
              <div class="flex items-center space-x-3">
                <select v-model="notesFilter" class="text-sm border-gray-300 rounded-lg focus:ring-primary-500 focus:border-primary-500">
                  <option value="all">所有笔记</option>
                  <option value="today">今天</option>
                  <option value="week">本周</option>
                  <option value="month">本月</option>
                </select>
                <router-link
                  to="/notes"
                  class="text-sm text-primary-600 hover:text-primary-800 font-medium flex items-center space-x-1"
                >
                  <span>查看全部</span>
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                  </svg>
                </router-link>
              </div>
            </div>
          </div>

          <!-- 笔记列表 -->
          <div v-if="recentNotesLoading" class="p-12 text-center">
            <LoadingSpinner />
            <p class="mt-3 text-sm text-gray-500">加载中...</p>
          </div>
          
          <div v-else-if="filteredRecentNotes.length === 0" class="p-12 text-center">
            <div class="w-16 h-16 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
              <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <h3 class="text-lg font-medium text-gray-900 mb-2">暂无笔记</h3>
            <p class="text-gray-500 mb-4">开始创建您的第一篇笔记吧</p>
            <router-link to="/notes/create" class="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              创建笔记
            </router-link>
          </div>
          
          <div v-else class="divide-y divide-gray-100">
            <div
              v-for="note in filteredRecentNotes.slice(0, 6)"
              :key="note.id"
              class="p-6 hover:bg-gray-50 cursor-pointer transition-all group"
              @click="viewNote(note.id)"
            >
              <div class="flex items-start justify-between">
                <div class="flex-1 min-w-0">
                  <div class="flex items-center space-x-2 mb-2">
                    <h3 class="text-base font-semibold text-gray-900 truncate group-hover:text-primary-600 transition-colors">
                      {{ note.title }}
                    </h3>
                    <span v-if="isNewNote(note)" class="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded-full">
                      新
                    </span>
                  </div>
                  <p class="text-sm text-gray-600 line-clamp-2 mb-3">
                    {{ getPlainText(note.content) }}
                  </p>
                  <div class="flex items-center justify-between">
                    <div class="flex items-center space-x-2">
                      <div class="flex flex-wrap gap-1">
                        <span
                          v-for="tag in note.tags.slice(0, 3)"
                          :key="tag.id"
                          :style="{ backgroundColor: tag.color + '15', color: tag.color }"
                          class="px-2 py-1 rounded-full text-xs font-medium"
                        >
                          {{ tag.name }}
                        </span>
                        <span v-if="note.tags.length > 3" class="px-2 py-1 bg-gray-100 text-gray-600 text-xs rounded-full">
                          +{{ note.tags.length - 3 }}
                        </span>
                      </div>
                    </div>
                    <div class="text-xs text-gray-400 flex items-center space-x-3">
                      <span>{{ getWordCount(note.content) }} 字</span>
                      <span>•</span>
                      <span>{{ formatRelativeTime(note.updatedAt) }}</span>
                    </div>
                  </div>
                </div>
                <div class="ml-4 opacity-0 group-hover:opacity-100 transition-opacity">
                  <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- AI对话模态框 -->
      <teleport to="body">
        <div v-if="showAIChat" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showAIChat = false">
          <div class="bg-white rounded-2xl w-full max-w-md max-h-96 overflow-hidden">
            <div class="p-4 border-b border-gray-200 flex items-center justify-between">
              <h3 class="font-semibold text-gray-900">AI助手</h3>
              <button @click="showAIChat = false" class="text-gray-400 hover:text-gray-600">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            <div class="p-4 space-y-4">
              <div v-if="aiStore.lastResponse" class="bg-gray-50 p-3 rounded-lg text-sm">
                <p class="text-gray-700 whitespace-pre-wrap">{{ aiStore.lastResponse.answer }}</p>
              </div>
              
              <div class="flex space-x-2">
                <input
                  v-model="aiQuestion"
                  @keyup.enter="askAI"
                  placeholder="问问关于您笔记的问题..."
                  class="flex-1 text-sm border-gray-300 rounded-lg focus:ring-primary-500 focus:border-primary-500"
                  :disabled="aiStore.isThinking"
                />
                <button
                  @click="askAI"
                  :disabled="!aiQuestion.trim() || aiStore.isThinking"
                  class="px-3 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50 text-sm"
                >
                  <LoadingSpinner v-if="aiStore.isThinking" size="small" color="white" />
                  <span v-else>发送</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </teleport>
    </div>
  </Layout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotesStore } from '@/stores/notes'
import { useSearchStore } from '@/stores/search'
import { useAIStore } from '@/stores/ai'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import { formatRelativeTime } from '@/utils/date'
import { getPlainText } from '@/utils/text'

const router = useRouter()
const authStore = useAuthStore()
const notesStore = useNotesStore()
const searchStore = useSearchStore()
const aiStore = useAIStore()
const notificationStore = useNotificationStore()

const recentNotesLoading = ref(false)
const showAIChat = ref(false)
const aiQuestion = ref('')
const quickSearchQuery = ref('')
const showSearchSuggestions = ref(false)
const notesFilter = ref('all')

// 图标组件
const DocumentTextIcon = { template: '<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>' }
const TagIcon = { template: '<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" /></svg>' }
const PlusCircleIcon = { template: '<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v3m0 0v3m0-3h3m-3 0H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>' }
const ChartBarIcon = { template: '<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" /></svg>' }
const ClockIcon = { template: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>' }
const ShareIcon = { template: '<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.367 2.684 3 3 0 00-5.367-2.684z" /></svg>' }

// 缓存日期计算结果
const todayString = computed(() => new Date().toDateString())
const weekAgo = computed(() => new Date(Date.now() - 7 * 24 * 60 * 60 * 1000))

// 增强的统计数据
const enhancedStats = computed(() => {
  const notes = notesStore.notes
  const todayStr = todayString.value
  const weekAgoDate = weekAgo.value
  const monthAgoDate = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)

  const todayNotes = notes.filter(note => 
    new Date(note.createdAt).toDateString() === todayStr
  ).length

  const weeklyActive = notes.filter(note => 
    new Date(note.updatedAt) >= weekAgoDate
  ).length

  const monthlyNotes = notes.filter(note => 
    new Date(note.createdAt) >= monthAgoDate
  ).length

  return [
    {
      title: '总笔记数',
      value: notes.length,
      subtitle: `本月新增 ${monthlyNotes} 篇`,
      icon: 'DocumentTextIcon',
      iconBg: 'bg-blue-100',
      iconColor: 'text-blue-600',
      bgClass: 'hover:bg-blue-50/50',
      trend: notes.length > 10 ? 12 : null,
      action: 'viewAllNotes'
    },
    {
      title: '标签数量',
      value: notesStore.tags.length,
      subtitle: '知识分类',
      icon: 'TagIcon',
      iconBg: 'bg-green-100',
      iconColor: 'text-green-600',
      bgClass: 'hover:bg-green-50/50',
      action: 'viewTags'
    },
    {
      title: '今日新增',
      value: todayNotes,
      subtitle: '继续加油',
      icon: 'PlusCircleIcon',
      iconBg: 'bg-purple-100',
      iconColor: 'text-purple-600',
      bgClass: 'hover:bg-purple-50/50',
      trend: todayNotes > 0 ? 100 : 0,
      action: 'createNote'
    },
    {
      title: '本周活跃',
      value: weeklyActive,
      subtitle: '持续输出',
      icon: 'ChartBarIcon',
      iconBg: 'bg-orange-100',
      iconColor: 'text-orange-600',
      bgClass: 'hover:bg-orange-50/50',
      trend: weeklyActive > 5 ? 8 : -5,
      action: 'viewWeeklyStats'
    }
  ]
})

// 搜索建议
const searchSuggestions = [
  '上周的会议记录',
  '关于JavaScript的笔记',
  '项目管理相关内容',
  '学习笔记和心得'
]

// 智能推荐
const recommendations = ref([
  {
    id: 1,
    title: '复习上周的笔记',
    description: '您上周创建了12篇笔记，建议回顾一下',
    icon: 'ClockIcon',
    iconBg: 'bg-indigo-100',
    iconColor: 'text-indigo-600',
    action: 'reviewLastWeek'
  },
  {
    id: 2,
    title: '整理无标签笔记',
    description: '有18篇笔记还没有添加标签',
    icon: 'TagIcon',
    iconColor: 'text-yellow-600',
    iconBg: 'bg-yellow-100',
    action: 'organizeNotes'
  },
  {
    id: 3,
    title: '分享知识卡片',
    description: '把您的精彩笔记制作成卡片分享',
    icon: 'ShareIcon',
    iconColor: 'text-pink-600',
    iconBg: 'bg-pink-100',
    action: 'createKnowledgeCard'
  }
])

// 今日活动
const todayActivities = ref([
  { id: 1, action: '创建了《React项目经验》', time: '10:30', dotColor: 'bg-green-400' },
  { id: 2, action: '更新了《产品设计思路》', time: '14:15', dotColor: 'bg-blue-400' },
  { id: 3, action: '搜索了"API设计"', time: '16:45', dotColor: 'bg-purple-400' }
])

// 数据洞察
const weeklyWritingTime = ref('3.5小时')
const bestWritingTime = ref('上午5-10点')
const averageNoteLength = ref('520')

const recentNotes = computed(() => notesStore.recentNotes)

// 优化标签计数算法，使用Map提高性能
const popularTags = computed(() => {
  const tagCounts = new Map()
  const tagsMap = new Map(notesStore.tags.map(tag => [tag.name, tag]))
  
  notesStore.notes.forEach(note => {
    note.tags?.forEach(tag => {
      if (tag.name) {
        tagCounts.set(tag.name, (tagCounts.get(tag.name) || 0) + 1)
      }
    })
  })
  
  return Array.from(tagCounts.entries())
    .map(([name, count]) => {
      const tagInfo = tagsMap.get(name)
      return {
        name,
        count,
        color: tagInfo?.color || '#6B7280',
        id: tagInfo?.id
      }
    })
    .sort((a, b) => b.count - a.count)
    .slice(0, 10)
})

// 过滤后的笔记
const filteredRecentNotes = computed(() => {
  const notes = notesStore.recentNotes || []
  const now = new Date()
  
  switch (notesFilter.value) {
    case 'today':
      return notes.filter(note => {
        const noteDate = new Date(note.createdAt)
        return noteDate.toDateString() === now.toDateString()
      })
    case 'week':
      const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000)
      return notes.filter(note => new Date(note.createdAt) >= weekAgo)
    case 'month':
      const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000)
      return notes.filter(note => new Date(note.createdAt) >= monthAgo)
    default:
      return notes
  }
})

const viewNote = (id) => {
  router.push(`/notes/${id}`)
}

const performSearch = (query) => {
  router.push(`/search?q=${encodeURIComponent(query)}`)
}

const performQuickSearch = () => {
  if (!quickSearchQuery.value.trim()) return
  showSearchSuggestions.value = false
  router.push(`/search?q=${encodeURIComponent(quickSearchQuery.value)}`)
}

const searchByTag = (tagName) => {
  router.push(`/notes?tag=${encodeURIComponent(tagName)}`)
}

const askAI = async () => {
  if (!aiQuestion.value.trim()) return
  
  try {
    await aiStore.askQuestion(aiQuestion.value)
    aiQuestion.value = ''
  } catch (error) {
    notificationStore.error('AI问答失败', error.message)
  }
}

// 新增的方法
const getGreeting = () => {
  const hour = new Date().getHours()
  if (hour < 6) return '夜深了'
  if (hour < 12) return '早上好'
  if (hour < 18) return '下午好'
  return '晚上好'
}

const getMotivationalMessage = () => {
  const messages = [
    '今天也要加油哦！',
    '继续记录您的精彩想法',
    '知识的积累永不停止',
    '每一篇笔记都是成长的足迹'
  ]
  return messages[Math.floor(Math.random() * messages.length)]
}

const getCurrentDate = () => {
  return new Date().toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long'
  })
}

const getWeatherInfo = () => {
  // 这里可以集成天气API
  return '适合写作的好天气 ☀️'
}

const isNewNote = (note) => {
  const oneDayAgo = new Date(Date.now() - 24 * 60 * 60 * 1000)
  return new Date(note.createdAt) > oneDayAgo
}

const getWordCount = (content) => {
  return content ? content.length : 0
}

const handleStatClick = (stat) => {
  switch (stat.action) {
    case 'viewAllNotes':
      router.push('/notes')
      break
    case 'createNote':
      router.push('/notes/create')
      break
    case 'viewTags':
      // 可以跳转到标签管理页面
      break
    case 'viewWeeklyStats':
      // 可以跳转到统计页面
      break
  }
}

const handleRecommendationClick = (recommendation) => {
  switch (recommendation.action) {
    case 'reviewLastWeek':
      router.push('/notes?filter=lastWeek')
      break
    case 'organizeNotes':
      router.push('/notes?filter=untagged')
      break
    case 'createKnowledgeCard':
      // 未来功能
      notificationStore.info('功能开发中', '知识卡片功能即将上线')
      break
  }
}

const refreshRecommendations = () => {
  // 这里可以根据用户行为生成个性化推荐
  notificationStore.success('推荐已刷新')
}

onMounted(async () => {
  recentNotesLoading.value = true
  try {
    await Promise.all([
      notesStore.fetchNotes(1, 5),
      notesStore.fetchTags()
    ])
    
    // 加载搜索历史和AI对话历史
    searchStore.loadHistoryFromLocal()
    aiStore.loadConversationsFromLocal()
  } catch (error) {
    notificationStore.error('加载数据失败', error.message)
  } finally {
    recentNotesLoading.value = false
  }
})
</script>

<style scoped>
@keyframes slide-up {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.animate-slide-up {
  animation: slide-up 0.3s ease-out forwards;
  opacity: 0;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>