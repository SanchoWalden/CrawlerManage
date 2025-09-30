<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { AuthResponse } from '@/types'
import { apiBaseUrl } from '@/services/api'
import { applyAuth } from '@/services/auth'

const router = useRouter()

const loginForm = reactive({
  account: '',
  password: '',
})
const loginFormRef = ref<any>(null)
const loginFormValid = ref(false)
const loginLoading = ref(false)
const loginError = ref('')

const registerForm = reactive({
  email: '',
  userName: '',
  password: '',
  confirmPassword: '',
  displayName: '',
})
const registerFormRef = ref<any>(null)
const registerFormValid = ref(false)
const registerLoading = ref(false)
const registerError = ref('')
const showRegister = ref(false)

const emit = defineEmits<{
  snackbar: [message: string, color?: 'success' | 'error' | 'info' | 'warning']
}>()

const loginRules = {
  required: (value: string) => (!!value && value.trim().length > 0) || '请输入必填项',
}

const registerRules = {
  required: (value: string) => (!!value && value.trim().length > 0) || '请输入必填项',
  email: (value: string) => {
    const trimmed = value?.trim()
    if (!trimmed) return '请输入邮箱'
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailPattern.test(trimmed) || '请输入有效的邮箱地址'
  },
  minLength: (min: number) => (value: string) =>
    (value && value.length >= min) || `长度至少为 ${min} 个字符`,
  userName: (value: string) => {
    const trimmed = value?.trim()
    if (!trimmed) return '请输入用户名'
    if (trimmed.length > 64) return '用户名最多64个字符'
    return true
  },
  confirmPassword: (value: string) =>
    value === registerForm.password || '两次输入的密码不一致',
}

async function handleLogin() {
  const validation = await loginFormRef.value?.validate?.()
  if (!validation?.valid) {
    return
  }

  loginLoading.value = true
  loginError.value = ''
  try {
    const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        emailOrUserName: loginForm.account.trim(),
        password: loginForm.password,
      }),
    })

    if (!response.ok) {
      const message = await response.text()
      loginError.value = message || '登录失败，请检查账号密码'
      return
    }

    const data = (await response.json()) as AuthResponse
    applyAuth(data)
    emit('snackbar', '登录成功')
    router.push('/dashboard')
  } catch (error) {
    console.error(error)
    loginError.value = '登录失败，请稍后再试'
  } finally {
    loginLoading.value = false
  }
}

async function handleRegister() {
  const validation = await registerFormRef.value?.validate?.()
  if (!validation?.valid) {
    return
  }

  registerLoading.value = true
  registerError.value = ''
  try {
    const response = await fetch(`${apiBaseUrl}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email: registerForm.email.trim(),
        userName: registerForm.userName.trim(),
        password: registerForm.password,
        displayName: registerForm.displayName.trim() || undefined,
      }),
    })

    if (!response.ok) {
      const contentType = response.headers.get('content-type')
      if (contentType?.includes('application/json')) {
        const data = await response.json()
        registerError.value = data.message || '注册失败'
      } else {
        const text = await response.text()
        registerError.value = text || '注册失败'
      }
      return
    }

    const data = (await response.json()) as AuthResponse
    applyAuth(data)
    emit('snackbar', '注册成功')
    showRegister.value = false
    router.push('/dashboard')
  } catch (error) {
    console.error(error)
    registerError.value = '注册失败，请稍后再试'
  } finally {
    registerLoading.value = false
  }
}

function switchToRegister() {
  showRegister.value = true
  registerError.value = ''
  registerFormRef.value?.resetValidation?.()
}

function switchToLogin() {
  showRegister.value = false
  loginError.value = ''
  loginFormRef.value?.resetValidation?.()
}
</script>

<template>
  <v-container class="py-8" fluid>
    <v-row justify="center">
      <v-col cols="12" sm="8" md="5">
        <v-card elevation="4">
          <v-card-title class="text-h6">
            {{ showRegister ? '用户注册' : '用户登录' }}
          </v-card-title>
          <v-card-text>
            <!-- 登录表单 -->
            <template v-if="!showRegister">
              <v-alert v-if="loginError" type="error" variant="tonal" class="mb-4">
                {{ loginError }}
              </v-alert>
              <v-form ref="loginFormRef" v-model="loginFormValid" lazy-validation>
                <v-text-field
                  v-model="loginForm.account"
                  :rules="[loginRules.required]"
                  label="邮箱或用户名"
                  variant="outlined"
                  prepend-inner-icon="mdi-account"
                  autocomplete="username"
                />
                <v-text-field
                  v-model="loginForm.password"
                  :rules="[loginRules.required]"
                  label="密码"
                  variant="outlined"
                  prepend-inner-icon="mdi-lock"
                  type="password"
                  autocomplete="current-password"
                />
                <v-btn
                  block
                  color="primary"
                  class="mt-2"
                  :loading="loginLoading"
                  @click="handleLogin"
                >
                  登录
                </v-btn>
                <v-divider class="my-4" />
                <div class="text-center">
                  <span class="text-body-2 text-medium-emphasis">还没有账号？</span>
                  <v-btn variant="text" color="primary" size="small" @click="switchToRegister">
                    立即注册
                  </v-btn>
                </div>
              </v-form>
            </template>

            <!-- 注册表单 -->
            <template v-else>
              <v-alert v-if="registerError" type="error" variant="tonal" class="mb-4">
                {{ registerError }}
              </v-alert>
              <v-form ref="registerFormRef" v-model="registerFormValid" lazy-validation>
                <v-text-field
                  v-model="registerForm.email"
                  :rules="[registerRules.required, registerRules.email]"
                  label="邮箱"
                  variant="outlined"
                  prepend-inner-icon="mdi-email"
                  autocomplete="email"
                />
                <v-text-field
                  v-model="registerForm.userName"
                  :rules="[registerRules.required, registerRules.userName]"
                  label="用户名"
                  variant="outlined"
                  prepend-inner-icon="mdi-account"
                  autocomplete="username"
                />
                <v-text-field
                  v-model="registerForm.password"
                  :rules="[registerRules.required, registerRules.minLength(6)]"
                  label="密码"
                  variant="outlined"
                  prepend-inner-icon="mdi-lock"
                  type="password"
                  autocomplete="new-password"
                />
                <v-text-field
                  v-model="registerForm.confirmPassword"
                  :rules="[registerRules.required, registerRules.confirmPassword]"
                  label="确认密码"
                  variant="outlined"
                  prepend-inner-icon="mdi-lock-check"
                  type="password"
                  autocomplete="new-password"
                />
                <v-text-field
                  v-model="registerForm.displayName"
                  label="显示名称（可选）"
                  variant="outlined"
                  prepend-inner-icon="mdi-account-details"
                />
                <v-btn
                  block
                  color="primary"
                  class="mt-2"
                  :loading="registerLoading"
                  @click="handleRegister"
                >
                  注册
                </v-btn>
                <v-divider class="my-4" />
                <div class="text-center">
                  <span class="text-body-2 text-medium-emphasis">已有账号？</span>
                  <v-btn variant="text" color="primary" size="small" @click="switchToLogin">
                    返回登录
                  </v-btn>
                </div>
              </v-form>
            </template>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>