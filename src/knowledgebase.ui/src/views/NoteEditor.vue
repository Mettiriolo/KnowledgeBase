<template>
  <Layout>
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <form @submit.prevent="saveNote" class="space-y-6">
        <!-- 编辑器头部 -->
        <div class="flex items-center justify-between">
          <h1 class="text-2xl font-bold text-gray-900">
            {{ isEditMode ? '编辑笔记' : '创建新笔记' }}
          </h1>
          <div class="flex items-center space-x-3">
            <button
              type="button"
              @click="saveDraft"
              :disabled="saving"
              class="inline-flex items-center px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" />
              </svg>
              保存草稿
            </button>
            <button
              type="submit"
              :disabled="saving || !isValid"
              class="inline-flex items-center px-4 py-2 text-white bg-primary-600 border border-transparent rounded-lg hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <LoadingSpinner v-if="saving" size="small" color="white" class="mr-2" />
              {{ isEditMode ? '更新笔记' : '发布笔记' }}
            </button>
          </div>
        </div>

        <!-- 标题输入 -->
        <div>
          <input
            v-model="form.title"
            type="text"
            placeholder="输入笔记标题..."
            class="w-full text-2xl font-semibold border-0 border-b-2 border-gray-200 focus:border-primary-500 focus:ring-0 pb-2 placeholder-gray-400"
            required
            autofocus
          />
        </div>

        <!-- 标签输入 -->
        <div>
          <TagInput
            v-model="form.tags"
            placeholder="添加标签（按Enter确认）"
            :suggestions="notesStore.tags"
          />
        </div>

        <!-- 编辑器区域 -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 min-h-[600px]">
          <!-- 编辑器 -->
          <div class="bg-white rounded-lg shadow-sm border overflow-hidden flex flex-col">
            <!-- 工具栏 -->
            <div class="bg-gray-50 px-4 py-2 border-b flex items-center justify-between">
              <span class="text-sm font-medium text-gray-700">Markdown 编辑器</span>
              <div class="flex items-center space-x-1">
                <button
                  type="button"
                  @click="insertMarkdown('**', '**')"
                  class="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded transition-colors"
                  title="粗体 (Ctrl+B)"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 5H7v14h6m0-7h6m-6-7v14m6-7H7" />
                  </svg>
                </button>
                <button
                  type="button"
                  @click="insertMarkdown('*', '*')"
                  class="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded transition-colors"
                  title="斜体 (Ctrl+I)"
                >
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M8 2a1 1 0 011 1v2H7V3a1 1 0 011-1zm3 0a1 1 0 011 1v2h2V3a1 1 0 011-1h1a1 1 0 110 2h-.293l-2 8H15a1 1 0 110 2h-1a1 1 0 01-1-1v-2h-2v2a1 1 0 01-1 1H9a1 1 0 110-2h.293l2-8H9a1 1 0 110-2h1a1 1 0 011-1h1z" clip-rule="evenodd" />
                  </svg>
                </button>
                <button
                  type="button"
                  @click="insertMarkdown('`', '`')"
                  class="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded transition-colors"
                  title="代码"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
                  </svg>
                </button>
                <button
                  type="button"
                  @click="insertLink"
                  class="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded transition-colors"
                  title="链接 (Ctrl+K)"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
                  </svg>
                </button>
                <div class="w-px h-4 bg-gray-300 mx-1"></div>
                <button
                  type="button"
                  @click="insertMarkdown('# ', '')"
                  class="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded transition-colors"
                  title="标题"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z" />
                  </svg>
                </button>
                <button
                  type="button"
                  @click="insertMarkdown('- ', '')"
                  class="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded transition-colors"
                  title="列表"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                  </svg>
                </button>
                <button
                  type="button"
                  @click="insertMarkdown('> ', '')"
                  class="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-200 rounded transition-colors"
                  title="引用"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h8m-8 4h4" />
                  </svg>
                </button>
              </div>
            </div>
            <!-- 文本编辑区 -->
            <textarea
              ref="editorRef"
              v-model="form.content"
              placeholder="开始写作..."
              class="flex-1 w-full p-4 resize-none focus:outline-none font-mono text-sm"
              @input="handleInput"
              @keydown="handleKeydown"
            />
          </div>

          <!-- 预览 -->
          <div class="bg-white rounded-lg shadow-sm border overflow-hidden flex flex-col">
            <div class="bg-gray-50 px-4 py-2 border-b flex items-center justify-between">
              <span class="text-sm font-medium text-gray-700">预览</span>
              <div class="flex items-center space-x-2">
                <span class="text-xs text-gray-500">实时预览</span>
              </div>
            </div>
            <div class="flex-1 p-4 overflow-y-auto">
              <div
                v-if="form.content"
                class="prose prose-slate max-w-none"
                v-html="preview"
              />
              <div v-else class="text-gray-400 text-center py-12">
                <svg class="mx-auto h-12 w-12 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <p class="mt-2 text-sm">预览区域</p>
              </div>
            </div>
          </div>
        </div>

        <!-- 底部信息栏 -->
        <div class="bg-gray-50 rounded-lg p-4 flex items-center justify-between text-sm text-gray-600">
          <div class="flex items-center space-x-4">
            <span>{{ wordCount }} 字</span>
            <span>•</span>
            <span>{{ characterCount }} 字符</span>
            <span>•</span>
            <span>预计阅读时间 {{ readingTime }} 分钟</span>
          </div>
          <div class="flex items-center space-x-4">
            <span v-if="lastSaved">
              上次保存：{{ formatRelativeTime(lastSaved) }}
            </span>
            <span v-if="autoSaveEnabled" class="flex items-center text-green-600">
              <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
              </svg>
              自动保存已开启
            </span>
          </div>
        </div>
      </form>

      <!-- AI建议浮动按钮 -->
      <div class="fixed bottom-6 right-6 z-40">
        <button
          @click="showAISuggestions = true"
          class="p-4 bg-purple-600 text-white rounded-full shadow-lg hover:bg-purple-700 transition-colors group"
          title="AI写作建议"
        >
          <svg class="w-6 h-6 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
          </svg>
        </button>
      </div>
    </div>

    <!-- AI建议对话框 -->
    <teleport to="body">
      <div v-if="showAISuggestions" class="fixed inset-0 z-50 overflow-y-auto">
        <!-- 背景遮罩 -->
        <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" @click="showAISuggestions = false"></div>
        
        <!-- 对话框容器 -->
        <div class="flex min-h-screen items-center justify-center p-4">
          <!-- 对话框 -->
          <div class="relative bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-hidden">
            <!-- 对话框头部 -->
            <div class="px-6 py-4 border-b">
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-bold">AI写作助手</h3>
                <button @click="showAISuggestions = false" class="text-gray-500 hover:text-gray-700">
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <!-- 对话框内容 -->
            <div class="p-6 overflow-y-auto max-h-[calc(90vh-8rem)]">
              <div class="space-y-4">
                <!-- AI功能按钮 -->
                <div class="grid grid-cols-2 gap-3">
                  <button
                    @click="improveWriting"
                    :disabled="aiStore.isThinking || !form.content"
                    class="p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-purple-500 hover:bg-purple-50 transition-all text-left disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <svg class="w-5 h-5 text-purple-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    <p class="text-sm font-medium text-gray-700">优化文本</p>
                    <p class="text-xs text-gray-500 mt-1">改进语言表达和结构</p>
                  </button>

                  <button
                    @click="expandContent"
                    :disabled="aiStore.isThinking || !form.content"
                    class="p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-blue-500 hover:bg-blue-50 transition-all text-left disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <svg class="w-5 h-5 text-blue-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
                    </svg>
                    <p class="text-sm font-medium text-gray-700">扩展内容</p>
                    <p class="text-xs text-gray-500 mt-1">增加更多细节和说明</p>
                  </button>

                  <button
                    @click="suggestOutline"
                    :disabled="aiStore.isThinking"
                    class="p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-green-500 hover:bg-green-50 transition-all text-left disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <svg class="w-5 h-5 text-green-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                    </svg>
                    <p class="text-sm font-medium text-gray-700">生成大纲</p>
                    <p class="text-xs text-gray-500 mt-1">创建文章结构框架</p>
                  </button>

                  <button
                    @click="checkGrammar"
                    :disabled="aiStore.isThinking || !form.content"
                    class="p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-orange-500 hover:bg-orange-50 transition-all text-left disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <svg class="w-5 h-5 text-orange-600 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <p class="text-sm font-medium text-gray-700">语法检查</p>
                    <p class="text-xs text-gray-500 mt-1">检查拼写和语法错误</p>
                  </button>
                </div>

                <!-- AI响应区域 -->
                <div v-if="aiSuggestion" class="bg-gray-50 p-4 rounded-lg">
                  <div class="flex items-center justify-between mb-3">
                    <h4 class="font-medium text-gray-900">AI建议</h4>
                    <div class="flex items-center space-x-2">
                      <button
                        v-if="canApplySuggestion"
                        @click="applySuggestion"
                        class="px-3 py-1 text-sm text-white bg-primary-600 rounded-lg hover:bg-primary-700 transition-colors"
                      >
                        应用建议
                      </button>
                      <button
                        @click="aiSuggestion = ''"
                        class="px-3 py-1 text-sm text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                      >
                        清除
                      </button>
                    </div>
                  </div>
                  <div class="prose prose-sm max-w-none text-gray-700" v-html="aiSuggestionHtml" />
                </div>

                <!-- 加载状态 -->
                <div v-if="aiStore.isThinking" class="text-center py-6">
                  <LoadingSpinner />
                  <p class="mt-3 text-sm text-gray-500">AI正在思考...</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </teleport>

    <!-- 离开确认 -->
    <teleport to="body">
      <div v-if="showLeaveConfirm" class="fixed inset-0 z-50 overflow-y-auto">
        <!-- 背景遮罩 -->
        <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity"></div>
        
        <!-- 对话框容器 -->
        <div class="flex min-h-screen items-center justify-center p-4">
          <!-- 对话框 -->
          <div class="relative bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <h3 class="text-lg font-bold text-gray-900 mb-4">未保存的更改</h3>
            <p class="text-gray-600 mb-6">
              您有未保存的更改，确定要离开吗？
            </p>
            <div class="flex justify-end space-x-3">
              <button 
                @click="cancelLeave" 
                class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                继续编辑
              </button>
              <button 
                @click="confirmLeave" 
                class="px-4 py-2 text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors"
              >
                放弃更改
              </button>
            </div>
          </div>
        </div>
      </div>
    </teleport>
  </Layout>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount } from 'vue'
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router'
import { useNotesStore } from '@/stores/notes'
import { useAIStore } from '@/stores/ai'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import TagInput from '@/components/Common/TagInput.vue'
import { getWordCount, getCharacterCount, calculateReadingTime } from '@/utils/text'
import { formatRelativeTime } from '@/utils/date'
import { marked } from 'marked'
import DOMPurify from 'dompurify'
import { debounce } from 'lodash-es'

const route = useRoute()
const router = useRouter()
const notesStore = useNotesStore()
const aiStore = useAIStore()
const notificationStore = useNotificationStore()

// 状态
const editorRef = ref(null)
const saving = ref(false)
const autoSaveEnabled = ref(true)
const lastSaved = ref(null)
const showAISuggestions = ref(false)
const showLeaveConfirm = ref(false)
const aiSuggestion = ref('')
const canApplySuggestion = ref(false)
const hasUnsavedChanges = ref(false)
const leaveCallback = ref(null)

// 表单数据
const form = reactive({
  title: '',
  content: '',
  tags: []
})

// 原始数据用于比较
const originalForm = reactive({
  title: '',
  content: '',
  tags: []
})

// 计算属性
const isEditMode = computed(() => !!route.params.id)
const noteId = computed(() => route.params.id)

const isValid = computed(() => {
  return form.title.trim().length > 0 && form.content.trim().length > 0
})

const preview = computed(() => {
  if (!form.content) return ''
  const html = marked(form.content)
  return DOMPurify.sanitize(html)
})

const aiSuggestionHtml = computed(() => {
  if (!aiSuggestion.value) return ''
  const html = marked(aiSuggestion.value)
  return DOMPurify.sanitize(html)
})

const wordCount = computed(() => getWordCount(form.content))
const characterCount = computed(() => getCharacterCount(form.content))
const readingTime = computed(() => calculateReadingTime(form.content))

// 监听表单变化
watch(() => form, (newVal) => {
  hasUnsavedChanges.value =
    newVal.title !== originalForm.title ||
    newVal.content !== originalForm.content ||
    JSON.stringify(newVal.tags.sort()) !== JSON.stringify(originalForm.tags.sort())
}, { deep: true })

// 自动保存
const autoSave = debounce(async () => {
  if (autoSaveEnabled.value && isValid.value && hasUnsavedChanges.value) {
    await saveDraft()
  }
}, 3000)

watch(() => form.content, () => {
  if (autoSaveEnabled.value) {
    autoSave()
  }
})

// 方法
const saveNote = async () => {
  if (!isValid.value) return

  saving.value = true
  try {
    const noteData = {
      title: form.title,
      content: form.content,
      tags: form.tags
    }

    if (isEditMode.value) {
      await notesStore.updateNote(noteId.value, noteData)
      notificationStore.success('笔记更新成功')
    } else {
      const newNote = await notesStore.createNote(noteData)
      notificationStore.success('笔记创建成功')
      router.push(`/notes/${newNote.id}`)
    }

    hasUnsavedChanges.value = false
    updateOriginalForm()
  } catch (error) {
    notificationStore.error('保存失败', error.message)
  } finally {
    saving.value = false
  }
}

const saveDraft = async () => {
  if (!isValid.value) return

  try {
    const draftData = {
      title: form.title,
      content: form.content,
      tags: form.tags,
      isDraft: true
    }

    if (isEditMode.value) {
      await notesStore.updateNote(noteId.value, draftData)
    } else {
      await notesStore.createNote(draftData)
    }

    lastSaved.value = new Date()
    hasUnsavedChanges.value = false
    updateOriginalForm()
    notificationStore.success('草稿已保存')
  } catch (error) {
    notificationStore.error('保存草稿失败', error.message)
  }
}

const insertMarkdown = (before, after) => {
  const textarea = editorRef.value
  const start = textarea.selectionStart
  const end = textarea.selectionEnd
  const selectedText = form.content.slice(start, end)
  const replacement = before + selectedText + after
  form.content = form.content.slice(0, start) + replacement + form.content.slice(end)
  
  // 设置光标位置
  setTimeout(() => {
    textarea.focus()
    const newPosition = start + before.length + (selectedText.length || 0)
    textarea.setSelectionRange(newPosition, newPosition)
  }, 0)
}

const insertLink = () => {
  const url = prompt('请输入链接地址:', 'https://')
  if (url) {
    insertMarkdown('[', `](${url})`)
  }
}

const handleInput = () => {
  autoSave()
}

const handleKeydown = (e) => {
  // Tab键插入空格
  if (e.key === 'Tab') {
    e.preventDefault()
    const start = e.target.selectionStart
    const end = e.target.selectionEnd
    form.content = form.content.substring(0, start) + '  ' + form.content.substring(end)
    e.target.selectionStart = e.target.selectionEnd = start + 2
  }
  
  // 快捷键
  if (e.ctrlKey || e.metaKey) {
    switch(e.key) {
      case 'b':
        e.preventDefault()
        insertMarkdown('**', '**')
        break
      case 'i':
        e.preventDefault()
        insertMarkdown('*', '*')
        break
      case 'k':
        e.preventDefault()
        insertLink()
        break
      case 's':
        e.preventDefault()
        saveNote()
        break
    }
  }
}

const improveWriting = async () => {
  try {
    const response = await aiStore.improveWriting(form.content)
    aiSuggestion.value = response.improvedContent
    canApplySuggestion.value = true
  } catch (error) {
    notificationStore.error('优化文本失败', error.message)
  }
}

const expandContent = async () => {
  try {
    const response = await aiStore.expandContent(form.content)
    aiSuggestion.value = response.expandedContent
    canApplySuggestion.value = true
  } catch (error) {
    notificationStore.error('扩展内容失败', error.message)
  }
}

const suggestOutline = async () => {
  try {
    const response = await aiStore.suggestOutline(form.title, form.content)
    aiSuggestion.value = response.outline
    canApplySuggestion.value = false
  } catch (error) {
    notificationStore.error('生成大纲失败', error.message)
  }
}

const checkGrammar = async () => {
  try {
    const response = await aiStore.checkGrammar(form.content)
    aiSuggestion.value = response.corrections
    canApplySuggestion.value = false
  } catch (error) {
    notificationStore.error('语法检查失败', error.message)
  }
}

const applySuggestion = () => {
  form.content = aiSuggestion.value
  aiSuggestion.value = ''
  canApplySuggestion.value = false
  showAISuggestions.value = false
  notificationStore.success('已应用AI建议')
}

const updateOriginalForm = () => {
  originalForm.title = form.title
  originalForm.content = form.content
  originalForm.tags = [...form.tags]
}

const loadNote = async () => {
  try {
    const note = await notesStore.getNote(noteId.value)
    form.title = note.title
    form.content = note.content
    form.tags = note.tags.map(tag => tag.name)
    updateOriginalForm()
  } catch (error) {
    notificationStore.error('加载笔记失败', error.message)
    router.push('/notes')
  }
}

const cancelLeave = () => {
  showLeaveConfirm.value = false
  leaveCallback.value = null
}

const confirmLeave = () => {
  showLeaveConfirm.value = false
  if (leaveCallback.value) {
    leaveCallback.value()
  }
}

// 生命周期钩子
onMounted(() => {
  if (isEditMode.value) {
    loadNote()
  }
  
  // 监听浏览器关闭事件
  window.addEventListener('beforeunload', handleBeforeUnload)
})

onBeforeUnmount(() => {
  autoSave.cancel()
  window.removeEventListener('beforeunload', handleBeforeUnload)
})

// 浏览器关闭提醒
const handleBeforeUnload = (e) => {
  if (hasUnsavedChanges.value) {
    e.preventDefault()
    e.returnValue = ''
  }
}

// 路由守卫
onBeforeRouteLeave((to, from, next) => {
  if (hasUnsavedChanges.value) {
    showLeaveConfirm.value = true
    leaveCallback.value = () => next()
    next(false)
  } else {
    next()
  }
})
</script>

<style scoped>
/* Markdown预览样式 */
.prose :deep(pre) {
  background-color: #1e293b;
  color: #e2e8f0;
  padding: 1rem;
  border-radius: 0.375rem;
  overflow-x: auto;
}

.prose :deep(pre code) {
  background-color: transparent;
  padding: 0;
  color: inherit;
  font-size: 0.875em;
}

.prose :deep(code) {
  background-color: #f3f4f6;
  padding: 0.2rem 0.4rem;
  border-radius: 0.25rem;
  font-size: 0.875em;
  color: #dc2626;
}

.prose :deep(blockquote) {
  border-left: 4px solid #e5e7eb;
  padding-left: 1rem;
  font-style: italic;
  color: #4b5563;
  margin: 1rem 0;
}

.prose :deep(h1) {
  font-size: 2rem;
  font-weight: 700;
  margin-top: 0;
  margin-bottom: 1rem;
  line-height: 1.2;
}

.prose :deep(h2) {
  font-size: 1.5rem;
  font-weight: 600;
  margin-top: 2rem;
  margin-bottom: 1rem;
  line-height: 1.3;
}

.prose :deep(h3) {
  font-size: 1.25rem;
  font-weight: 600;
  margin-top: 1.5rem;
  margin-bottom: 0.75rem;
  line-height: 1.4;
}

.prose :deep(ul),
.prose :deep(ol) {
  padding-left: 1.5rem;
  margin: 1rem 0;
}

.prose :deep(li) {
  margin: 0.5rem 0;
}

.prose :deep(table) {
  border-collapse: collapse;
  width: 100%;
  margin: 1rem 0;
}

.prose :deep(th),
.prose :deep(td) {
  border: 1px solid #e5e7eb;
  padding: 0.5rem 1rem;
  text-align: left;
}

.prose :deep(th) {
  background-color: #f3f4f6;
  font-weight: 600;
}

.prose :deep(tr:nth-child(even)) {
  background-color: #f9fafb;
}

.prose :deep(img) {
  max-width: 100%;
  height: auto;
  border-radius: 0.375rem;
  margin: 1rem 0;
}

.prose :deep(a) {
  color: #2563eb;
  text-decoration: underline;
}

.prose :deep(a:hover) {
  color: #1d4ed8;
}

/* 编辑器字体 */
textarea {
  font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', 'Consolas', 'source-code-pro', monospace;
  line-height: 1.6;
}

/* 动画效果 */
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

.relative {
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

/* 响应式调整 */
@media (max-width: 1024px) {
  .grid {
    grid-template-columns: 1fr;
  }
  
  .min-h-\[600px\] {
    min-height: 400px;
  }
}
</style>