<template>
  <Layout>
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 欢迎区域 -->
      <div class="mb-8 animate-fade-in">
        <h1 class="text-3xl font-bold text-gray-900">
          欢迎回来，{{ authStore.userName }}！
        </h1>
        <p class="text-gray-600 mt-2">
          这里是您的知识库概览，今天想要创建什么笔记呢？
        </p>
      </div>

      <!-- 统计卡片 -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <StatCard
          v-for="stat in stats"
          :key="stat.title"
          v-bind="stat"
          class="animate-slide-up"
          :style="{ animationDelay: `${stat.delay}ms` }"
        />
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- 左侧内容 -->
        <div class="lg:col-span-2 space-y-8">
          <!-- 快速操作 -->
          <div class="bg-white rounded-lg shadow animate-fade-in">
            <div class="p-6 border-b">
              <h2 class="text-lg font-semibold text-gray-900">快速操作</h2>
            </div>
            <div class="p-6 grid grid-cols-1 md:grid-cols-3 gap-4">
              <router-link
                to="/notes/create"
                class="flex flex-col items-center p-6 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-all group"
              >
                <div class="p-3 bg-gray-100 rounded-lg group-hover:bg-primary-100 transition-colors">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                </div>
                <span class="mt-3 text-sm font-medium text-gray-700">创建笔记</span>
              </router-link>

              <router-link
                to="/search"
                class="flex flex-col items-center p-6 border-2 border-dashed border-gray-300 rounded-lg hover:border-green-500 hover:bg-green-50 transition-all group"
              >
                <div class="p-3 bg-gray-100 rounded-lg group-hover:bg-green-100 transition-colors">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <span class="mt-3 text-sm font-medium text-gray-700">智能搜索</span>
              </router-link>

              <button
                @click="showAIChat = true"
                class="flex flex-col items-center p-6 border-2 border-dashed border-gray-300 rounded-lg hover:border-purple-500 hover:bg-purple-50 transition-all group"
              >
                <div class="p-3 bg-gray-100 rounded-lg group-hover:bg-purple-100 transition-colors">
                  <svg class="w-6 h-6 text-gray-600 group-hover:text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                  </svg>
                </div>
                <span class="mt-3 text-sm font-medium text-gray-700">AI问答</span>
              </button>
            </div>
          </div>

          <!-- 最近笔记 -->
          <div class="bg-white rounded-lg shadow animate-fade-in">
            <div class="p-6 border-b">
              <div class="flex items-center justify-between">
                <h2 class="text-lg font-semibold text-gray-900">最近笔记</h2>
                <router-link
                  to="/notes"
                  class="text-sm text-primary-600 hover:text-primary-800"
                >
                  查看全部 →
                </router-link>
              </div>
            </div>
            
            <div v-if="recentNotesLoading" class="p-12 text-center">
              <LoadingSpinner />
              <p class="mt-3 text-sm text-gray-500">加载中...</p>
            </div>
            
            <div v-else-if="recentNotes.length === 0" class="p-12 text-center">
              <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <h3 class="mt-2 text-sm font-medium text-gray-900">暂无笔记</h3>
              <p class="mt-1 text-sm text-gray-500">开始创建您的第一篇笔记吧</p>
            </div>
            
            <div v-else class="divide-y divide-gray-200">
              <div
                v-for="note in recentNotes"
                :key="note.id"
                class="p-6 hover:bg-gray-50 cursor-pointer transition-colors"
                @click="viewNote(note.id)"
              >
                <div class="flex items-start justify-between">
                  <div class="flex-1 min-w-0">
                    <h3 class="text-sm font-medium text-gray-900 truncate hover:text-primary-600">
                      {{ note.title }}
                    </h3>
                    <p class="text-sm text-gray-500 mt-1 line-clamp-2">
                      {{ getPlainText(note.content) }}
                    </p>
                    <div class="flex items-center mt-2 space-x-2">
                      <span
                        v-for="tag in note.tags.slice(0, 2)"
                        :key="tag.id"
                        :style="{ backgroundColor: tag.color + '20', color: tag.color }"
                        class="px-2 py-0.5 rounded-full text-xs font-medium"
                      >
                        {{ tag.name }}
                      </span>
                      <span v-if="note.tags.length > 2" class="text-xs text-gray-500">
                        +{{ note.tags.length - 2 }}
                      </span>
                    </div>
                  </div>
                  <div class="text-xs text-gray-500 ml-4">
                    {{ formatRelativeTime(note.updatedAt) }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 右侧边栏 -->
        <div class="space-y-8">
          <!-- 热门标签 -->
          <div class="bg-white rounded-lg shadow animate-fade-in">
            <div class="p-6 border-b">
              <h2 class="text-lg font-semibold text-gray-900">热门标签</h2>
            </div>
            <div class="p-6">
              <div v-if="popularTags.length === 0" class="text-center text-sm text-gray-500">
                暂无标签
              </div>
              <div v-else class="flex flex-wrap gap-2">
                <button
                  v-for="tag in popularTags"
                  :key="tag.id"
                  @click="searchByTag(tag.name)"
                  :style="{ 
                    backgroundColor: tag.color + '20', 
                    color: tag.color,
                    fontSize: `${0.75 + (tag.count / maxTagCount) * 0.5}rem`
                  }"
                  class="px-3 py-1 rounded-full font-medium hover:opacity-80 transition-opacity"
                >
                  {{ tag.name }} ({{ tag.count }})
                </button>
              </div>
            </div>
          </div>

          <!-- 搜索历史 -->
          <div class="bg-white rounded-lg shadow animate-fade-in">
            <div class="p-6 border-b">
              <h2 class="text-lg font-semibold text-gray-900">最近搜索</h2>
            </div>
            <div class="p-6">
              <div v-if="searchStore.recentSearches.length === 0" class="text-center text-sm text-gray-500">
                暂无搜索历史
              </div>
              <div v-else class="space-y-2">
                <button
                  v-for="(query, index) in searchStore.recentSearches"
                  :key="index"
                  @click="performSearch(query)"
                  class="w-full text-left p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors group"
                >
                  <div class="flex items-center justify-between">
                    <span class="text-sm text-gray-700 truncate">{{ query }}</span>
                    <svg class="w-4 h-4 text-gray-400 group-hover:text-gray-600 opacity-0 group-hover:opacity-100 transition-opacity" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                  </div>
                </button>
              </div>
            </div>
          </div>

          <!-- AI对话历史 -->
          <div class="bg-white rounded-lg shadow animate-fade-in">
            <div class="p-6 border-b">
              <h2 class="text-lg font-semibold text-gray-900">AI对话</h2>
            </div>
            <div class="p-6">
              <div v-if="!aiStore.hasConversations" class="text-center text-sm text-gray-500">
                暂无AI对话历史
              </div>
              <div v-else class="space-y-3">
                <div
                  v-for="conversation in aiStore.recentConversations.slice(0, 3)"
                  :key="conversation.id"
                  class="p-3 bg-gradient-to-r from-purple-50 to-primary-50 rounded-lg cursor-pointer hover:from-purple-100 hover:to-primary-100 transition-all"
                  @click="viewConversation(conversation)"
                >
                  <p class="text-sm font-medium text-gray-900 truncate">
                    {{ conversation.question }}
                  </p>
                  <p class="text-xs text-gray-500 mt-1">
                    {{ formatRelativeTime(conversation.timestamp) }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- AI对话模态框 -->
    <teleport to="body">
      <div v-if="showAIChat" class="modal-backdrop" @click.self="showAIChat = false">
        <div class="modal max-w-2xl animate-slide-up">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-bold">AI助手</h3>
            <button @click="showAIChat = false" class="text-gray-500 hover:text-gray-700">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          
          <div class="space-y-4">
            <div v-if="aiStore.lastResponse" class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-700 whitespace-pre-wrap">{{ aiStore.lastResponse.answer }}</p>
            </div>
            
            <div class="flex space-x-2">
              <input
                v-model="aiQuestion"
                @keyup.enter="askAI"
                placeholder="询问关于您笔记的任何问题..."
                class="flex-1 input"
                :disabled="aiStore.isThinking"
              />
              <button
                @click="askAI"
                :disabled="!aiQuestion.trim() || aiStore.isThinking"
                class="btn btn-primary"
              >
                <LoadingSpinner v-if="aiStore.isThinking" size="small" color="white" />
                <span v-else>发送</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </teleport>
  </Layout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotesStore } from '@/stores/notes'
import { useSearchStore } from '@/stores/search'
import { useAIStore } from '@/stores/ai'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import StatCard from '@/components/Dashboard/StatCard.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import { formatRelativeTime } from '@/utils/date'
import { getPlainText } from '@/utils/text'

const router = useRouter()
const authStore = useAuthStore()
const notesStore = useNotesStore()
const searchStore = useSearchStore()
const aiStore = useAIStore()
const notificationStore = useNotificationStore()

const recentNotesLoading = ref(false)
const showAIChat = ref(false)
const aiQuestion = ref('')

// 缓存日期计算结果
const todayString = computed(() => new Date().toDateString())
const weekAgo = computed(() => new Date(Date.now() - 7 * 24 * 60 * 60 * 1000))

const stats = computed(() => {
  const notes = notesStore.notes
  const todayStr = todayString.value
  const weekAgoDate = weekAgo.value

  const todayNotes = notes.filter(note => 
    new Date(note.createdAt).toDateString() === todayStr
  ).length

  const weeklyActive = notes.filter(note => 
    new Date(note.updatedAt) >= weekAgoDate
  ).length

  return [
    {
      title: '总笔记数',
      value: notesStore.notes.length,
      icon: 'document-text',
      color: 'blue',
      trend: 12,
      delay: 0
    },
    {
      title: '标签数量',
      value: notesStore.tags.length,
      icon: 'tag',
      color: 'green',
      delay: 100
    },
    {
      title: '今日新增',
      value: todayNotes,
      icon: 'plus-circle',
      color: 'purple',
      delay: 200
    },
    {
      title: '本周活跃',
      value: weeklyActive,
      icon: 'chart-bar',
      color: 'orange',
      trend: weeklyActive > 0 ? 8 : -5,
      delay: 300
    }
  ]
})

const recentNotes = computed(() => notesStore.recentNotes)

// 优化标签计数算法，使用Map提高性能
const popularTags = computed(() => {
  const tagCounts = new Map()
  const tagsMap = new Map(notesStore.tags.map(tag => [tag.name, tag]))
  
  notesStore.notes.forEach(note => {
    note.tags?.forEach(tag => {
      if (tag.name) {
        tagCounts.set(tag.name, (tagCounts.get(tag.name) || 0) + 1)
      }
    })
  })
  
  return Array.from(tagCounts.entries())
    .map(([name, count]) => {
      const tagInfo = tagsMap.get(name)
      return {
        name,
        count,
        color: tagInfo?.color || '#6B7280',
        id: tagInfo?.id
      }
    })
    .sort((a, b) => b.count - a.count)
    .slice(0, 10)
})

const maxTagCount = computed(() => 
  Math.max(...popularTags.value.map(tag => tag.count), 1)
)

const viewNote = (id) => {
  router.push(`/notes/${id}`)
}

const performSearch = (query) => {
  router.push(`/search?q=${encodeURIComponent(query)}`)
}

const searchByTag = (tagName) => {
  router.push(`/notes?tag=${encodeURIComponent(tagName)}`)
}

const viewConversation = (conversation) => {
  aiQuestion.value = conversation.question
  showAIChat.value = true
}

const askAI = async () => {
  if (!aiQuestion.value.trim()) return
  
  try {
    await aiStore.askQuestion(aiQuestion.value)
    aiQuestion.value = ''
  } catch (error) {
    notificationStore.error('AI问答失败', error.message)
  }
}

onMounted(async () => {
  recentNotesLoading.value = true
  try {
    await Promise.all([
      notesStore.fetchNotes(1, 5),
      notesStore.fetchTags()
    ])
    
    // 加载搜索历史和AI对话历史
    searchStore.loadHistoryFromLocal()
    aiStore.loadConversationsFromLocal()
  } catch (error) {
    notificationStore.error('加载数据失败', error.message)
  } finally {
    recentNotesLoading.value = false
  }
})
</script>

<style scoped>
@keyframes slide-up {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.animate-slide-up {
  animation: slide-up 0.3s ease-out forwards;
  opacity: 0;
}
</style>