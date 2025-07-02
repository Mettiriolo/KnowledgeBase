<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 to-gray-100">
    <div class="w-full max-w-md bg-white/80 rounded-2xl shadow-xl p-8 sm:p-10 space-y-8 backdrop-blur">
      <!-- Logo & Brand -->
      <div class="flex flex-col items-center gap-2">
        <div class="mx-auto h-12 w-12 bg-primary-500 rounded-lg flex items-center justify-center">
          <svg class="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h1 class="text-2xl font-bold text-primary-600 tracking-tight">AI知识库</h1>
      </div>

      <!-- 表单 -->
      <form @submit.prevent="handleSubmit" class="space-y-5">
        <transition name="fade">
          <div v-if="error" key="error" class="bg-red-100 border border-red-200 text-red-700 rounded px-3 py-2 text-sm flex items-center gap-2" role="alert">
            <svg class="w-4 h-4 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
            <span>{{ error }}</span>
          </div>
        </transition>
        <div class="space-y-4">
          <div v-if="!isLogin">
            <input
              v-model="form.username"
              type="text"
              :disabled="isLoading"
              placeholder="用户名"
              class="input-modern"
              autocomplete="username"
              autofocus
            />
            <p v-if="errors.username" class="text-xs text-red-500 mt-1">{{ errors.username }}</p>
          </div>
          <div>
            <input
              v-model="form.email"
              type="email"
              :disabled="isLoading"
              placeholder="邮箱地址"
              class="input-modern"
              autocomplete="email"
              :autofocus="isLogin"
            />
            <p v-if="errors.email" class="text-xs text-red-500 mt-1">{{ errors.email }}</p>
          </div>
          <div class="relative">
            <input
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              :disabled="isLoading"
              placeholder="密码"
              class="input-modern pr-10"
              autocomplete="current-password"
            />
            <button type="button" @click="showPassword = !showPassword" tabindex="-1" class="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-primary-500 focus:outline-none">
              <svg v-if="!showPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
              <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" /></svg>
            </button>
            <p v-if="errors.password" class="text-xs text-red-500 mt-1">{{ errors.password }}</p>
          </div>
          <div v-if="!isLogin">
            <input
              v-model="form.confirmPassword"
              type="password"
              :disabled="isLoading"
              placeholder="确认密码"
              class="input-modern"
              autocomplete="new-password"
            />
            <p v-if="errors.confirmPassword" class="text-xs text-red-500 mt-1">{{ errors.confirmPassword }}</p>
          </div>
        </div>
        <div v-if="isLogin" class="flex items-center justify-between text-xs mt-2">
          <label class="flex items-center gap-2 select-none">
            <input v-model="form.rememberMe" type="checkbox" class="rounded border-gray-300 text-primary-600 focus:ring-primary-500" /> 记住我
          </label>
          <!-- <button type="button" class="text-primary-400 hover:underline focus:outline-none" @click="showForgot = true">忘记密码？</button> -->
          <router-link to="/forgot-password" class="text-primary-400 hover:underline focus:outline-none" tabindex="-1">
            忘记密码？
          </router-link>
        </div>
        <button type="submit" :disabled="isLoading" class="btn-modern w-full flex items-center justify-center gap-2">
          <LoadingSpinner v-if="isLoading" size="small" color="#fff" />
          <span>{{ isLogin ? '登录' : '注册' }}</span>
        </button>
        <button
          type="button"
          @click="toggleMode"
          class="block mx-auto mt-3 text-primary-500 hover:underline text-sm focus:outline-none"
        >
          {{ isLogin ? '没有账号？去注册' : '已有账号？去登录' }}
        </button>
      </form>
      <!-- 分割线 -->
      <div class="flex items-center gap-2 my-2">
        <div class="flex-1 h-px bg-gray-200"></div>
        <span class="text-gray-400 text-xs">或</span>
        <div class="flex-1 h-px bg-gray-200"></div>
      </div>
      <!-- 演示账户登录 -->
      <button @click="loginDemo" :disabled="isLoading" class="btn-demo w-full flex items-center justify-center gap-2">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" /></svg>
        <span>使用演示账户登录</span>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotificationStore } from '@/stores/notification'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const notificationStore = useNotificationStore()

const isLogin = ref(true)
const showPassword = ref(false)
const isLoading = ref(false)
const error = ref('')
const errors = reactive({})

const form = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  rememberMe: false
})

const redirectUrl = computed(() => route.query.redirect || '/dashboard')

// 忘记密码弹窗相关
const showForgot = ref(false)
const forgotEmail = ref('')
const forgotLoading = ref(false)
const forgotError = ref('')
const forgotSuccess = ref('')

const toggleMode = () => {
  isLogin.value = !isLogin.value
  error.value = ''
  Object.keys(errors).forEach(key => delete errors[key])
  Object.keys(form).forEach(key => {
    if (typeof form[key] === 'boolean') {
      form[key] = false
    } else {
      form[key] = ''
    }
  })
}

const validateForm = () => {
  Object.keys(errors).forEach(key => delete errors[key])
  let isValid = true
  if (!isLogin.value) {
    if (!form.username.trim()) {
      errors.username = '请输入用户名'
      isValid = false
    } else if (form.username.length < 3) {
      errors.username = '用户名至少3个字符'
      isValid = false
    }
    if (form.password !== form.confirmPassword) {
      errors.confirmPassword = '两次输入的密码不一致'
      isValid = false
    }
  }
  if (!form.email.trim()) {
    errors.email = '请输入邮箱地址'
    isValid = false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = '请输入有效的邮箱地址'
    isValid = false
  }
  if (!form.password.trim()) {
    errors.password = '请输入密码'
    isValid = false
  } else if (form.password.length < 6) {
    errors.password = '密码至少6个字符'
    isValid = false
  }
  return isValid
}

const handleSubmit = async () => {
  if (!validateForm()) return
  error.value = ''
  isLoading.value = true
  try {
    if (isLogin.value) {
      await authStore.login(form.email, form.password)
      notificationStore.success('登录成功', '欢迎回来！')
    } else {
      await authStore.register(form.username, form.email, form.password)
      notificationStore.success('注册成功', '欢迎加入AI知识库！')
    }
    router.push(redirectUrl.value)
  } catch (err) {
    error.value = err.response?.data?.message || err.message || (isLogin.value ? '登录失败' : '注册失败')
  } finally {
    isLoading.value = false
  }
}

const loginDemo = async () => {
  isLoading.value = true
  error.value = ''
  try {
    await authStore.login('demo@example.com', 'demo123')
    notificationStore.success('演示登录成功')
    router.push(redirectUrl.value)
  } catch (error) {
    error.value = '演示登录失败，请稍后重试'
  } finally {
    isLoading.value = false
  }
}

// 错误提示自动消失
watch(error, val => {
  if (val) setTimeout(() => error.value = '', 3000)
})
</script>

<style scoped>
.input-modern {
  width: 100%;
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 0.75rem;
  background: #f7fafc;
  box-shadow: 0 1px 2px 0 rgb(0 0 0 / 2%);
  font-size: 1rem;
  outline: none;
  transition: box-shadow 0.2s, background 0.2s;
  margin-bottom: 0.25rem;
}
.input-modern:focus {
  background: #fff;
  box-shadow: 0 0 0 2px #3b82f6;
}
.btn-modern {
  background: linear-gradient(90deg, #3b82f6 0%, #2563eb 100%);
  color: #fff;
  font-weight: 600;
  border: none;
  border-radius: 0.75rem;
  padding: 0.75rem 0;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.2s, box-shadow 0.2s;
  box-shadow: 0 2px 8px 0 rgb(59 130 246 / 8%);
}
.btn-modern:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.btn-modern:hover:not(:disabled) {
  background: linear-gradient(90deg, #2563eb 0%, #3b82f6 100%);
}
.btn-demo {
  background: #f3f4f6;
  color: #374151;
  font-weight: 500;
  border: none;
  border-radius: 0.75rem;
  padding: 0.7rem 0;
  font-size: 1rem;
  transition: background 0.2s;
}
.btn-demo:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.btn-demo:hover:not(:disabled) {
  background: #e5e7eb;
}
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>