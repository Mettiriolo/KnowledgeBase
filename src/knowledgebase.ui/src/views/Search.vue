<template>
  <Layout>
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 class="text-3xl font-bold text-gray-900 mb-6">搜索</h1>

      <!-- 搜索输入框 -->
      <div class="mb-6">
        <div class="relative">
          <input
            v-model="searchQuery"
            @input="debouncedSearch"
            @keyup.enter="performSearch"
            type="text"
            placeholder="搜索笔记..."
            class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- 搜索选项 -->
      <div class="bg-white rounded-lg shadow-sm border p-4 mb-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <!-- 搜索类型 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">搜索类型</label>
            <div class="space-y-2">
              <label class="flex items-center">
                <input
                  v-model="searchType"
                  type="radio"
                  value="fulltext"
                  class="text-primary-600 focus:ring-primary-500"
                />
                <span class="ml-2 text-sm text-gray-700">全文搜索</span>
              </label>
              <label class="flex items-center">
                <input
                  v-model="searchType"
                  type="radio"
                  value="semantic"
                  class="text-primary-600 focus:ring-primary-500"
                />
                <span class="ml-2 text-sm text-gray-700">
                  语义搜索
                  <span class="text-xs text-gray-500">（AI驱动）</span>
                </span>
              </label>
            </div>
          </div>

          <!-- 过滤选项 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">标签过滤</label>
            <select
              v-model="selectedTag"
              class="w-full border-gray-300 rounded-lg text-sm focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">所有标签</option>
              <option
                v-for="tag in notesStore.tags"
                :key="tag.id"
                :value="tag.name"
              >
                {{ tag.name }}
              </option>
            </select>
          </div>

          <!-- 日期范围 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">日期范围</label>
            <select
              v-model="dateRange"
              class="w-full border-gray-300 rounded-lg text-sm focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="">所有时间</option>
              <option value="today">今天</option>
              <option value="week">最近一周</option>
              <option value="month">最近一月</option>
              <option value="year">最近一年</option>
            </select>
          </div>
        </div>
      </div>

      <!-- 搜索历史 -->
      <div v-if="searchHistory.length > 0 && !searchQuery" class="mb-6">
        <div class="flex items-center justify-between mb-3">
          <h3 class="text-sm font-medium text-gray-700">搜索历史</h3>
          <button
            @click="clearHistory"
            class="text-xs text-gray-500 hover:text-gray-700"
          >
            清空历史
          </button>
        </div>
        <div class="flex flex-wrap gap-2">
          <button
            v-for="(item, index) in searchHistory"
            :key="index"
            @click="searchFromHistory(item)"
            class="px-3 py-1 bg-gray-100 text-gray-700 rounded-full text-sm hover:bg-gray-200 transition-colors"
          >
            {{ item.query }}
            <span v-if="item.type === 'semantic'" class="ml-1 text-xs text-purple-600">AI</span>
          </button>
        </div>
      </div>

      <!-- 搜索结果 -->
      <div v-if="searched">
        <!-- 搜索状态 -->
        <div v-if="searching" class="text-center py-12">
          <LoadingSpinner />
          <p class="mt-3 text-gray-500">
            {{ searchType === 'semantic' ? 'AI正在理解您的搜索意图...' : '搜索中...' }}
          </p>
        </div>

        <!-- 结果统计 -->
        <div v-else-if="searchResults.length > 0" class="space-y-6">
          <div class="flex items-center justify-between">
            <p class="text-gray-600">
              找到 <span class="font-semibold text-gray-900">{{ totalResults }}</span> 个相关结果
              <span v-if="searchTime" class="text-sm text-gray-500 ml-2">
                (耗时 {{ searchTime }}ms)
              </span>
            </p>
            <div class="flex items-center space-x-2">
              <label class="text-sm text-gray-700">排序：</label>
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
                      <span v-if="result.score && searchType === 'semantic'" class="flex items-center">
                        <svg class="w-4 h-4 mr-1 text-purple-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                        </svg>
                        相关度 {{ Math.round(result.score * 100) }}%
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
        <div v-else class="text-center py-12 bg-white rounded-lg shadow-sm">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900">没有找到结果</h3>
          <p class="mt-1 text-sm text-gray-500">
            尝试使用不同的关键词或切换搜索类型
          </p>
          <div v-if="searchType === 'fulltext'" class="mt-6">
            <button
              @click="switchToSemantic"
              class="btn btn-primary"
            >
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
              </svg>
              尝试AI语义搜索
            </button>
          </div>
        </div>
      </div>

      <!-- 搜索建议 -->
      <div v-else-if="!searchQuery" class="bg-gradient-to-r from-primary-50 to-purple-50 rounded-lg p-6 text-center">
        <svg class="mx-auto h-12 w-12 text-primary-600 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
        <h3 class="text-lg font-semibold text-gray-900 mb-2">智能搜索提示</h3>
        <p class="text-sm text-gray-600 mb-4">
          使用AI语义搜索可以理解您的搜索意图，找到更相关的内容
        </p>
        <div class="flex flex-wrap justify-center gap-2 text-sm">
          <span class="px-3 py-1 bg-white rounded-full">支持自然语言</span>
          <span class="px-3 py-1 bg-white rounded-full">理解上下文</span>
          <span class="px-3 py-1 bg-white rounded-full">智能排序</span>
        </div>
      </div>
    </div>
  </Layout>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
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
const searchType = ref('fulltext')
const selectedTag = ref('')
const dateRange = ref('')
const sortBy = ref('relevance')
const searching = ref(false)
const searched = ref(false)
const searchResults = ref([])
const searchTime = ref(0)
const currentPage = ref(1)
const perPage = 20

// 计算属性
const searchHistory = computed(() => searchStore.history)
const totalResults = computed(() => searchResults.value.length)

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

// 方法
const performSearch = async () => {
  if (!searchQuery.value.trim()) return

  searching.value = true
  searched.value = true
  const startTime = performance.now()

  try {
    let results
    if (searchType.value === 'semantic') {
      results = await searchStore.semanticSearch(searchQuery.value)
    } else {
      results = await searchStore.fulltextSearch(searchQuery.value)
    }

    searchResults.value = results
    searchTime.value = Math.round(performance.now() - startTime)
    currentPage.value = 1

    // 添加到搜索历史
    searchStore.addToHistory({
      query: searchQuery.value,
      type: searchType.value
    })
  } catch (error) {
    notificationStore.error('搜索失败', error.message)
  } finally {
    searching.value = false
  }
}

const debouncedSearch = debounce(() => {
  if (searchQuery.value.trim()) {
    performSearch()
  }
}, 500)

const searchFromHistory = (item) => {
  searchQuery.value = item.query
  searchType.value = item.type
  performSearch()
}

const clearHistory = () => {
  searchStore.clearHistory()
  notificationStore.success('搜索历史已清空')
}

const switchToSemantic = () => {
  searchType.value = 'semantic'
  performSearch()
}

const highlightText = (text) => {
  if (!searchQuery.value || searchType.value === 'semantic') return text

  const regex = new RegExp(`(${escapeRegExp(searchQuery.value)})`, 'gi')
  return text.replace(regex, '<mark class="bg-yellow-200">$1</mark>')
}

const getHighlightedExcerpt = (result) => {
  if (searchType.value === 'semantic' && result.excerpt) {
    return result.excerpt
  }

  let excerpt = result.content.substring(0, 200)

  if (searchQuery.value && searchType.value === 'fulltext') {
    const queryIndex = result.content.toLowerCase().indexOf(searchQuery.value.toLowerCase())
    if (queryIndex > 100) {
      excerpt = '...' + result.content.substring(queryIndex - 50, queryIndex + 150) + '...'
    }
  }

  return highlightText(excerpt)
}

// 监听过滤条件变化
watch([selectedTag, dateRange, sortBy], () => {
  currentPage.value = 1
})
</script>
