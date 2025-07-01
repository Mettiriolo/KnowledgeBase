<template>
  <div class="tag-input">
    <div v-if="tags.length > 0" class="flex flex-wrap gap-2 mb-2">
      <span
        v-for="tag in tags"
        :key="tag"
        :style="{ backgroundColor: getTagColor(tag) + '20', color: getTagColor(tag) }"
        class="inline-flex items-center px-2.5 py-1 rounded-full text-sm font-medium animate-fade-in"
      >
        {{ tag }}
        <button
          @click="removeTag(tag)"
          class="ml-1.5 text-current hover:bg-black hover:bg-opacity-10 rounded-full p-0.5 transition-colors"
        >
          <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </span>
    </div>

    <div class="relative">
      <input
        ref="inputRef"
        v-model="inputValue"
        @input="handleInput"
        @keydown="handleKeydown"
        @focus="handleFocus"
        @blur="handleBlur"
        :placeholder="placeholder"
        class="input text-sm"
      />

      <!-- 标签建议下拉菜单 -->
      <transition name="slide">
        <div
          v-if="showSuggestions && filteredSuggestions.length > 0"
          class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-48 overflow-y-auto"
        >
          <button
            v-for="(suggestion, index) in filteredSuggestions"
            :key="suggestion.id || suggestion"
            @mousedown="selectSuggestion(suggestion)"
            :class="[
              'w-full text-left px-3 py-2 text-sm hover:bg-gray-100 transition-colors',
              { 'bg-primary-50': index === selectedSuggestionIndex }
            ]"
          >
            <span
              :style="{ backgroundColor: getTagColor(suggestion.name || suggestion) + '20', color: getTagColor(suggestion.name || suggestion) }"
              class="inline-block px-2 py-0.5 rounded-full text-xs mr-2"
            >
              {{ suggestion.name || suggestion }}
            </span>
            <span v-if="suggestion.count" class="text-xs text-gray-500">
              ({{ suggestion.count }})
            </span>
          </button>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useNotesStore } from '@/stores/notes'

const props = defineProps({
  modelValue: {
    type: Array,
    default: () => []
  },
  placeholder: {
    type: String,
    default: '添加标签...'
  },
  suggestions: {
    type: Array,
    default: () => []
  },
  maxTags: {
    type: Number,
    default: 10
  }
})

const emit = defineEmits(['update:modelValue'])

const notesStore = useNotesStore()
const inputRef = ref(null)
const inputValue = ref('')
const showSuggestions = ref(false)
const selectedSuggestionIndex = ref(-1)

const tags = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const allSuggestions = computed(() => {
  const storeTags = notesStore.tags || []
  const customSuggestions = props.suggestions || []
  
  // 合并并去重
  const merged = [...storeTags, ...customSuggestions.map(s => 
    typeof s === 'string' ? { name: s } : s
  )]
  
  const unique = merged.reduce((acc, curr) => {
    if (!acc.find(item => item.name === curr.name)) {
      acc.push(curr)
    }
    return acc
  }, [])
  
  return unique
})

const filteredSuggestions = computed(() => {
  const query = inputValue.value.toLowerCase().trim()
  
  if (!query) {
    return allSuggestions.value
      .filter(s => !tags.value.includes(s.name))
      .slice(0, 8)
  }
  
  return allSuggestions.value
    .filter(s => 
      s.name.toLowerCase().includes(query) && 
      !tags.value.includes(s.name)
    )
    .slice(0, 8)
})

const getTagColor = (tagName) => {
  const existingTag = notesStore.tags?.find(tag => tag.name === tagName)
  if (existingTag?.color) return existingTag.color
  
  // 生成一致的颜色
  const colors = [
    '#3B82F6', '#10B981', '#F59E0B', '#EF4444', 
    '#8B5CF6', '#06B6D4', '#84CC16', '#F97316',
    '#EC4899', '#14B8A6', '#6366F1', '#A855F7'
  ]
  
  const hash = tagName.split('').reduce((a, b) => {
    a = ((a << 5) - a) + b.charCodeAt(0)
    return a & a
  }, 0)
  
  return colors[Math.abs(hash) % colors.length]
}

const addTag = (tagName) => {
  const trimmedTag = tagName.trim()
  if (trimmedTag && !tags.value.includes(trimmedTag) && tags.value.length < props.maxTags) {
    tags.value = [...tags.value, trimmedTag]
    inputValue.value = ''
    selectedSuggestionIndex.value = -1
    return true
  }
  return false
}

const removeTag = (tag) => {
  tags.value = tags.value.filter(t => t !== tag)
}

const selectSuggestion = (suggestion) => {
  const tagName = suggestion.name || suggestion
  if (addTag(tagName)) {
    showSuggestions.value = false
  }
  inputRef.value?.focus()
}

const handleInput = () => {
  showSuggestions.value = true
  selectedSuggestionIndex.value = -1
}

const handleFocus = () => {
  showSuggestions.value = true
}

const handleBlur = () => {
  // 延迟关闭以允许点击建议
  setTimeout(() => {
    showSuggestions.value = false
    selectedSuggestionIndex.value = -1
    // 自动添加输入的标签
    if (inputValue.value.trim()) {
      addTag(inputValue.value)
    }
  }, 200)
}

const handleKeydown = (event) => {
  switch (event.key) {
    case 'Enter':
      event.preventDefault()
      if (selectedSuggestionIndex.value >= 0 && selectedSuggestionIndex.value < filteredSuggestions.value.length) {
        selectSuggestion(filteredSuggestions.value[selectedSuggestionIndex.value])
      } else if (inputValue.value.trim()) {
        addTag(inputValue.value)
      }
      break
    
    case 'ArrowDown':
      event.preventDefault()
      if (showSuggestions.value && filteredSuggestions.value.length > 0) {
        selectedSuggestionIndex.value = Math.min(
          selectedSuggestionIndex.value + 1,
          filteredSuggestions.value.length - 1
        )
      }
      break
    
    case 'ArrowUp':
      event.preventDefault()
      if (showSuggestions.value && filteredSuggestions.value.length > 0) {
        selectedSuggestionIndex.value = Math.max(selectedSuggestionIndex.value - 1, -1)
      }
      break
    
    case 'Escape':
      showSuggestions.value = false
      selectedSuggestionIndex.value = -1
      inputValue.value = ''
      break
    
    case 'Backspace':
      if (!inputValue.value && tags.value.length > 0) {
        removeTag(tags.value[tags.value.length - 1])
      }
      break
    
    case ',':
    case ';':
    case 'Tab':
      if (inputValue.value.trim()) {
        event.preventDefault()
        addTag(inputValue.value)
      }
      break
  }
}

// 监听输入值变化，重置选择索引
watch(inputValue, () => {
  selectedSuggestionIndex.value = -1
})
</script>