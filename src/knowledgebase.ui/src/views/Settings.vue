<template>
  <Layout>
    <div class="max-w-4xl mx-auto py-6">
      <!-- 页面标题 -->
      <div class="mb-8">
        <h1 class="text-2xl font-bold text-gray-900">个人设置</h1>
        <p class="mt-1 text-sm text-gray-600">管理您的账户设置和偏好</p>
      </div>

      <!-- 设置选项卡 -->
      <div class="border-b border-gray-200 mb-6">
        <nav class="-mb-px flex space-x-8">
          <button
            v-for="tab in tabs"
            :key="tab.key"
            @click="activeTab = tab.key"
            :class="[
              activeTab === tab.key
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
              'whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm'
            ]"
          >
            {{ tab.label }}
          </button>
        </nav>
      </div>

      <!-- 个人信息 -->
      <div v-if="activeTab === 'profile'" class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">个人信息</h3>
          
          <form @submit.prevent="updateProfile">
            <div class="grid grid-cols-1 gap-6">
              <!-- 头像 -->
              <div class="flex items-center space-x-6">
                <div class="shrink-0">
                  <div class="h-16 w-16 rounded-full overflow-hidden bg-gray-200 flex items-center justify-center">
                    <img 
                      v-if="profileForm.avatar"
                      :src="profileForm.avatar" 
                      :alt="profileForm.name"
                      class="h-full w-full object-cover"
                    />
                    <span 
                      v-else
                      class="text-2xl font-medium text-gray-500"
                    >
                      {{ profileForm.name ? profileForm.name[0].toUpperCase() : '?' }}
                    </span>
                  </div>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">头像</label>
                  <div class="mt-1">
                    <input
                      type="file"
                      accept="image/*"
                      @change="handleAvatarChange"
                      class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
                    />
                  </div>
                </div>
              </div>

              <!-- 姓名 -->
              <div>
                <label class="block text-sm font-medium text-gray-700">姓名</label>
                <input
                  v-model="profileForm.name"
                  type="text"
                  class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  :class="{ 'border-red-300': profileErrors.name }"
                />
                <p v-if="profileErrors.name" class="mt-1 text-sm text-red-600">{{ profileErrors.name }}</p>
              </div>

              <!-- 邮箱 -->
              <div>
                <label class="block text-sm font-medium text-gray-700">邮箱</label>
                <input
                  v-model="profileForm.email"
                  type="email"
                  class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  :class="{ 'border-red-300': profileErrors.email }"
                />
                <p v-if="profileErrors.email" class="mt-1 text-sm text-red-600">{{ profileErrors.email }}</p>
              </div>

              <!-- 个人简介 -->
              <div>
                <label class="block text-sm font-medium text-gray-700">个人简介</label>
                <textarea
                  v-model="profileForm.bio"
                  rows="3"
                  class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  placeholder="简单介绍一下自己..."
                ></textarea>
              </div>
            </div>

            <div class="mt-6 flex justify-end">
              <button
                type="submit"
                :disabled="profileSaving"
                class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
              >
                <LoadingSpinner v-if="profileSaving" class="w-4 h-4 mr-2" />
                {{ profileSaving ? '保存中...' : '保存更改' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- 密码修改 -->
      <div v-if="activeTab === 'password'" class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">修改密码</h3>
          
          <form @submit.prevent="updatePassword">
            <div class="grid grid-cols-1 gap-6">
              <!-- 当前密码 -->
              <div>
                <label class="block text-sm font-medium text-gray-700">当前密码</label>
                <input
                  v-model="passwordForm.currentPassword"
                  type="password"
                  class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  :class="{ 'border-red-300': passwordErrors.currentPassword }"
                />
                <p v-if="passwordErrors.currentPassword" class="mt-1 text-sm text-red-600">{{ passwordErrors.currentPassword }}</p>
              </div>

              <!-- 新密码 -->
              <div>
                <label class="block text-sm font-medium text-gray-700">新密码</label>
                <input
                  v-model="passwordForm.newPassword"
                  type="password"
                  class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  :class="{ 'border-red-300': passwordErrors.newPassword }"
                />
                <p v-if="passwordErrors.newPassword" class="mt-1 text-sm text-red-600">{{ passwordErrors.newPassword }}</p>
              </div>

              <!-- 确认密码 -->
              <div>
                <label class="block text-sm font-medium text-gray-700">确认新密码</label>
                <input
                  v-model="passwordForm.confirmPassword"
                  type="password"
                  class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  :class="{ 'border-red-300': passwordErrors.confirmPassword }"
                />
                <p v-if="passwordErrors.confirmPassword" class="mt-1 text-sm text-red-600">{{ passwordErrors.confirmPassword }}</p>
              </div>
            </div>

            <div class="mt-6 flex justify-end">
              <button
                type="submit"
                :disabled="passwordSaving"
                class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
              >
                <LoadingSpinner v-if="passwordSaving" class="w-4 h-4 mr-2" />
                {{ passwordSaving ? '保存中...' : '修改密码' }}
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- 偏好设置 -->
      <div v-if="activeTab === 'preferences'" class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">偏好设置</h3>
          
          <div class="space-y-6">
            <!-- 主题设置 -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">主题</label>
              <select
                v-model="preferencesForm.theme"
                class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
              >
                <option value="light">浅色主题</option>
                <option value="dark">深色主题</option>
                <option value="auto">跟随系统</option>
              </select>
            </div>

            <!-- 语言设置 -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">语言</label>
              <select
                v-model="preferencesForm.language"
                class="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
              >
                <option value="zh-CN">简体中文</option>
                <option value="en">English</option>
              </select>
            </div>

            <!-- 自动保存 -->
            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">自动保存</label>
                <p class="text-sm text-gray-500">编辑笔记时自动保存草稿</p>
              </div>
              <button
                @click="preferencesForm.autoSave = !preferencesForm.autoSave"
                :class="[
                  preferencesForm.autoSave ? 'bg-primary-600' : 'bg-gray-200',
                  'relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
                ]"
              >
                <span
                  :class="[
                    preferencesForm.autoSave ? 'translate-x-5' : 'translate-x-0',
                    'pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out'
                  ]"
                />
              </button>
            </div>

            <!-- 通知设置 -->
            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">桌面通知</label>
                <p class="text-sm text-gray-500">接收重要通知的桌面提醒</p>
              </div>
              <button
                @click="preferencesForm.notifications = !preferencesForm.notifications"
                :class="[
                  preferencesForm.notifications ? 'bg-primary-600' : 'bg-gray-200',
                  'relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
                ]"
              >
                <span
                  :class="[
                    preferencesForm.notifications ? 'translate-x-5' : 'translate-x-0',
                    'pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out'
                  ]"
                />
              </button>
            </div>
          </div>

          <div class="mt-6 flex justify-end">
            <button
              @click="updatePreferences"
              :disabled="preferencesSaving"
              class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
            >
              <LoadingSpinner v-if="preferencesSaving" class="w-4 h-4 mr-2" />
              {{ preferencesSaving ? '保存中...' : '保存设置' }}
            </button>
          </div>
        </div>
      </div>

      <!-- 账户安全 -->
      <div v-if="activeTab === 'security'" class="bg-white shadow rounded-lg">
        <div class="px-4 py-5 sm:p-6">
          <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">账户安全</h3>
          
          <div class="space-y-6">
            <!-- 两步验证 -->
            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">两步验证</label>
                <p class="text-sm text-gray-500">为您的账户添加额外的安全保护</p>
              </div>
              <button
                @click="toggleTwoFactor"
                :class="[
                  preferencesForm.twoFactorEnabled ? 'bg-primary-600' : 'bg-gray-200',
                  'relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
                ]"
              >
                <span
                  :class="[
                    preferencesForm.twoFactorEnabled ? 'translate-x-5' : 'translate-x-0',
                    'pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out'
                  ]"
                />
              </button>
            </div>

            <!-- 登录历史 -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">最近登录</label>
              <div class="bg-gray-50 rounded-md p-4">
                <div v-if="loginHistory.length > 0" class="space-y-2">
                  <div
                    v-for="(login, index) in loginHistory"
                    :key="index"
                    class="flex items-center justify-between text-sm"
                  >
                    <div>
                      <span class="font-medium">{{ login.device }}</span>
                      <span class="text-gray-500 ml-2">{{ login.location }}</span>
                    </div>
                    <span class="text-gray-500">{{ formatDate(login.timestamp) }}</span>
                  </div>
                </div>
                <div v-else class="text-sm text-gray-500">
                  暂无登录历史
                </div>
              </div>
            </div>

            <!-- 危险操作 -->
            <div class="border-t pt-6">
              <h4 class="text-sm font-medium text-red-600 mb-4">危险操作</h4>
              <div class="space-y-4">
                <div class="flex items-center justify-between">
                  <div>
                    <label class="text-sm font-medium text-gray-700">删除账户</label>
                    <p class="text-sm text-gray-500">永久删除您的账户和所有数据</p>
                  </div>
                  <button
                    @click="showDeleteAccountModal = true"
                    class="inline-flex items-center px-3 py-2 border border-red-300 text-sm font-medium rounded-md text-red-700 bg-white hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                  >
                    删除账户
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 删除账户确认模态框 -->
    <ConfirmModal
      v-model:show="showDeleteAccountModal"
      title="删除账户"
      message="此操作将永久删除您的账户和所有数据，包括笔记、设置等。此操作无法撤销，请谨慎操作。"
      confirm-text="确认删除"
      cancel-text="取消"
      confirm-button-class="bg-red-600 hover:bg-red-700 focus:ring-red-500"
      @confirm="deleteAccount"
    />
  </Layout>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useSettingsStore } from '@/stores/settings'
import { useAuthStore } from '@/stores/auth'
import { useNotificationStore } from '@/stores/notification'
import Layout from '@/components/Common/Layout.vue'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'
import ConfirmModal from '@/components/Common/ConfirmModal.vue'
import { formatDate } from '@/utils/date'
import { useLogger } from '@/composables/useLogger'

const router = useRouter()
const settingsStore = useSettingsStore()
const authStore = useAuthStore()
const notificationStore = useNotificationStore()
const logger = useLogger()

// 当前激活的标签页
const activeTab = ref('profile')

// 标签页定义
const tabs = [
  { key: 'profile', label: '个人信息' },
  { key: 'password', label: '修改密码' },
  { key: 'preferences', label: '偏好设置' },
  { key: 'security', label: '账户安全' }
]

// 个人信息表单
const profileForm = reactive({
  name: '',
  email: '',
  bio: '',
  avatar: ''
})

const profileErrors = reactive({
  name: '',
  email: ''
})

const profileSaving = ref(false)

// 密码修改表单
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const passwordErrors = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const passwordSaving = ref(false)

// 偏好设置表单
const preferencesForm = reactive({
  theme: 'light',
  language: 'zh-CN',
  autoSave: true,
  notifications: true,
  twoFactorEnabled: false
})

const preferencesSaving = ref(false)

// 登录历史
const loginHistory = ref([])

// 模态框状态
const showDeleteAccountModal = ref(false)

// 初始化数据
onMounted(async () => {
  await loadUserData()
  await loadLoginHistory()
})

// 加载用户数据
const loadUserData = async () => {
  try {
    const user = authStore.user
    if (user) {
      profileForm.name = user.name || ''
      profileForm.email = user.email || ''
      profileForm.bio = user.bio || ''
      profileForm.avatar = user.avatar || ''
    }
    
    // 加载用户设置
    await settingsStore.loadSettings()
    const settings = settingsStore.settings
    
    preferencesForm.theme = settings.theme || 'light'
    preferencesForm.language = settings.language || 'zh-CN'
    preferencesForm.autoSave = settings.autoSave ?? true
    preferencesForm.notifications = settings.notifications ?? true
    preferencesForm.twoFactorEnabled = settings.twoFactorEnabled ?? false
    
  } catch (error) {
    logger.error('加载用户数据失败', error)
    notificationStore.error('加载用户数据失败', error.message)
  }
}

// 加载登录历史
const loadLoginHistory = async () => {
  try {
    loginHistory.value = await settingsStore.getLoginHistory()
  } catch (error) {
    logger.error('加载登录历史失败', error)
  }
}

// 处理头像变更
const handleAvatarChange = async (event) => {
  const file = event.target.files[0]
  if (!file) return
  
  try {
    const { uploadAPI } = await import('@/services/upload')
    const result = await uploadAPI.uploadImage(file)
    
    if (result.success) {
      profileForm.avatar = result.data.url
      notificationStore.success('头像上传成功')
    }
  } catch (error) {
    logger.error('头像上传失败', error)
    notificationStore.error('头像上传失败', error.message)
  }
}

// 更新个人信息
const updateProfile = async () => {
  // 重置错误
  Object.keys(profileErrors).forEach(key => {
    profileErrors[key] = ''
  })
  
  // 验证表单
  let hasError = false
  
  if (!profileForm.name.trim()) {
    profileErrors.name = '姓名不能为空'
    hasError = true
  }
  
  if (!profileForm.email.trim()) {
    profileErrors.email = '邮箱不能为空'
    hasError = true
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(profileForm.email)) {
    profileErrors.email = '邮箱格式不正确'
    hasError = true
  }
  
  if (hasError) return
  
  profileSaving.value = true
  
  try {
    await settingsStore.updateProfile({
      name: profileForm.name.trim(),
      email: profileForm.email.trim(),
      bio: profileForm.bio.trim(),
      avatar: profileForm.avatar
    })
    
    notificationStore.success('个人信息更新成功')
    logger.userAction('更新个人信息')
    
  } catch (error) {
    logger.error('更新个人信息失败', error)
    notificationStore.error('更新个人信息失败', error.message)
  } finally {
    profileSaving.value = false
  }
}

// 更新密码
const updatePassword = async () => {
  // 重置错误
  Object.keys(passwordErrors).forEach(key => {
    passwordErrors[key] = ''
  })
  
  // 验证表单
  let hasError = false
  
  if (!passwordForm.currentPassword) {
    passwordErrors.currentPassword = '请输入当前密码'
    hasError = true
  }
  
  if (!passwordForm.newPassword) {
    passwordErrors.newPassword = '请输入新密码'
    hasError = true
  } else if (passwordForm.newPassword.length < 6) {
    passwordErrors.newPassword = '新密码至少6位'
    hasError = true
  }
  
  if (!passwordForm.confirmPassword) {
    passwordErrors.confirmPassword = '请确认新密码'
    hasError = true
  } else if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    passwordErrors.confirmPassword = '两次输入的密码不一致'
    hasError = true
  }
  
  if (hasError) return
  
  passwordSaving.value = true
  
  try {
    await settingsStore.updatePassword({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword
    })
    
    // 清空表单
    passwordForm.currentPassword = ''
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
    
    notificationStore.success('密码修改成功')
    logger.userAction('修改密码')
    
  } catch (error) {
    logger.error('密码修改失败', error)
    notificationStore.error('密码修改失败', error.message)
  } finally {
    passwordSaving.value = false
  }
}

// 更新偏好设置
const updatePreferences = async () => {
  preferencesSaving.value = true
  
  try {
    await settingsStore.updateSettings({
      theme: preferencesForm.theme,
      language: preferencesForm.language,
      autoSave: preferencesForm.autoSave,
      notifications: preferencesForm.notifications,
      twoFactorEnabled: preferencesForm.twoFactorEnabled
    })
    
    notificationStore.success('偏好设置已保存')
    logger.userAction('更新偏好设置')
    
  } catch (error) {
    logger.error('保存偏好设置失败', error)
    notificationStore.error('保存偏好设置失败', error.message)
  } finally {
    preferencesSaving.value = false
  }
}

// 切换两步验证
const toggleTwoFactor = async () => {
  try {
    if (preferencesForm.twoFactorEnabled) {
      // 禁用两步验证
      await settingsStore.disableTwoFactor()
      preferencesForm.twoFactorEnabled = false
      notificationStore.success('两步验证已禁用')
    } else {
      // 启用两步验证
      await settingsStore.enableTwoFactor()
      preferencesForm.twoFactorEnabled = true
      notificationStore.success('两步验证已启用')
    }
    
    logger.userAction('切换两步验证', { enabled: preferencesForm.twoFactorEnabled })
    
  } catch (error) {
    logger.error('切换两步验证失败', error)
    notificationStore.error('操作失败', error.message)
  }
}

// 删除账户
const deleteAccount = async () => {
  try {
    await settingsStore.deleteAccount()
    
    logger.userAction('删除账户')
    notificationStore.success('账户已删除')
    
    // 清理本地数据并跳转到登录页
    authStore.logout()
    router.push('/login')
    
  } catch (error) {
    logger.error('删除账户失败', error)
    notificationStore.error('删除账户失败', error.message)
  }
}
</script>