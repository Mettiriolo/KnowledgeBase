<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 to-gray-100">
    <div class="w-full max-w-md bg-white/80 rounded-2xl shadow-xl p-8 space-y-6 backdrop-blur">
      <!-- Header -->
      <div class="text-center">
        <router-link to="/login" class="inline-flex items-center gap-2 text-gray-600 hover:text-primary-600 mb-4">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          返回登录
        </router-link>
        <h2 class="text-2xl font-bold text-gray-900">忘记密码？</h2>
        <p class="mt-2 text-sm text-gray-600">
          输入您的邮箱地址，我们将发送密码重置链接
        </p>
      </div>

      <!-- Success Message -->
      <transition name="fade">
        <div v-if="isSuccess" class="bg-green-50 border border-green-200 rounded-lg p-4">
          <div class="flex items-start gap-3">
            <svg class="w-5 h-5 text-green-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <h3 class="font-semibold text-green-900">邮件已发送！</h3>
              <p class="mt-1 text-sm text-green-700">
                如果该邮箱已注册，您将收到密码重置邮件。请检查您的邮箱。
              </p>
            </div>
          </div>
        </div>
      </transition>

      <!-- Form -->
      <form v-if="!isSuccess" @submit.prevent="handleSubmit" class="space-y-4">
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
            邮箱地址
          </label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            :disabled="isLoading"
            placeholder="your@email.com"
            class="input-modern"
            autofocus
          />
          <p v-if="error" class="text-sm text-red-600 mt-1">{{ error }}</p>
        </div>

        <button
          type="submit"
          :disabled="isLoading || !email"
          class="btn-modern w-full flex items-center justify-center gap-2"
        >
          <LoadingSpinner v-if="isLoading" size="small" color="#fff" />
          <span>{{ isLoading ? '发送中...' : '发送重置邮件' }}</span>
        </button>
      </form>

      <!-- Additional Info -->
      <div v-if="isSuccess" class="text-center space-y-4">
        <p class="text-sm text-gray-600">
          没有收到邮件？
        </p>
        <button
          @click="resend"
          :disabled="isLoading || countdown > 0"
          class="text-primary-600 hover:text-primary-700 text-sm font-medium"
        >
          {{ countdown > 0 ? `${countdown}秒后可重新发送` : '重新发送' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const error = ref('')
const isLoading = ref(false)
const isSuccess = ref(false)
const countdown = ref(0)

let countdownTimer = null

const startCountdown = () => {
  countdown.value = 60
  countdownTimer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      clearInterval(countdownTimer)
    }
  }, 1000)
}

const handleSubmit = async () => {
  error.value = ''
  isLoading.value = true

  try {
    await authStore.forgotPassword(email.value)
    isSuccess.value = true
    startCountdown()
  } catch (err) {
    error.value = err.response?.data?.message || '发送失败，请稍后重试'
  } finally {
    isLoading.value = false
  }
}

const resend = () => {
  isSuccess.value = false
  error.value = ''
}

// Cleanup
watch(() => router.currentRoute.value, () => {
  if (countdownTimer) {
    clearInterval(countdownTimer)
  }
})
</script>

<style scoped>
.input-modern {
  width: 100%;
  padding: 0.75rem 1rem;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  font-size: 1rem;
  transition: all 0.2s;
}

.input-modern:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgb(59 130 246 / 10%);
}

.btn-modern {
  background: #3b82f6;
  color: white;
  font-weight: 600;
  padding: 0.75rem 1.5rem;
  border-radius: 0.5rem;
  transition: all 0.2s;
  border: none;
  cursor: pointer;
}

.btn-modern:hover:not(:disabled) {
  background: #2563eb;
}

.btn-modern:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
