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

        <!-- Toast UI Editor -->
        <div class="bg-white rounded-lg shadow-sm border overflow-hidden">
          <div ref="editorRef" class="toast-ui-editor"></div>
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

  </Layout>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted, onBeforeUnmount, nextTick, getCurrentInstance } from 'vue'
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
    // 这里添加图片上传逻辑
    // 上传成功后调用 callback(imageUrl, 'alt text')
    // 例如: 
    // const imageUrl = await uploadImage(file)
    // callback(imageUrl, 'image alt text')
  } catch (error) {
    notificationStore.error('图片上传失败', error.message)
  }
}

// 自动保存
const autoSave = debounce(async () => {
  if (autoSaveEnabled.value && isValid.value && hasUnsavedChanges.value) {
    await saveDraft()
  }
}, 3000)

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

/* 响应式调整 */
@media (max-width: 768px) {
  :deep(.toastui-editor) {
    min-height: 400px;
  }
}
</style>
