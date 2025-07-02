<template>
  <Layout>
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 页面头部 -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900">我的笔记</h1>
            <p class="text-gray-600 mt-2">
              共 {{ notesStore.totalNotes }} 篇笔记
              <span v-if="selectedTag" class="ml-2">
                · 标签筛选:
                <span
                  :style="{ backgroundColor: selectedTagObject?.color + '20', color: selectedTagObject?.color }"
                  class="px-2 py-1 rounded-full text-sm font-medium"
                >
                  {{ selectedTag }}
                </span>
              </span>
            </p>
          </div>
          <router-link 
            to="/notes/create" 
            class="inline-flex items-center px-4 py-2 bg-primary-600 text-white font-medium rounded-lg hover:bg-primary-700 transition-colors"
          >
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            新建笔记
          </router-link>
        </div>
      </div>

      <div class="flex gap-8">
        <!-- 左侧过滤栏 -->
        <div class="w-64 flex-shrink-0">
          <!-- 搜索框 -->
          <div class="mb-6">
            <input
              v-model="searchQuery"
              @input="debouncedSearch"
              placeholder="搜索笔记..."
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            />
          </div>

          <!-- 标签过滤 -->
          <div class="bg-white rounded-lg shadow p-4 mb-6">
            <h3 class="font-semibold text-gray-900 mb-3">标签过滤</h3>
            <div class="space-y-2">
              <label
                v-for="tag in notesStore.tags"
                :key="tag.id"
                class="flex items-center cursor-pointer hover:bg-gray-50 p-2 rounded-lg"
              >
                <input
                  type="checkbox"
                  :value="tag.name"
                  v-model="selectedTags"
                  class="text-primary-600 rounded focus:ring-primary-500"
                />
                <span
                  :style="{ color: tag.color }"
                  class="ml-2 text-sm font-medium"
                >
                  {{ tag.name }}
                </span>
                <span class="ml-auto text-xs text-gray-500">
                  {{ tag.count || 0 }}
                </span>
              </label>
            </div>
          </div>

          <!-- 排序选项 -->
          <div class="bg-white rounded-lg shadow p-4">
            <h3 class="font-semibold text-gray-900 mb-3">排序方式</h3>
            <select 
              v-model="sortBy" 
              class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value="updatedAt">最近更新</option>
              <option value="createdAt">创建时间</option>
              <option value="title">标题排序</option>
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

          <!-- 笔记网格 -->
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div
              v-for="note in paginatedNotes"
              :key="note.id"
              class="relative"
            >
              <!-- 选择框 -->
              <div class="absolute top-2 left-2 z-10">
                <input
                  type="checkbox"
                  :value="note.id"
                  v-model="selectedNotes"
                  class="h-4 w-4 text-primary-600 rounded focus:ring-primary-500"
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
const perPage = 9
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
      notificationStore.success('批量删除成功', `已删除 ${selectedNotes.value.length} 篇笔记`)
      selectedNotes.value = []
    } else {
      await notesStore.deleteNote(deleteTarget.value.id)
      notificationStore.success('删除成功')
    }

    closeDeleteConfirm()

    // 如果当前页没有笔记了，返回上一页
    if (paginatedNotes.value.length === 0 && currentPage.value > 1) {
      currentPage.value--
    }
  } catch (error) {
    notificationStore.error('删除失败', error.message)
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