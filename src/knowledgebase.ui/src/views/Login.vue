<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <div class="mx-auto h-12 w-12 bg-primary-500 rounded-lg flex items-center justify-center">
          <svg class="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          {{ isLogin ? '登录到您的账户' : '创建新账户' }}
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          {{ isLogin ? '还没有账户？' : '已有账户？' }}
          <button
            @click="toggleMode"
            class="font-medium text-primary-600 hover:text-primary-500"
          >
            {{ isLogin ? '立即注册' : '立即登录' }}
          </button>
        </p>
      </div>

      <form @submit.prevent="handleSubmit" class="mt-8 space-y-6">
        <div class="space-y-4">
          <!-- 用户名（仅注册时显示） -->
          <div v-if="!isLogin">
            <label for="username" class="block text-sm font-medium text-gray-700">
              用户名
            </label>
            <input
              id="username"
              v-model="form.username"
              type="text"
              required
              :disabled="isLoading"
              class="input"
              placeholder="请输入用户名"
            />
            <p v-if="errors.username" class="mt-1 text-sm text-red-600">
              {{ errors.username }}
            </p>
          </div>

          <!-- 邮箱 -->
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700">
              邮箱地址
            </label>
            <input
              id="email"
              v-model="form.email"
              type="email"
              required
              :disabled="isLoading"
              class="input"
              placeholder="请输入邮箱地址"
            />
            <p v-if="errors.email" class="mt-1 text-sm text-red-600">
              {{ errors.email }}
            </p>
          </div>

          <!-- 密码 -->
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700">
              密码
            </label>
            <div class="relative">
              <input
                id="password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                required
                :disabled="isLoading"
                class="input pr-10"
                placeholder="请输入密码"
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                <svg
                  :class="showPassword ? 'text-primary-500' : 'text-gray-400'"
                  class="h-5 w-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    v-if="!showPassword"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                  />
                  <path
                    v-if="!showPassword"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                  />
                  <path
                    v-else
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"
                  />
                </svg>
              </button>
            </div>
            <p v-if="errors.password" class="mt-1 text-sm text-red-600">
              {{ errors.password }}
            </p>
          </div>

          <!-- 确认密码（仅注册时显示） -->
          <div v-if="!isLogin">
            <label for="confirmPassword" class="block text-sm font-medium text-gray-700">
              确认密码
            </label>
            <input
              id="confirmPassword"
              v-model="form.confirmPassword"
              type="password"
              required    
              :disabled="isLoading"
              class="input"
              placeholder="请再次输入密码"
            />
            <p v-if="errors.confirmPassword" class="mt-1 text-sm text-red-600">
              {{ errors.confirmPassword }}
            </p>
          </div>
        </div>

        <!-- 错误信息 -->
        <div v-if="error" class="bg-red-50 border border-red-200 rounded-md p-4">
          <div class="flex">
            <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div class="ml-3">
              <p class="text-sm text-red-700">{{ error }}</p>
            </div>
          </div>
        </div>

        <!-- 提交按钮 -->
        <div>
          <button
            type="submit"
            :disabled="isLoading"
            class="w-full btn btn-primary relative"
          >
            <LoadingSpinner v-if="isLoading" size="small" color="white" class="absolute left-4" />
            <span :class="{ 'ml-8': isLoading }">
              {{ isLogin ? '登录' : '注册' }}
            </span>
          </button>
        </div>

        <!-- 记住我（仅登录时显示） -->
        <div v-if="isLogin" class="flex items-center justify-between">
          <div class="flex items-center">
            <input
              id="remember-me"
              v-model="form.rememberMe"
              type="checkbox"
              class="h-4 w-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            />
            <label for="remember-me" class="ml-2 block text-sm text-gray-900">
              记住我
            </label>
          </div>
          <div class="text-sm">
            <a href="#" class="font-medium text-primary-600 hover:text-primary-500">
              忘记密码？
            </a>
          </div>
        </div>
      </form>

      <!-- 演示账户 -->
      <div class="mt-6">
        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-300" />
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-2 bg-gray-50 text-gray-500">或</span>
          </div>
        </div>
        <div class="mt-6">
          <button
            @click="loginDemo"
            :disabled="isLoading"
            class="w-full btn bg-gray-100 text-gray-700 hover:bg-gray-200"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            使用演示账户登录
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
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

const toggleMode = () => {
  isLogin.value = !isLogin.value
  error.value = ''
  Object.keys(errors).forEach(key => delete errors[key])
  // 清空表单
  Object.keys(form).forEach(key => {
    if (typeof form[key] === 'boolean') {
      form[key] = false
    } else {
      form[key] = ''
    }
  })
}

const validateForm = () => {
  // 清空之前的错误
  Object.keys(errors).forEach(key => delete errors[key])
  let isValid = true

  if (!isLogin.value) {
    if (!form.username.trim()) {
      errors.username = '请输入用户名'
      isValid = false
    } else if (form.username.length < 3) {
      errors.username = '用户名至少需要3个字符'
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
    errors.password = '密码至少需要6个字符'
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
  } catch (err) {
    error.value = '演示登录失败，请稍后重试'
  } finally {
    isLoading.value = false
  }
}
</script>