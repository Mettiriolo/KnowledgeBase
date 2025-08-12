<template>
  <Layout>
    <div class="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50/20 to-indigo-50/30">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <!-- 页面头部 - 重新设计 -->
        <div class="mb-10">
          <div class="bg-white/60 backdrop-blur-sm rounded-3xl p-8 border border-white/50 shadow-lg">
            <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between">
              <div class="mb-6 lg:mb-0">
                <div class="flex items-center mb-4">
                  <div class="p-3 bg-gradient-to-r from-primary-500 to-purple-500 rounded-2xl mr-4">
                    <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                    </svg>
                  </div>
                  <div>
                    <h1 class="text-4xl font-bold bg-gradient-to-r from-gray-900 via-primary-600 to-purple-600 bg-clip-text text-transparent">
                      我的笔记
                    </h1>
                    <p class="text-lg text-gray-600 mt-2">
                      共 {{ notesStore.totalNotes }} 篇精彩内容
                      <span v-if="selectedTag" class="ml-3">
                        · 筛选:
                        <span
                          :style="{ backgroundColor: selectedTagObject?.color + '15', color: selectedTagObject?.color, borderColor: selectedTagObject?.color + '30' }"
                          class="ml-2 px-3 py-1.5 rounded-full text-sm font-semibold border"
                        >
                          {{ selectedTag }}
                        </span>
                      </span>
                    </p>
                  </div>
                </div>
              </div>
              <div class="flex items-center space-x-4">
                <router-link 
                  to="/notes/create" 
                  class="group relative inline-flex items-center px-6 py-3 bg-gradient-to-r from-primary-500 to-purple-500 text-white font-semibold rounded-2xl shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-0.5"
                >
                  <div class="absolute inset-0 bg-gradient-to-r from-primary-600 to-purple-600 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity"></div>
                  <div class="relative flex items-center">
                    <div class="p-1 bg-white/20 rounded-lg mr-3 group-hover:scale-110 transition-transform">
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                      </svg>
                    </div>
                    创建笔记
                  </div>
                </router-link>
              </div>
            </div>
          </div>
        </div>

        <div class="flex gap-8">
          <!-- 左侧过滤栏 - 美化 -->
          <div class="w-64 flex-shrink-0 space-y-6">
            <!-- 搜索框 -->
            <div class="relative">
              <input
                v-model="searchQuery"
                @input="debouncedSearch"
                placeholder="搜索笔记..."
                class="w-full pl-12 pr-4 py-3 bg-white/80 backdrop-blur-sm border border-white/60 rounded-2xl focus:outline-none focus:ring-2 focus:ring-primary-400 focus:border-primary-400 transition-all shadow-lg"
              />
              <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-primary-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
            </div>

            <!-- 标签过滤 -->
            <div class="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 border border-white/60">
              <h3 class="font-bold text-gray-900 mb-4 flex items-center">
                <svg class="w-5 h-5 mr-2 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                </svg>
                标签筛选
              </h3>
              <div class="space-y-3">
                <label
                  v-for="tag in notesStore.tags"
                  :key="tag.id"
                  class="group flex items-center cursor-pointer hover:bg-gradient-to-r hover:from-primary-50 hover:to-purple-50 p-3 rounded-xl transition-all duration-200"
                >
                  <input
                    type="checkbox"
                    :value="tag.name"
                    v-model="selectedTags"
                    class="text-primary-600 rounded-md focus:ring-primary-500 focus:ring-offset-0"
                  />
                  <span
                    :style="{ color: tag.color }"
                    class="ml-3 text-sm font-semibold group-hover:scale-105 transition-transform"
                  >
                    {{ tag.name }}
                  </span>
                  <span class="ml-auto text-xs font-medium px-2 py-1 bg-gray-100 text-gray-600 rounded-full group-hover:bg-white transition-colors">
                    {{ tag.count || 0 }}
                  </span>
                </label>
              </div>
            </div>

            <!-- 排序选项 -->
            <div class="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 border border-white/60">
              <h3 class="font-bold text-gray-900 mb-4 flex items-center">
                <svg class="w-5 h-5 mr-2 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4h13M3 8h9m-9 4h9m5-4v12m0 0l-4-4m4 4l4-4" />
                </svg>
                排序方式
              </h3>
              <select 
                v-model="sortBy" 
                class="w-full px-4 py-3 bg-white/90 border border-gray-200 rounded-xl text-sm font-medium focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-all"
              >
                <option value="updatedAt">🕒 最近更新</option>
                <option value="createdAt">📅 创建时间</option>
                <option value="title">🔤 标题排序</option>
              </select>
            </div>
          </div>

        <!-- 笔记列表 -->
        <div class="flex-1">
          <!-- 批量操作栏 -->
          <div v-if="selectedNotes.length > 0" class="bg-primary-50 border border-primary-200 rounded-lg p-4 mb-4">
            <div class="flex items-center justify-between">
              <span class="text-sm text-primary-700">
                已选择 {{ selectedNotes.length }} 篇笔记
              </span>
              <div class="space-x-2">
                <button 
                  @click="batchDelete" 
                  class="px-3 py-1.5 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors"
                >
                  批量删除
                </button>
                <button 
                  @click="clearSelection" 
                  class="px-3 py-1.5 bg-white text-gray-700 text-sm font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
                >
                  取消选择
                </button>
              </div>
            </div>
          </div>

          <!-- 加载状态 -->
          <div v-if="notesStore.isLoading" class="text-center py-12">
            <LoadingSpinner />
            <p class="mt-3 text-gray-500">加载中...</p>
          </div>

          <!-- 空状态 -->
          <div v-else-if="filteredNotes.length === 0" class="text-center py-12 bg-white rounded-lg shadow">
            <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <h3 class="mt-2 text-sm font-medium text-gray-900">没有找到笔记</h3>
            <p class="mt-1 text-sm text-gray-500">
              {{ searchQuery || selectedTags.length ? '尝试调整筛选条件' : '开始创建您的第一篇笔记吧' }}
            </p>
            <div class="mt-6">
              <router-link 
                to="/notes/create" 
                class="inline-flex items-center px-4 py-2 bg-primary-600 text-white font-medium rounded-lg hover:bg-primary-700 transition-colors"
              >
                创建笔记
              </router-link>
            </div>
          </div>

          <!-- 笔记列表 -->
          <div v-else class="space-y-4">
            <div
              v-for="note in paginatedNotes"
              :key="note.id"
              class="relative"
            >
              <!-- 选择框 -->
              <div class="absolute top-4 left-4 z-20">
                <input
                  type="checkbox"
                  :value="note.id"
                  v-model="selectedNotes"
                  class="h-5 w-5 text-primary-600 rounded-md focus:ring-primary-500 bg-white/80 backdrop-blur-sm"
                  @click.stop
                />
              </div>

              <NoteCard
                :note="note"
                @view="viewNote"
                @edit="editNote"
                @delete="deleteNote"
              />
            </div>
          </div>

          <!-- 分页 -->
          <div v-if="totalPages > 1" class="mt-8 flex justify-center">
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
      </div>
    </div>
    </div>
    <!-- 删除确认对话框 -->
    <teleport to="body">
      <div v-if="showDeleteConfirm" class="fixed inset-0 z-50 overflow-y-auto">
        <!-- 背景遮罩 -->
        <div 
          class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" 
          @click="closeDeleteConfirm"
        ></div>
        
        <!-- 对话框容器 -->
        <div class="flex min-h-screen items-center justify-center p-4">
          <!-- 对话框 -->
          <div class="relative bg-white rounded-lg shadow-xl max-w-md w-full p-6 transform transition-all">
            <h3 class="text-lg font-bold text-gray-900 mb-4">确认删除</h3>
            <p class="text-gray-600 mb-6">
              {{ deleteTarget.batch
                ? `确定要删除选中的 ${selectedNotes.length} 篇笔记吗？`
                : '确定要删除这篇笔记吗？'
              }}
              此操作无法撤销。
            </p>
            <div class="flex justify-end space-x-3">
              <button 
                @click="closeDeleteConfirm" 
                class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                取消
              </button>
              <button 
                @click="confirmDelete" 
                class="px-4 py-2 text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors"
              >
                确认删除
              </button>
            </div>
          </div>
        </div>
      </div>
    </teleport>
  </Layout>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useNotesStore } from '@/stores/notes'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import NoteCard from '@/components/Notes/NoteCard.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import { debounce } from 'lodash-es'

const router = useRouter()
const route = useRoute()
const notesStore = useNotesStore()
const notificationStore = useNotificationStore()

// 状态
const searchQuery = ref('')
const selectedTags = ref([])
const sortBy = ref('updatedAt')
const currentPage = ref(1)
const perPage = 15
const selectedNotes = ref([])
const showDeleteConfirm = ref(false)
const deleteTarget = ref({})

// 从路由获取标签参数
const selectedTag = computed(() => route.query.tag)
const selectedTagObject = computed(() =>
  notesStore.tags.find(t => t.name === selectedTag.value)
)

// 监听标签参数变化
watch(() => route.query.tag, (tag) => {
  if (tag) {
    selectedTags.value = [tag]
  } else {
    selectedTags.value = []
  }
})

// 过滤和排序笔记
const filteredNotes = computed(() => {
  let notes = [...notesStore.notes]

  // 搜索过滤
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    notes = notes.filter(note =>
      note.title.toLowerCase().includes(query) ||
      note.content.toLowerCase().includes(query)
    )
  }

  // 标签过滤
  if (selectedTags.value.length > 0) {
    notes = notes.filter(note =>
      selectedTags.value.some(tag =>
        note.tags.some(noteTag => noteTag.name === tag)
      )
    )
  }

  // 排序
  notes.sort((a, b) => {
    if (sortBy.value === 'title') {
      return a.title.localeCompare(b.title)
    }
    return new Date(b[sortBy.value]) - new Date(a[sortBy.value])
  })

  return notes
})

// 分页
const totalPages = computed(() =>
  Math.ceil(filteredNotes.value.length / perPage)
)

const paginatedNotes = computed(() => {
  const start = (currentPage.value - 1) * perPage
  return filteredNotes.value.slice(start, start + perPage)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value

  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 3) {
      for (let i = 1; i <= 5; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(total)
    } else if (current >= total - 2) {
      pages.push(1)
      pages.push('...')
      for (let i = total - 4; i <= total; i++) {
        pages.push(i)
      }
    } else {
      pages.push(1)
      pages.push('...')
      for (let i = current - 1; i <= current + 1; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(total)
    }
  }

  return pages
})

// 防抖搜索
const debouncedSearch = debounce(() => {
  currentPage.value = 1
}, 300)

// 方法
const viewNote = (id) => {
  router.push(`/notes/${id}`)
}

const editNote = (id) => {
  router.push(`/notes/${id}/edit`)
}

const deleteNote = (id) => {
  deleteTarget.value = { id, batch: false }
  showDeleteConfirm.value = true
}

const batchDelete = () => {
  deleteTarget.value = { batch: true }
  showDeleteConfirm.value = true
}

const confirmDelete = async () => {
  try {
    if (deleteTarget.value.batch) {
      await notesStore.batchDeleteNotes(selectedNotes.value)
      selectedNotes.value = []
    } else {
      await notesStore.deleteNote(deleteTarget.value.id, true)
    }

    closeDeleteConfirm()

    // 如果当前页没有笔记了，返回上一页
    if (paginatedNotes.value.length === 0 && currentPage.value > 1) {
      currentPage.value--
    }
  } catch (error) {
    // Store methods handle their own error notifications, so we don't need to duplicate
    console.error('Delete failed:', error)
  }
}

const closeDeleteConfirm = () => {
  showDeleteConfirm.value = false
  deleteTarget.value = {}
}

const clearSelection = () => {
  selectedNotes.value = []
}

// 监听筛选条件变化，重置页码
watch([searchQuery, selectedTags, sortBy], () => {
  currentPage.value = 1
})

// 初始化
onMounted(async () => {
  try {
    await Promise.all([
      notesStore.fetchNotes(),
      notesStore.fetchTags()
    ])

    // 如果URL有标签参数，设置过滤
    if (selectedTag.value) {
      selectedTags.value = [selectedTag.value]
    }
  } catch (error) {
    notificationStore.error('加载笔记失败', error.message)
  }
})
</script>

<style scoped>
/* 对话框动画 */
.fixed {
  animation: fadeIn 0.2s ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.transform {
  animation: slideUp 0.3s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>