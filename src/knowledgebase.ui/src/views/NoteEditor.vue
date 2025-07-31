<template>
  <Layout>
    <!-- 精简布局 -->
    <div class="min-h-screen bg-gray-50">
      <!-- 顶部工具栏 -->
      <div class="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex items-center justify-between h-16">
            <!-- 左侧 -->
            <div class="flex items-center space-x-4">
              <button
                @click="$router.back()"
                class="p-2 text-gray-400 hover:text-gray-600 transition-colors"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div class="flex items-center space-x-2">
                <h1 class="text-lg font-semibold text-gray-900">
                  {{ isEditMode ? '编辑笔记' : '新笔记' }}
                </h1>
                <span v-if="hasUnsavedChanges" class="w-2 h-2 bg-orange-400 rounded-full" title="有未保存的更改"></span>
              </div>
            </div>
            
            <!-- 右侧 -->
            <div class="flex items-center space-x-3">
              <!-- 状态显示 -->
              <div class="flex items-center space-x-2 text-sm text-gray-500">
                <span v-if="autoSaveEnabled && lastSaved" class="flex items-center">
                  <svg class="w-4 h-4 mr-1 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                  </svg>
                  已保存
                </span>
                <span class="text-gray-300">|</span>
                <span>{{ wordCount }} 字</span>
              </div>
              
              <!-- 保存按钮 -->
              <button
                @click="saveNote"
                :disabled="saving || !isValid"
                class="inline-flex items-center px-4 py-2 text-white bg-primary-600 rounded-lg hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all font-medium"
              >
                <LoadingSpinner v-if="saving" size="small" color="white" class="mr-2" />
                <svg v-else class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                </svg>
                {{ saving ? '保存中...' : '保存' }}
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <!-- 编辑区域 -->
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <form @submit.prevent="saveNote" class="space-y-4">

          <!-- 标题输入 -->
          <div class="bg-white rounded-lg shadow-sm border p-6">
            <input
              v-model="form.title"
              type="text"
              placeholder="笔记标题..."
              class="w-full text-2xl font-bold border-0 focus:ring-0 p-0 placeholder-gray-400 bg-transparent"
              required
              autofocus
            />
            
            <!-- 标签输入 -->
            <div class="mt-4 pt-4 border-t border-gray-100">
              <TagInput
                v-model="form.tags"
                placeholder="添加标签..."
                :suggestions="notesStore.tags"
                class="border-0 focus:ring-0 p-0 bg-transparent"
              />
            </div>
          </div>

          <!-- 编辑器 -->
          <div class="bg-white rounded-lg shadow-sm border overflow-hidden">
            <div ref="editorRef" class="toast-ui-editor min-h-96"></div>
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

// 自动保存
const autoSave = debounce(async () => {
  if (autoSaveEnabled.value && isValid.value && hasUnsavedChanges.value) {
    await saveDraft()
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
      height: 'calc(100vh - 300px)',
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
  min-height: 500px;
  border: 1px solid #e2e8f0;
  border-radius: 0.375rem;
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
