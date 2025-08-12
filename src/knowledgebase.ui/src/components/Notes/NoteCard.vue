<template>
  <div 
    class="relative bg-white/90 backdrop-blur-sm rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 cursor-pointer group border border-white/60 hover:border-primary-200/50 transform hover:-translate-y-1 overflow-hidden"
    @click="$emit('view', note.id)"
  >
    <!-- 装饰性渐变背景 -->
    <div class="absolute inset-0 bg-gradient-to-r from-primary-50/30 via-transparent to-purple-50/20 opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>
    
    <div class="relative z-10 p-6 pl-16">
      <div class="flex items-start justify-between">
        <!-- 左侧内容区 -->
        <div class="flex-1 min-w-0 pr-6">
          <!-- 标题行 -->
          <div class="flex items-start justify-between mb-3">
            <h3 class="text-2xl font-bold text-gray-900 group-hover:text-primary-600 transition-colors leading-tight">
              {{ note.title }}
            </h3>
          </div>
          
          <!-- 摘要或内容预览 -->
          <div class="mb-4">
            <p class="text-gray-700 text-base leading-relaxed line-clamp-2">
              {{ displayContent }}
            </p>
          </div>
          
          <!-- 标签和元信息行 -->
          <div class="flex items-center justify-between">
            <!-- 左侧：标签 -->
            <div class="flex items-center space-x-4">
              <div v-if="note.tags && note.tags.length > 0" class="flex flex-wrap gap-2">
                <span
                  v-for="tag in displayTags"
                  :key="tag.id"
                  :style="{ backgroundColor: tag.color + '15', color: tag.color, borderColor: tag.color + '30' }"
                  class="px-3 py-1.5 rounded-full text-xs font-semibold border transition-all duration-200 hover:scale-105 cursor-pointer"
                  @click.stop
                >
                  {{ tag.name }}
                </span>
                <span
                  v-if="note.tags.length > 3"
                  class="px-3 py-1.5 rounded-full text-xs font-semibold bg-gradient-to-r from-gray-100 to-gray-200 text-gray-700 border border-gray-200"
                >
                  +{{ note.tags.length - 3 }}
                </span>
              </div>
              
              <!-- AI摘要标识 -->
              <div v-if="note.summary" class="flex items-center text-xs text-primary-600 bg-primary-50 rounded-full px-3 py-1.5">
                <svg class="w-3 h-3 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                </svg>
                AI摘要
              </div>
            </div>
            
            <!-- 右侧：时间和字数 -->
            <div class="flex items-center space-x-4 text-sm text-gray-500">
              <div class="flex items-center">
                <svg class="w-4 h-4 mr-1.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {{ relativeTime }}
              </div>
              <div class="flex items-center">
                <svg class="w-4 h-4 mr-1.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                {{ wordCount }} 字
              </div>
            </div>
          </div>
        </div>
        
        <!-- 右侧：操作按钮 -->
        <div class="flex items-center space-x-2 opacity-0 group-hover:opacity-100 transition-all duration-300 transform translate-x-2 group-hover:translate-x-0 flex-shrink-0">
          <button
            @click.stop="$emit('edit', note.id)"
            class="p-3 text-gray-400 hover:text-primary-500 rounded-2xl hover:bg-primary-50 transition-all duration-200 hover:scale-110"
            title="编辑笔记"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
          </button>
          <button
            @click.stop="handleDelete"
            class="p-3 text-gray-400 hover:text-red-500 rounded-2xl hover:bg-red-50 transition-all duration-200 hover:scale-110"
            title="删除笔记"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>

  <!-- 删除确认模态框 -->
  <ConfirmModal
    v-model:show="showDeleteModal"
    title="删除笔记"
    :message="`确定要删除笔记「${note.title}」吗？此操作无法撤销。`"
    confirm-text="删除"
    cancel-text="取消"
    @confirm="confirmDelete"
    @cancel="cancelDelete"
  />
</template>

<script setup>
import { computed, ref } from 'vue'
import ConfirmModal from '@/components/Common/ConfirmModal.vue'
import { truncateText, getWordCount } from '@/utils/text'
import { formatRelativeTime } from '@/utils/date'

const props = defineProps({
  note: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['view', 'edit', 'delete'])

const contentPreview = computed(() => {
  return truncateText(props.note.content, 150)
})

// 优先显示摘要，如果没有摘要则显示内容预览
const displayContent = computed(() => {
  if (props.note.summary) {
    return props.note.summary
  }
  return truncateText(props.note.content, 200)
})

const displayTags = computed(() => {
  return props.note.tags?.slice(0, 3) || []
})

const wordCount = computed(() => {
  return getWordCount(props.note.content)
})

const relativeTime = computed(() => {
  return formatRelativeTime(props.note.updatedAt)
})

// 删除确认相关
const showDeleteModal = ref(false)

// 处理删除操作
const handleDelete = () => {
  showDeleteModal.value = true
}

const confirmDelete = () => {
  emit('delete', props.note.id)
  showDeleteModal.value = false
}

const cancelDelete = () => {
  showDeleteModal.value = false
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* 高级卡片动画效果 */
.group:hover {
  transform: translateY(-4px) scale(1.02);
}

/* 添加动画类 */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes shimmer {
  0% {
    transform: translateX(-100%);
  }
  100% {
    transform: translateX(100%);
  }
}

.fade-in-up {
  animation: fadeInUp 0.6s ease-out forwards;
}

/* 添加光泽效果 */
.group::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.4), transparent);
  transition: left 0.5s;
  pointer-events: none;
}

.group:hover::before {
  left: 100%;
}
</style>