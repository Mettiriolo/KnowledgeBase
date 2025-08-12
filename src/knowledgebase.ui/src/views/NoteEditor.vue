<template>
  <Layout>
    <!-- 现代化编辑器布局 -->
    <div class="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50/10 to-indigo-50/20">
      <!-- 顶部工具栏 - 重新设计 -->
      <div class="bg-white/80 backdrop-blur-md border-b border-white/60 sticky top-0 z-10 shadow-lg">
        <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex items-center justify-between h-20">
            <!-- 左侧 -->
            <div class="flex items-center space-x-6">
              <button
                @click="$router.back()"
                class="p-3 text-gray-400 hover:text-gray-600 rounded-xl hover:bg-white/60 transition-all duration-200 hover:scale-110"
              >
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div class="flex items-center space-x-4">
                <div class="flex items-center space-x-3">
                  <div class="p-2 bg-gradient-to-r from-primary-500 to-purple-500 rounded-xl">
                    <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </div>
                  <div>
                    <h1 class="text-xl font-bold text-gray-900">
                      {{ isEditMode ? '编辑笔记' : '创建新笔记' }}
                    </h1>
                    <div class="flex items-center space-x-2 text-sm">
                      <span v-if="draftNoteId" class="px-2 py-1 bg-amber-100 text-amber-700 rounded-full text-xs font-medium">
                        草稿
                      </span>
                      <span v-if="hasUnsavedChanges" class="flex items-center text-amber-600">
                        <span class="w-2 h-2 bg-amber-400 rounded-full mr-2 animate-pulse"></span>
                        有未保存更改
                      </span>
                      <span v-else-if="lastSaved" class="flex items-center text-green-600">
                        <span class="w-2 h-2 bg-green-400 rounded-full mr-2"></span>
                        已保存
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- 右侧 -->
            <div class="flex items-center space-x-6">
              <!-- 状态信息 -->
              <div class="hidden sm:flex items-center space-x-4">
                <div class="flex items-center px-3 py-2 bg-white/60 rounded-xl">
                  <svg class="w-4 h-4 mr-2 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  <span class="text-sm font-medium text-gray-700">{{ wordCount }} 字</span>
                </div>
              </div>
              
              <!-- 保存按钮 - 重新设计 -->
              <button
                type="submit"
                form="note-form"
                :disabled="saving || !isValid"
                class="group relative inline-flex items-center px-6 py-3 bg-gradient-to-r from-primary-500 to-purple-500 text-white font-semibold rounded-2xl shadow-lg hover:shadow-xl disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-300 transform hover:-translate-y-0.5"
              >
                <div class="absolute inset-0 bg-gradient-to-r from-primary-600 to-purple-600 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity"></div>
                <div class="relative flex items-center">
                  <LoadingSpinner v-if="saving" size="small" color="white" class="mr-3" />
                  <div v-else class="p-1 bg-white/20 rounded-lg mr-3 group-hover:scale-110 transition-transform">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                    </svg>
                  </div>
                  {{ saving ? '保存中...' : '保存笔记' }}
                </div>
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <!-- 编辑区域 - 现代化设计 -->
      <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <form id="note-form" @submit.prevent="saveNote" class="space-y-8">

          <!-- 标题和标签区域 -->
          <div class="bg-white/80 backdrop-blur-sm rounded-3xl shadow-xl border border-white/60 p-8">
            <!-- 标题输入 -->
            <div class="mb-6">
              <input
                v-model="form.title"
                type="text"
                placeholder="为您的想法起个精彩的标题..."
                class="w-full text-3xl font-bold border-0 focus:ring-0 p-0 placeholder-gray-400 bg-transparent focus:placeholder-gray-300 transition-colors"
                required
                autofocus
              />
            </div>
            
            <!-- 标签输入区域 -->
            <div class="pt-6 border-t border-gray-100">
              <div class="flex items-center mb-3">
                <svg class="w-5 h-5 mr-3 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                </svg>
                <span class="text-sm font-semibold text-gray-600">标签</span>
              </div>
              <TagInput
                v-model="form.tags"
                placeholder="添加标签帮助整理笔记..."
                :suggestions="notesStore.tags"
                class="border-0 focus:ring-0 p-0 bg-transparent"
              />
            </div>
          </div>

          <!-- 编辑器区域 -->
          <div class="bg-white/80 backdrop-blur-sm rounded-3xl shadow-xl border border-white/60 overflow-hidden">
            <div class="p-2">
              <div ref="editorRef" class="toast-ui-editor min-h-96"></div>
            </div>
          </div>

        </form>
      </div>

      <!-- AI助手按钮 - 简化后 -->
      <div class="fixed bottom-8 right-8 z-40">
        <div class="flex flex-col space-y-3">
          <!-- AI摘要 -->
          <button
            v-if="form.content.trim().length > 100"
            @click="generateSummary"
            :disabled="aiStore.isThinking"
            class="p-3 bg-green-600 text-white rounded-full shadow-lg hover:bg-green-700 transition-all group"
            title="生成摘要"
          >
            <LoadingSpinner v-if="aiStore.isThinking" size="small" color="white" />
            <svg v-else class="w-5 h-5 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </button>
          
          <!-- AI问答 -->
          <button
            @click="showAIChat = true"
            class="p-3 bg-purple-600 text-white rounded-full shadow-lg hover:bg-purple-700 transition-all group"
            title="AI问答"
          >
            <svg class="w-5 h-5 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
          </button>
        </div>
      </div>
      
      <!-- AI对话模态框 - 精简版 -->
      <teleport to="body">
        <div v-if="showAIChat" class="fixed inset-0 bg-black bg-opacity-50 flex items-end sm:items-center justify-center z-50" @click.self="showAIChat = false">
          <div class="bg-white rounded-t-2xl sm:rounded-2xl w-full sm:max-w-md max-h-96 sm:max-h-80 overflow-hidden animate-slide-up">
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
                  placeholder="问问关于这篇笔记的问题..."
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
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router'
import { useNotesStore } from '@/stores/notes'
import { useAIStore } from '@/stores/ai'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import TagInput from '@/components/Common/TagInput.vue'
import { getWordCount, getCharacterCount, calculateReadingTime } from '@/utils/text'
import { formatRelativeTime } from '@/utils/date'
import { debounce } from 'lodash-es'

// 直接导入 ToastUI 组件和插件
import { Editor } from '@toast-ui/editor';
import codeSyntaxHighlight from '@toast-ui/editor-plugin-code-syntax-highlight';

// 定义插件数组
const plugins = [
  codeSyntaxHighlight
];

const route = useRoute()
const router = useRouter()
const notesStore = useNotesStore()
const aiStore = useAIStore()
const notificationStore = useNotificationStore()

// 状态
const editorRef = ref(null)
const editorInstance = ref(null)
const saving = ref(false)
const autoSaveEnabled = ref(true)
const lastSaved = ref(null)
const showAIChat = ref(false)
const aiQuestion = ref('')
const showLeaveConfirm = ref(false)
const hasUnsavedChanges = ref(false)
const leaveCallback = ref(null)
const draftNoteId = ref(null) // 追踪自动保存创建的草稿ID

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
const isEditMode = computed(() => !!route.params.id || !!draftNoteId.value)
const noteId = computed(() => route.params.id || draftNoteId.value)

const isValid = computed(() => {
  return form.title.trim().length > 0 && form.content.trim().length > 0
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

// 处理编辑器内容变化
const handleEditorChange = () => {
  const content = editorInstance.value.getMarkdown()
  form.content = content
  if (autoSaveEnabled.value) {
    autoSave()
  }
}

// 处理编辑器失去焦点
const handleEditorBlur = () => {
  // 可以在这里添加失去焦点时的逻辑
}

// 处理图片上传
const handleImageUpload = async (file, callback) => {
  try {
    // 验证图片文件
    const { validateImageFile } = await import('@/services/upload')
    validateImageFile(file)
    
    // 显示上传进度
    const uploadProgress = ref(0)
    notificationStore.info('正在上传图片...', '请稍候')
    
    // 上传图片
    const { uploadAPI } = await import('@/services/upload')
    const result = await uploadAPI.uploadImage(file, (progress) => {
      uploadProgress.value = progress
    })
    
    // 上传成功，调用编辑器回调
    if (result.success && result.data.url) {
      const imageUrl = result.data.url
      const altText = result.data.filename || file.name || 'uploaded image'
      callback(imageUrl, altText)
      notificationStore.success('图片上传成功')
    } else {
      throw new Error('上传失败：服务器返回异常')
    }
  } catch (error) {
    notificationStore.error('图片上传失败', error.message)
    // 调用回调但不传递URL，这样编辑器就不会插入图片
    callback('', '')
  }
}

// 自动保存 - 使用静默模式避免重复提示
const autoSave = debounce(async () => {
  if (autoSaveEnabled.value && isValid.value && hasUnsavedChanges.value) {
    await saveDraft(true) // 静默保存，不显示通知
  }
}, 3000)

// AI功能
const generateSummary = async () => {
  if (!form.content.trim()) return
  
  try {
    const summary = await aiStore.generateSummary(form.content)
    notificationStore.success('摘要已生成', summary)
  } catch (error) {
    notificationStore.error('生成摘要失败', error.message)
  }
}

const askAI = async () => {
  if (!aiQuestion.value.trim()) return
  
  try {
    const context = [form.content]
    await aiStore.askQuestion(aiQuestion.value, context)
    aiQuestion.value = ''
  } catch (error) {
    notificationStore.error('AI问答失败', error.message)
  }
}

// 初始化编辑器
const initEditor = () => {
  if (editorInstance.value) {
    editorInstance.value.destroy()
    editorInstance.value = null
  }
  
  // 确保DOM元素存在
  if (!editorRef.value) {
    console.error('Editor element not found')
    return
  }
  
  try {
    // 创建编辑器实例
    editorInstance.value = new Editor({
      el: editorRef.value,
      initialEditType: 'markdown',
      previewStyle: 'vertical',
      height: 'calc(100vh - 280px)',
      initialValue: form.content || '',
      usageStatistics: false,
      hideModeSwitch: false,
      plugins: plugins,
      events: {
        change: handleEditorChange,
        blur: handleEditorBlur,
        keydown: (type, ev) => {
          // 添加 Ctrl+S / Cmd+S 保存快捷键
          if ((ev.ctrlKey || ev.metaKey) && ev.key === 's') {
            ev.preventDefault()
            saveNote()
          }
        }
      },
      hooks: {
        addImageBlobHook: handleImageUpload,
        // 添加加载完成后的回调
        load: function() {
          // 确保内容是最新的
          if (form.content) {
            try {
              editorInstance.value.setMarkdown(form.content, false)
            } catch (error) {
              console.error('Error setting initial content:', error)
            }
          }
        }
      },
      toolbarItems: [
        ['heading', 'bold', 'italic', 'strike'],
        ['hr', 'quote'],
        ['ul', 'ol', 'task', 'indent', 'outdent'],
        ['table', 'image', 'link'],
        ['code', 'codeblock'],
        ['scrollSync']
      ]
    })
  } catch (error) {
    console.error('Failed to initialize editor:', error)
  }
}

// 方法
const saveNote = async () => {
  if (!isValid.value || saving.value) return

  saving.value = true
  try {
    const noteData = {
      title: form.title,
      content: form.content,
      tags: form.tags,
      isDraft: false
    }

    let savedNote
    if (route.params.id) {
      // 更新已存在的笔记
      savedNote = await notesStore.updateNote(route.params.id, noteData)
      notificationStore.success('笔记更新成功')
    } else if (draftNoteId.value) {
      // 将草稿转换为正式笔记
      const finalNoteData = { ...noteData, isDraft: false }
      savedNote = await notesStore.updateNote(draftNoteId.value, finalNoteData)
      notificationStore.success('笔记保存成功')
      router.push(`/notes/${draftNoteId.value}`)
    } else {
      // 创建新笔记
      savedNote = await notesStore.createNote(noteData)
      notificationStore.success('笔记创建成功')
      router.push(`/notes/${savedNote.id}`)
    }

    hasUnsavedChanges.value = false
    updateOriginalForm()
  } catch (error) {
    notificationStore.error('保存失败', error.message)
  } finally {
    saving.value = false
  }
}

const saveDraft = async (silent = false) => {
  if (!isValid.value) return

  try {
    const draftData = {
      title: form.title,
      content: form.content,
      tags: form.tags,
      isDraft: true
    }

    if (route.params.id) {
      // 编辑已存在的笔记
      await notesStore.updateNote(route.params.id, draftData, false)
    } else if (draftNoteId.value) {
      // 更新已创建的草稿
      await notesStore.updateNote(draftNoteId.value, draftData, false)
    } else {
      // 首次创建草稿
      const draftNote = await notesStore.createNote(draftData, false)
      draftNoteId.value = draftNote.id
    }

    lastSaved.value = new Date()
    hasUnsavedChanges.value = false
    updateOriginalForm()
    
    // 只在非静默模式下显示通知
    if (!silent) {
      notificationStore.success('草稿已保存')
    }
  } catch (error) {
    notificationStore.error('保存草稿失败', error.message)
  }
}

// AI功能方法
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
  editorInstance.value.setMarkdown(form.content)
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
    
    // 更新编辑器内容 - 取消注释并确保编辑器已初始化
    await nextTick()
    if (editorInstance.value && form.content) {
      editorInstance.value.setMarkdown(form.content)
    }
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
onMounted(async () => {
  // 如果是编辑模式，先加载数据
  if (isEditMode.value) {
    await loadNote()
  }
  
  // 然后初始化编辑器
  await nextTick()
  initEditor()
  
  // 监听浏览器关闭事件
  window.addEventListener('beforeunload', handleBeforeUnload)
})

onBeforeUnmount(() => {
  autoSave.cancel()
  window.removeEventListener('beforeunload', handleBeforeUnload)
  
  // 销毁编辑器实例
  if (editorInstance.value) {
    editorInstance.value.destroy()
  }
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
/* 编辑器容器样式 */
:deep(.toastui-editor) {
  min-height: 600px;
  border: 1px solid #e2e8f0;
  border-radius: 0.375rem;
  width: 100%;
}

:deep(.toastui-editor-defaultUI) {
  border: none;
}

:deep(.toastui-editor-ww-container) {
  height: 100%;
}

:deep(.toastui-editor-contents) {
  font-size: 16px;
  line-height: 1.7;
}

/* 代码块样式修复 */
:deep(.toastui-editor-contents) {
  /* 内联代码样式 */
  code {
    background-color: #f3f4f6 !important;
    color: #dc2626 !important;
    padding: 0.125rem 0.25rem !important;
    border-radius: 0.25rem !important;
    font-size: 0.875em !important;
    font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', 'Consolas', 'source-code-pro', monospace !important;
  }

  /* 代码块样式 */
  pre {
    background-color: #1e293b !important;
    color: #e2e8f0 !important;
    padding: 1rem !important;
    border-radius: 0.375rem !important;
    overflow-x: auto !important;
    margin: 1rem 0 !important;
    
    code {
      background-color: transparent !important;
      color: inherit !important;
      padding: 0 !important;
    }
  }

  /* 语法高亮代码块 */
  .hljs {
    background-color: #1e293b !important;
    color: #e2e8f0 !important;
  }

  /* ToastUI Editor 特有的代码块 */
  .language-javascript,
  .language-typescript,
  .language-html,
  .language-css,
  .language-json,
  .language-markdown,
  .language-bash,
  .language-shell,
  .language-python,
  .language-java,
  .language-cpp,
  .language-c {
    background-color: #1e293b !important;
    color: #e2e8f0 !important;
    
    pre {
      background-color: #1e293b !important;
      color: #e2e8f0 !important;
    }
  }
}

/* 编辑器预览模式的代码块样式 */
:deep(.toastui-editor-ww-container) {
  .toastui-editor-contents {
    code {
      background-color: #f3f4f6 !important;
      color: #dc2626 !important;
    }

    pre {
      background-color: #1e293b !important;
      color: #e2e8f0 !important;
      
      code {
        background-color: transparent !important;
        color: inherit !important;
      }
    }
  }
}

/* 编辑器markdown模式的代码块样式 */
:deep(.toastui-editor-md-container) {
  .toastui-editor-contents {
    code {
      background-color: #f3f4f6 !important;
      color: #dc2626 !important;
    }

    pre {
      background-color: #1e293b !important;
      color: #e2e8f0 !important;
      
      code {
        background-color: transparent !important;
        color: inherit !important;
      }
    }
  }
}

/* 响应式调整 */
@media (max-width: 768px) {
  :deep(.toastui-editor) {
    min-height: 400px;
  }
}
</style>
