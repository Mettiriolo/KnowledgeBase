<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 to-gray-100">
    <div class="w-full max-w-md bg-white/80 rounded-2xl shadow-xl p-8 space-y-6 backdrop-blur">
      <!-- Loading State -->
      <div v-if="isVerifying" class="text-center py-8">
        <LoadingSpinner size="large" />
        <p class="mt-4 text-gray-600">验证重置链接...</p>
      </div>

      <!-- Invalid Token -->
      <div v-else-if="!isValidToken" class="text-center space-y-4">
        <div class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto">
          <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>
        <h2 class="text-2xl font-bold text-gray-900">链接无效或已过期</h2>
        <p class="text-gray-600">密码重置链接可能已过期或无效。请重新申请。</p>
        <router-link to="/forgot-password" class="inline-block">
          <button class="btn-modern">
            重新申请
          </button>
        </router-link>
      </div>

      <!-- Reset Form -->
      <template v-else>
        <!-- Header -->
        <div class="text-center">
          <h2 class="text-2xl font-bold text-gray-900">重置密码</h2>
          <p class="mt-2 text-sm text-gray-600">
            请输入您的新密码
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
                <h3 class="font-semibold text-green-900">密码重置成功！</h3>
                <p class="mt-1 text-sm text-green-700">
                  您的密码已更新，正在跳转到登录页...
                </p>
              </div>
            </div>
          </div>
        </transition>

        <!-- Form -->
        <form v-if="!isSuccess" @submit.prevent="handleSubmit" class="space-y-4">
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
              新密码
            </label>
            <div class="relative">
              <input
                id="password"
                v-model="form.newPassword"
                :type="showPassword ? 'text' : 'password'"
                required
                :disabled="isLoading"
                placeholder="至少6个字符"
                class="input-modern pr-10"
                autofocus
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
              >
                <svg v-if="!showPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
                </svg>
              </button>
            </div>
            <p v-if="errors.password" class="text-sm text-red-600 mt-1">{{ errors.password }}</p>
          </div>

          <div>
            <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">
              确认新密码
            </label>
            <input
              id="confirmPassword"
              v-model="form.confirmPassword"
              type="password"
              required
              :disabled="isLoading"
              placeholder="再次输入新密码"
              class="input-modern"
            />
            <p v-if="errors.confirmPassword" class="text-sm text-red-600 mt-1">{{ errors.confirmPassword }}</p>
          </div>

          <p v-if="error" class="text-sm text-red-600">{{ error }}</p>

          <button
            type="submit"
            :disabled="isLoading || !form.newPassword || !form.confirmPassword"
            class="btn-modern w-full flex items-center justify-center gap-2"
          >
            <LoadingSpinner v-if="isLoading" size="small" color="#fff" />
            <span>{{ isLoading ? '重置中...' : '重置密码' }}</span>
          </button>
        </form>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoadingSpinner from '@/components/Common/LoadingSpinner.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const isVerifying = ref(true)
const isValidToken = ref(false)
const isLoading = ref(false)
const isSuccess = ref(false)
const showPassword = ref(false)
const error = ref('')

const form = reactive({
  newPassword: '',
  confirmPassword: ''
})

const errors = reactive({
  password: '',
  confirmPassword: ''
})

const token = route.query.token
const email = route.query.email

// Verify token on mount
onMounted(async () => {
  if (!token || !email) {
    isValidToken.value = false
    isVerifying.value = false
    return
  }

  try {
    const response = await authStore.verifyResetToken(token, email)
    isValidToken.value = response.data.isValid
  } catch {
    isValidToken.value = false
  } finally {
    isVerifying.value = false
  }
})

const validateForm = () => {
  errors.password = ''
  errors.confirmPassword = ''
  
  if (form.newPassword.length < 6) {
    errors.password = '密码至少6个字符'
    return false
  }
  
  if (form.newPassword !== form.confirmPassword) {
    errors.confirmPassword = '两次密码输入不一致'
    return false
  }
  
  return true
}

const handleSubmit = async () => {
  if (!validateForm()) return

  error.value = ''
  isLoading.value = true

  try {
    await authStore.resetPassword(
      token,
      email,
      form.newPassword,
      form.confirmPassword
    )
    
    isSuccess.value = true
    
    // Redirect to login after 3 seconds
    setTimeout(() => {
      router.push('/login')
    }, 3000)
  } catch (err) {
    error.value = err.response?.data?.message || '重置密码失败，请稍后重试'
  } finally {
    isLoading.value = false
  }
}
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
