<template>
  <Layout>
    <div class="min-h-screen bg-gradient-to-br from-slate-50 via-indigo-50/30 to-purple-50/30">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <!-- 搜索头部 - 重新设计 -->
        <div class="text-center mb-12">
          <div class="mb-8">
            <div class="inline-flex items-center justify-center w-20 h-20 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-3xl mb-6 shadow-xl">
              <svg class="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <h1 class="text-5xl font-bold bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 bg-clip-text text-transparent mb-4">
              智能搜索
            </h1>
            <p class="text-xl text-gray-600 max-w-2xl mx-auto">
              让AI理解您的意图，用自然语言找到您需要的内容
            </p>
          </div>
        </div>

        <!-- 搜索框 - 全新设计 -->
        <div class="mb-12">
          <div class="relative max-w-4xl mx-auto">
            <div class="relative bg-white/80 backdrop-blur-sm rounded-3xl shadow-2xl border border-white/60 overflow-hidden">
              <input
                v-model="searchQuery"
                @input="debouncedSmartSearch"
                @keyup.enter="performSmartSearch"
                type="text"
                placeholder="试试「上周的会议记录」或「关于JavaScript的学习笔记」..."
                class="w-full pl-16 pr-32 py-6 text-lg bg-transparent border-0 focus:outline-none focus:ring-0 placeholder-gray-500"
              />
              <div class="absolute inset-y-0 left-0 pl-6 flex items-center pointer-events-none">
                <div class="p-2 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-2xl">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
              </div>
              <div class="absolute inset-y-0 right-0 pr-6 flex items-center space-x-3">
                <div v-if="isSmartSearch" class="inline-flex items-center px-4 py-2 rounded-2xl text-sm font-semibold bg-gradient-to-r from-purple-100 to-indigo-100 text-purple-800 border border-purple-200">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                  </svg>
                  AI智能搜索
                </div>
                <button 
                  @click="performSmartSearch"
                  :disabled="!searchQuery.trim()"
                  class="px-6 py-3 bg-gradient-to-r from-indigo-500 to-purple-500 text-white font-semibold rounded-2xl hover:from-indigo-600 hover:to-purple-600 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200 shadow-lg hover:shadow-xl transform hover:-translate-y-0.5"
                >
                  搜索
                </button>
              </div>
            </div>
          </div>
        </div>

      <!-- 简化的过滤选项 - 只在有搜索结果时显示 -->
      <div v-if="searched && searchResults.length > 0" class="mb-6">
        <div class="flex flex-wrap items-center justify-center gap-4 text-sm">
          <!-- 标签过滤 -->
          <div class="flex items-center space-x-2">
            <label class="text-gray-600">标签：</label>
            <select
              v-model="selectedTag"
              class="border-gray-300 rounded-lg text-sm focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">所有</option>
              <option
                v-for="tag in availableTags"
                :key="tag.id"
                :value="tag.name"
              >
                {{ tag.name }} ({{ tag.count }})
              </option>
            </select>
          </div>

          <!-- 日期过滤 -->
          <div class="flex items-center space-x-2">
            <label class="text-gray-600">时间：</label>
            <select
              v-model="dateRange"
              class="border-gray-300 rounded-lg text-sm focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">所有</option>
              <option value="today">今天</option>
              <option value="week">本周</option>
              <option value="month">本月</option>
            </select>
          </div>

          <!-- 排序 -->
          <div class="flex items-center space-x-2">
            <label class="text-gray-600">排序：</label>
            <select
              v-model="sortBy"
              class="border-gray-300 rounded-lg text-sm focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="relevance">相关度</option>
              <option value="date">日期</option>
              <option value="title">标题</option>
            </select>
          </div>
        </div>
      </div>

      <!-- 搜索历史和快捷搜索 -->
      <div v-if="(!searchQuery || !searched) && searchHistory.length > 0" class="mb-8">
        <div class="text-center mb-4">
          <h3 class="text-sm font-medium text-gray-700 mb-3">最近搜索</h3>
          <div class="flex flex-wrap justify-center gap-2">
            <button
              v-for="(item, index) in searchHistory.slice(0, 6)"
              :key="index"
              @click="searchFromHistory(item)"
              class="inline-flex items-center px-3 py-2 bg-white border border-gray-300 rounded-full text-sm text-gray-700 hover:bg-gray-50 hover:border-primary-300 transition-all"
            >
              <svg class="w-4 h-4 mr-1 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {{ item.query }}
            </button>
            <button
              v-if="searchHistory.length > 6"
              @click="showAllHistory = !showAllHistory"
              class="px-3 py-2 text-primary-600 hover:text-primary-800 text-sm"
            >
              {{ showAllHistory ? '收起' : `+${searchHistory.length - 6} 更多` }}
            </button>
          </div>
          <div v-if="showAllHistory" class="mt-3 flex flex-wrap justify-center gap-2">
            <button
              v-for="(item, index) in searchHistory.slice(6)"
              :key="index + 6"
              @click="searchFromHistory(item)"
              class="inline-flex items-center px-3 py-2 bg-white border border-gray-300 rounded-full text-sm text-gray-700 hover:bg-gray-50 transition-all"
            >
              {{ item.query }}
            </button>
          </div>
        </div>
      </div>

      <!-- 搜索结果 -->
      <div v-if="searched">
        <!-- 搜索状态 -->
        <div v-if="searching" class="text-center py-16">
          <div class="animate-pulse">
            <div class="inline-flex items-center px-4 py-2 bg-gradient-to-r from-primary-50 to-purple-50 rounded-full">
              <LoadingSpinner size="small" />
              <span class="ml-2 text-primary-700 font-medium">
                {{ isSmartSearch ? 'AI正在理解您的搜索意图...' : '搜索中...' }}
              </span>
            </div>
          </div>
        </div>

        <!-- 结果统计 -->
        <div v-else-if="searchResults.length > 0" class="space-y-6">
          <div class="bg-gradient-to-r from-green-50 to-primary-50 rounded-lg p-4 text-center">
            <div class="flex items-center justify-center space-x-2 text-green-700">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span class="font-medium">
                找到 <span class="font-bold">{{ totalResults }}</span> 个相关结果
              </span>
              <span v-if="isSmartSearch" class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
                AI智能排序
              </span>
            </div>
            <p v-if="searchTime" class="text-sm text-gray-600 mt-1">
              耗时 {{ searchTime }}ms
            </p>
          </div>

          <!-- 结果列表 -->
          <div class="space-y-4">
            <div
              v-for="result in paginatedResults"
              :key="result.id"
              class="bg-white rounded-lg shadow-sm border hover:shadow-md transition-shadow"
            >
              <router-link :to="`/notes/${result.id}`" class="block p-6">
                <div class="flex items-start justify-between">
                  <div class="flex-1">
                    <!-- 标题 -->
                    <h3 class="text-lg font-semibold text-gray-900 mb-2" v-html="highlightText(result.title)"></h3>

                    <!-- 内容摘要 -->
                    <p class="text-gray-600 line-clamp-3 mb-3" v-html="getHighlightedExcerpt(result)"></p>

                    <!-- 元信息 -->
                    <div class="flex items-center space-x-4 text-sm text-gray-500">
                      <span>{{ formatDate(result.updatedAt) }}</span>
                      <span v-if="result.score && isSmartSearch" class="flex items-center">
                        <svg class="w-4 h-4 mr-1 text-purple-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                        </svg>
                        相关度 {{ Math.round(result.score * 100) }}%
                      </span>
                      <span v-if="result.matchType" class="px-2 py-1 bg-gray-100 rounded-full text-xs">
                        {{ result.matchType === 'semantic' ? '语义匹配' : '关键词匹配' }}
                      </span>
                    </div>

                    <!-- 标签 -->
                    <div v-if="result.tags.length > 0" class="mt-3 flex flex-wrap gap-2">
                      <span
                        v-for="tag in result.tags"
                        :key="tag.id"
                        :style="{ backgroundColor: tag.color + '20', color: tag.color }"
                        class="px-2 py-1 rounded-full text-xs font-medium"
                      >
                        {{ tag.name }}
                      </span>
                    </div>
                  </div>

                  <!-- 匹配位置指示器 -->
                  <div v-if="result.matchLocations && result.matchLocations.length > 0" class="ml-4 flex-shrink-0">
                    <div class="bg-primary-100 text-primary-700 px-2 py-1 rounded text-xs font-medium">
                      {{ result.matchLocations.length }} 处匹配
                    </div>
                  </div>
                </div>
              </router-link>
            </div>
          </div>

          <!-- 分页 -->
          <div v-if="totalPages > 1" class="flex justify-center mt-8">
            <nav class="flex space-x-2">
              <button
                @click="currentPage = Math.max(1, currentPage - 1)"
                :disabled="currentPage === 1"
                class="px-3 py-2 bg-white border rounded-lg disabled:opacity-50 hover:bg-gray-50"
              >
                上一页
              </button>

              <button
                v-for="page in visiblePages"
                :key="page"
                @click="currentPage = page"
                :class="[
                  'px-3 py-2 border rounded-lg',
                  page === currentPage ? 'bg-primary-500 text-white' : 'bg-white hover:bg-gray-50'
                ]"
              >
                {{ page }}
              </button>

              <button
                @click="currentPage = Math.min(totalPages, currentPage + 1)"
                :disabled="currentPage === totalPages"
                class="px-3 py-2 bg-white border rounded-lg disabled:opacity-50 hover:bg-gray-50"
              >
                下一页
              </button>
            </nav>
          </div>
        </div>

        <!-- 无结果 -->
        <div v-else class="text-center py-16">
          <div class="max-w-md mx-auto">
            <div class="bg-gradient-to-br from-gray-50 to-gray-100 rounded-2xl p-8">
              <div class="w-16 h-16 mx-auto mb-4 bg-gradient-to-br from-gray-200 to-gray-300 rounded-full flex items-center justify-center">
                <svg class="w-8 h-8 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
              <h3 class="text-lg font-semibold text-gray-900 mb-2">未找到相关内容</h3>
              <p class="text-gray-600 mb-6 text-sm">
                "{{ searchQuery }}" 没有匹配的笔记
              </p>
              
              <!-- 搜索建议 -->
              <div class="space-y-3">
                <div class="text-left">
                  <p class="text-sm font-medium text-gray-700 mb-2">您可以尝试：</p>
                  <ul class="text-sm text-gray-600 space-y-1">
                    <li class="flex items-center">
                      <svg class="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      使用更简单的关键词
                    </li>
                    <li class="flex items-center">
                      <svg class="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      检查拼写是否正确
                    </li>
                    <li class="flex items-center">
                      <svg class="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      用自然语言描述您想找的内容
                    </li>
                  </ul>
                </div>
                
                <div class="pt-4 border-t border-gray-200">
                  <button
                    @click="retryWithSmartSearch"
                    class="inline-flex items-center px-4 py-2 bg-gradient-to-r from-primary-500 to-purple-500 text-white text-sm font-medium rounded-lg hover:from-primary-600 hover:to-purple-600 transition-all"
                  >
                    <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                    </svg>
                    重新搜索
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 搜索提示和示例 -->
      <div v-else-if="!searchQuery" class="space-y-8">
        <!-- 搜索提示 -->
        <div class="bg-gradient-to-br from-primary-50 via-purple-50 to-pink-50 rounded-2xl p-8 text-center">
          <div class="w-16 h-16 mx-auto mb-4 bg-gradient-to-br from-primary-500 to-purple-500 rounded-full flex items-center justify-center">
            <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
          </div>
          <h3 class="text-xl font-bold text-gray-900 mb-3">AI智能搜索</h3>
          <p class="text-gray-600 mb-6 max-w-md mx-auto">
            无需记住确切的关键词，用自然语言描述您想找的内容，AI会帮您找到最相关的笔记
          </p>
          <div class="flex flex-wrap justify-center gap-3">
            <span class="inline-flex items-center px-3 py-2 bg-white/80 backdrop-blur-sm rounded-full text-sm font-medium text-gray-700">
              <svg class="w-4 h-4 mr-1 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
              </svg>
              支持自然语言
            </span>
            <span class="inline-flex items-center px-3 py-2 bg-white/80 backdrop-blur-sm rounded-full text-sm font-medium text-gray-700">
              <svg class="w-4 h-4 mr-1 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              理解语义
            </span>
            <span class="inline-flex items-center px-3 py-2 bg-white/80 backdrop-blur-sm rounded-full text-sm font-medium text-gray-700">
              <svg class="w-4 h-4 mr-1 text-purple-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
              智能排序
            </span>
          </div>
        </div>

        <!-- 搜索示例 -->
        <div class="bg-white rounded-xl border border-gray-200 p-6">
          <h4 class="font-semibold text-gray-900 mb-4 text-center">搜索示例</h4>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <button
              v-for="example in searchExamples"
              :key="example.query"
              @click="searchQuery = example.query; performSmartSearch()"
              class="group p-4 bg-gray-50 rounded-lg hover:bg-primary-50 hover:border-primary-200 border border-transparent transition-all text-left"
            >
              <div class="flex items-start space-x-3">
                <div class="flex-shrink-0 w-8 h-8 bg-gradient-to-br from-gray-200 to-gray-300 group-hover:from-primary-200 group-hover:to-primary-300 rounded-lg flex items-center justify-center transition-all">
                  <svg class="w-4 h-4 text-gray-600 group-hover:text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <div class="flex-1">
                  <p class="font-medium text-gray-900 group-hover:text-primary-900 mb-1">{{ example.query }}</p>
                  <p class="text-sm text-gray-500">{{ example.description }}</p>
                </div>
              </div>
            </button>
          </div>
        </div>
      </div>
    </div>
    </div>
  </Layout>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useNotesStore } from '@/stores/notes'
import { useSearchStore } from '@/stores/search'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import { formatDate } from '@/utils/date'
import { debounce, escapeRegExp } from 'lodash-es'

const notesStore = useNotesStore()
const searchStore = useSearchStore()
const notificationStore = useNotificationStore()

// 状态
const searchQuery = ref('')
const selectedTag = ref('')
const dateRange = ref('')
const sortBy = ref('relevance')
const searching = ref(false)
const searched = ref(false)
const searchResults = ref([])
const searchTime = ref(0)
const currentPage = ref(1)
const perPage = 20
const showAllHistory = ref(false)
const isSmartSearch = ref(false)

// 搜索示例
const searchExamples = [
  { query: "如何提高工作效率", description: "查找效率相关的笔记" },
  { query: "上个月的会议记录", description: "按时间查找会议笔记" },
  { query: "JavaScript 学习笔记", description: "技术学习相关内容" },
  { query: "读书心得和感悟", description: "个人思考和总结" }
]

// 计算属性
const searchHistory = computed(() => searchStore.searchHistory)
const totalResults = computed(() => searchResults.value.length)

// 可用标签 - 从搜索结果中提取
const availableTags = computed(() => {
  const tagCounts = new Map()
  searchResults.value.forEach(note => {
    note.tags?.forEach(tag => {
      if (tag.name) {
        tagCounts.set(tag.name, (tagCounts.get(tag.name) || 0) + 1)
      }
    })
  })
  
  return Array.from(tagCounts.entries())
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => b.count - a.count)
})

const filteredResults = computed(() => {
  let results = [...searchResults.value]

  // 标签过滤
  if (selectedTag.value) {
    results = results.filter(note =>
      note.tags.some(tag => tag.name === selectedTag.value)
    )
  }

  // 日期过滤
  if (dateRange.value) {
    const now = new Date()
    const ranges = {
      today: 24 * 60 * 60 * 1000,
      week: 7 * 24 * 60 * 60 * 1000,
      month: 30 * 24 * 60 * 60 * 1000,
      year: 365 * 24 * 60 * 60 * 1000
    }
    const cutoff = now - ranges[dateRange.value]
    results = results.filter(note => new Date(note.updatedAt) >= cutoff)
  }

  // 排序
  if (sortBy.value === 'date') {
    results.sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt))
  } else if (sortBy.value === 'title') {
    results.sort((a, b) => a.title.localeCompare(b.title))
  }
  // relevance 排序保持原始顺序（后端已排序）

  return results
})

const totalPages = computed(() => Math.ceil(filteredResults.value.length / perPage))

const paginatedResults = computed(() => {
  const start = (currentPage.value - 1) * perPage
  return filteredResults.value.slice(start, start + perPage)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value

  for (let i = Math.max(1, current - 2); i <= Math.min(total, current + 2); i++) {
    pages.push(i)
  }

  return pages
})

// 智能搜索 - 统一入口
const performSmartSearch = async () => {
  if (!searchQuery.value.trim()) return

  searching.value = true
  searched.value = true
  const startTime = performance.now()

  try {
    // 使用AI智能搜索
    const results = await searchStore.smartSearch(searchQuery.value)
    isSmartSearch.value = true

    searchResults.value = results
    searchTime.value = Math.round(performance.now() - startTime)
    currentPage.value = 1

    // 添加到搜索历史
    searchStore.addToHistory({
      query: searchQuery.value,
      type: 'smart'
    })
  } catch (error) {
    // 如果AI搜索失败，回退到全文搜索
    console.warn('AI搜索失败，使用全文搜索', error)
    try {
      const results = await searchStore.fulltextSearch(searchQuery.value)
      isSmartSearch.value = false
      searchResults.value = results
      searchTime.value = Math.round(performance.now() - startTime)
      currentPage.value = 1
      
      searchStore.addToHistory({
        query: searchQuery.value,
        type: 'fulltext'
      })
    } catch (fallbackError) {
      notificationStore.error('搜索失败', fallbackError.message)
    }
  } finally {
    searching.value = false
  }
}

const debouncedSmartSearch = debounce(() => {
  if (searchQuery.value.trim()) {
    performSmartSearch()
  }
}, 800) // 稍微增加防抖时间，给AI搜索更多时间

const searchFromHistory = (item) => {
  searchQuery.value = item.query
  showAllHistory.value = false
  performSmartSearch()
}

const clearHistory = () => {
  searchStore.clearHistory()
  notificationStore.success('搜索历史已清空')
}

const retryWithSmartSearch = () => {
  performSmartSearch()
}

// 缓存正则表达式以提高性能
const highlightRegex = computed(() => {
  if (!searchQuery.value || isSmartSearch.value) return null
  return new RegExp(`(${escapeRegExp(searchQuery.value)})`, 'gi')
})

const highlightText = (text) => {
  const regex = highlightRegex.value
  if (!regex) return text
  return text.replace(regex, '<mark class="bg-yellow-200">$1</mark>')
}

const getHighlightedExcerpt = (result) => {
  if (isSmartSearch.value && result.excerpt) {
    return result.excerpt
  }

  let excerpt = result.content.substring(0, 200)

  if (searchQuery.value && !isSmartSearch.value) {
    const queryIndex = result.content.toLowerCase().indexOf(searchQuery.value.toLowerCase())
    if (queryIndex > 100) {
      excerpt = '...' + result.content.substring(queryIndex - 50, queryIndex + 150) + '...'
    }
  }

  return highlightText(excerpt)
}

// 组件挂载时初始化
onMounted(() => {
  // 加载搜索历史
  searchStore.loadHistoryFromLocal()
})

// 监听过滤条件变化
watch([selectedTag, dateRange, sortBy], () => {
  currentPage.value = 1
})
</script>
