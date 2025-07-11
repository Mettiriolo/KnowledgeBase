<template>
  <div 
    class="bg-white rounded-lg shadow hover:shadow-xl transition-all duration-200 cursor-pointer group p-6"
    @click="$emit('view', note.id)"
  >
    <!-- 笔记标题 -->
    <div class="flex items-start justify-between mb-3">
      <h3 class="text-lg font-semibold text-gray-900 group-hover:text-primary-600 transition-colors line-clamp-2">
        {{ note.title }}
      </h3>
      <div class="flex items-center space-x-1 opacity-0 group-hover:opacity-100 transition-opacity">
        <button
          @click.stop="$emit('edit', note.id)"
          class="p-1.5 text-gray-400 hover:text-primary-500 rounded-lg hover:bg-primary-50 transition-colors"
          title="编辑"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
        </button>
        <button
          @click.stop="handleDelete"
          class="p-1.5 text-gray-400 hover:text-red-500 rounded-lg hover:bg-red-50 transition-colors"
          title="删除"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </button>
      </div>
    </div>

    <!-- 笔记预览 -->
    <p class="text-gray-600 text-sm mb-4 line-clamp-3">
      {{ contentPreview }}
    </p>

    <!-- 标签 -->
    <div v-if="note.tags && note.tags.length > 0" class="flex flex-wrap gap-2 mb-4">
      <span
        v-for="tag in displayTags"
        :key="tag.id"
        :style="{ backgroundColor: tag.color + '20', color: tag.color }"
        class="px-2 py-1 rounded-full text-xs font-medium"
        @click.stop
      >
        {{ tag.name }}
      </span>
      <span
        v-if="note.tags.length > 3"
        class="px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-600"
      >
        +{{ note.tags.length - 3 }}
      </span>
    </div>

    <!-- 底部信息 -->
    <div class="flex items-center justify-between text-xs text-gray-500">
      <div class="flex items-center space-x-3">
        <span>{{ relativeTime }}</span>
        <span v-if="note.summary" class="flex items-center text-primary-600">
          <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
          </svg>
          AI摘要
        </span>
      </div>
      <div class="text-right">
        <span>{{ wordCount }} 字</span>
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

/* 卡片悬停效果 */
.group:hover {
  transform: translateY(-2px);
}
</style>