<template>
  <Layout>
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 加载状态 -->
      <div v-if="loading" class="text-center py-12">
        <LoadingSpinner />
        <p class="mt-3 text-gray-500">加载中...</p>
      </div>

      <!-- 笔记内容 -->
      <article v-else-if="note" class="bg-white rounded-lg shadow-lg overflow-hidden">
        <!-- 笔记头部 -->
        <div class="p-6 lg:p-8 border-b">
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <h1 class="text-3xl font-bold text-gray-900 mb-2">
                {{ note.title }}
              </h1>
              <div class="flex items-center text-sm text-gray-500 space-x-4">
                <span>创建于 {{ formatDate(note.createdAt) }}</span>
                <span>•</span>
                <span>更新于 {{ formatDate(note.updatedAt) }}</span>
                <span>•</span>
                <span>{{ wordCount }} 字</span>
              </div>
            </div>

            <!-- 操作按钮 -->
            <div class="flex items-center space-x-2 ml-4">
              <button
                @click="copyLink"
                class="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                title="复制链接"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5H6a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2v-1M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 012-2h2a2 2 0 012 2m0 0h2a2 2 0 012 2v3m2 4H10m0 0l3-3m-3 3l3 3" />
                </svg>
              </button>
              <router-link
                :to="`/notes/${note.id}/edit`"
                class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                title="编辑"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
              </router-link>
              <button
                @click="showDeleteConfirm = true"
                class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                title="删除"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </div>
          </div>

          <!-- 标签 -->
          <div v-if="note.tags && note.tags.length > 0" class="mt-4 flex flex-wrap gap-2">
            <router-link
              v-for="tag in note.tags"
              :key="tag.id"
              :to="`/notes?tag=${tag.name}`"
              :style="{ backgroundColor: tag.color + '20', color: tag.color }"
              class="px-3 py-1 rounded-full text-sm font-medium hover:opacity-80 transition-opacity"
            >
              {{ tag.name }}
            </router-link>
          </div>
        </div>

        <!-- AI生成的摘要 -->
        <div v-if="note.summary" class="p-6 lg:px-8 bg-gradient-to-r from-purple-50 to-primary-50 border-b">
          <div class="flex items-start space-x-3">
            <div class="flex-shrink-0">
              <div class="p-2 bg-white rounded-lg shadow-sm">
                <svg class="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                </svg>
              </div>
            </div>
            <div class="flex-1">
              <h3 class="text-sm font-semibold text-gray-900 mb-1">AI 摘要</h3>
              <p class="text-sm text-gray-700 leading-relaxed">{{ note.summary }}</p>
            </div>
          </div>
        </div>

        <!-- 笔记正文 -->
        <div class="p-6 lg:p-8">
          <div
            v-if="renderFormat === 'markdown'"
            class="prose prose-slate max-w-none"
            v-html="renderedContent"
          />
          <div
            v-else
            class="whitespace-pre-wrap text-gray-800"
          >
            {{ note.content }}
          </div>
        </div>

        <!-- 底部信息 -->
        <div class="p-6 lg:px-8 border-t bg-gray-50">
          <div class="flex items-center justify-between text-sm text-gray-500">
            <div class="flex items-center space-x-4">
              <span>阅读时间约 {{ readingTime }} 分钟</span>
              <span>•</span>
              <span>{{ characterCount }} 字符</span>
            </div>
            <div class="flex items-center space-x-2">
              <button
                @click="toggleFormat"
                class="text-primary-600 hover:text-primary-800"
              >
                {{ renderFormat === 'markdown' ? '查看源文本' : '查看渲染效果' }}
              </button>
            </div>
          </div>
        </div>
      </article>

      <!-- 404 状态 -->
      <div v-else class="text-center py-12">
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900">笔记不存在</h3>
        <p class="mt-1 text-sm text-gray-500">可能已被删除或链接错误</p>
        <div class="mt-6">
          <router-link to="/notes" class="btn btn-primary">
            返回笔记列表
          </router-link>
        </div>
      </div>

      <!-- AI助手浮动按钮 -->
      <div class="fixed bottom-6 right-6">
        <button
          @click="showAIAssistant = true"
          class="p-4 bg-purple-600 text-white rounded-full shadow-lg hover:bg-purple-700 transition-colors"
          title="AI助手"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
          </svg>
        </button>
      </div>
    </div>

    <!-- 删除确认对话框 -->
    <teleport to="body">
      <div v-if="showDeleteConfirm" class="fixed inset-0 z-50 overflow-y-auto">
        <!-- 背景遮罩 -->
        <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" @click="showDeleteConfirm = false"></div>
        
        <!-- 对话框容器 -->
        <div class="flex min-h-screen items-center justify-center p-4">
          <!-- 对话框 -->
          <div class="relative bg-white rounded-lg shadow-xl max-w-md w-full p-6 transform transition-all">
            <h3 class="text-lg font-bold text-gray-900 mb-4">确认删除</h3>
            <p class="text-gray-600 mb-6">
              确定要删除笔记"{{ note?.title }}"吗？此操作无法撤销。
            </p>
            <div class="flex justify-end space-x-3">
              <button 
                @click="showDeleteConfirm = false" 
                class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                取消
              </button>
              <button 
                @click="deleteNote" 
                class="px-4 py-2 text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors"
              >
                确认删除
              </button>
            </div>
          </div>
        </div>
      </div>
    </teleport>

    <!-- AI助手对话框 -->
    <teleport to="body">
      <div v-if="showAIAssistant" class="fixed inset-0 z-50 overflow-y-auto">
        <!-- 背景遮罩 -->
        <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" @click="showAIAssistant = false"></div>
        
        <!-- 对话框容器 -->
        <div class="flex min-h-screen items-center justify-center p-4">
          <!-- 对话框 -->
          <div class="relative bg-white rounded-lg shadow-xl max-w-2xl w-full p-6 transform transition-all">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-lg font-bold">AI助手 - 关于这篇笔记</h3>
              <button @click="showAIAssistant = false" class="text-gray-500 hover:text-gray-700">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div class="space-y-4">
              <!-- 快速操作 -->
              <div class="grid grid-cols-2 gap-3">
                <button
                  @click="generateSummary"
                  :disabled="aiStore.isThinking"
                  class="p-3 border-2 border-dashed border-gray-300 rounded-lg hover:border-purple-500 hover:bg-purple-50 transition-all text-left"
                >
                  <svg class="w-5 h-5 text-purple-600 mb-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  <p class="text-sm font-medium text-gray-700">生成摘要</p>
                </button>

                <button
                  @click="extractKeywords"
                  :disabled="aiStore.isThinking"
                  class="p-3 border-2 border-dashed border-gray-300 rounded-lg hover:border-green-500 hover:bg-green-50 transition-all text-left"
                >
                  <svg class="w-5 h-5 text-green-600 mb-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                  </svg>
                  <p class="text-sm font-medium text-gray-700">提取关键词</p>
                </button>
              </div>

              <!-- AI响应区域 -->
              <div v-if="aiResponse" class="bg-gray-50 p-4 rounded-lg">
                <p class="text-sm text-gray-700 whitespace-pre-wrap">{{ aiResponse }}</p>
              </div>

              <!-- 自由提问 -->
              <div class="flex space-x-2">
                <input
                  v-model="aiQuestion"
                  @keyup.enter="askAboutNote"
                  placeholder="询问关于这篇笔记的任何问题..."
                  class="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                  :disabled="aiStore.isThinking"
                />
                <button
                  @click="askAboutNote"
                  :disabled="!aiQuestion.trim() || aiStore.isThinking"
                  class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  <LoadingSpinner v-if="aiStore.isThinking" size="small" color="white" />
                  <span v-else>发送</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </teleport>
  </Layout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useNotesStore } from '@/stores/notes'
import { useAIStore } from '@/stores/ai'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import { formatDate } from '@/utils/date'
import { getWordCount, getCharacterCount, calculateReadingTime } from '@/utils/text'
import { marked } from 'marked'
import DOMPurify from 'dompurify'

const route = useRoute()
const router = useRouter()
const notesStore = useNotesStore()
const aiStore = useAIStore()
const notificationStore = useNotificationStore()

const note = ref(null)
const loading = ref(true)
const showDeleteConfirm = ref(false)
const showAIAssistant = ref(false)
const aiQuestion = ref('')
const aiResponse = ref('')
const renderFormat = ref('markdown')

const noteId = computed(() => route.params.id)
const wordCount = computed(() => note.value ? getWordCount(note.value.content) : 0)
const characterCount = computed(() => note.value ? getCharacterCount(note.value.content) : 0)
const readingTime = computed(() => note.value ? calculateReadingTime(note.value.content) : 0)

const renderedContent = computed(() => {
  if (!note.value) return ''
  const html = marked(note.value.content)
  return DOMPurify.sanitize(html)
})

const toggleFormat = () => {
  renderFormat.value = renderFormat.value === 'markdown' ? 'plain' : 'markdown'
}

const copyLink = async () => {
  try {
    const url = window.location.href
    await navigator.clipboard.writeText(url)
    notificationStore.success('链接已复制到剪贴板')
  } catch {
    notificationStore.error('复制失败')
  }
}

const deleteNote = async () => {
  try {
    await notesStore.deleteNote(noteId.value)
    notificationStore.success('笔记已删除')
    router.push('/notes')
  } catch (error) {
    notificationStore.error('删除失败', error.message)
  }
}

const generateSummary = async () => {
  try {
    aiResponse.value = ''
    const response = await aiStore.generateSummary(note.value)
    aiResponse.value = response.summary

    // 更新笔记摘要
    await notesStore.updateNote(noteId.value, {
      summary: response.summary
    })

    note.value.summary = response.summary
    notificationStore.success('摘要生成成功')
  } catch (error) {
    notificationStore.error('生成摘要失败', error.message)
  }
}

const extractKeywords = async () => {
  try {
    aiResponse.value = ''
    const response = await aiStore.extractKeywords(note.value.content)
    aiResponse.value = `提取的关键词：\n${response.keywords.join(', ')}`
  } catch (error) {
    notificationStore.error('提取关键词失败', error.message)
  }
}

const askAboutNote = async () => {
  if (!aiQuestion.value.trim()) return

  try {
    aiResponse.value = ''
    const response = await aiStore.askAboutNote(note.value, aiQuestion.value)
    aiResponse.value = response.answer
    aiQuestion.value = ''
  } catch (error) {
    notificationStore.error('AI问答失败', error.message)
  }
}

onMounted(async () => {
  try {
      note.value = await notesStore.getNote(noteId.value)
  } catch (error) {
    console.error('加载笔记失败:', error)
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
/* 添加动画效果 */
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

/* 对话框动画 */
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