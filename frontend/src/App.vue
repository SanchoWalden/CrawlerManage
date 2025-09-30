<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import type { SnackbarColor } from '@/types'
import { auth, isAuthenticated, logout as authLogout, restoreAuth } from '@/services/auth'

const router = useRouter()
const route = useRoute()

const dashboardRef = ref<any>(null)

const snackbar = reactive({
  visible: false,
  message: '',
  color: 'success' as SnackbarColor,
})

const isAuthPage = computed(() => route.name === 'login')

function showSnackbar(message: string, color: SnackbarColor = 'success') {
  snackbar.message = message
  snackbar.color = color
  snackbar.visible = true
}

function logout() {
  authLogout()
  showSnackbar('已退出登录', 'info')
  router.push('/login')
}

function refreshData() {
  console.log('refreshData called, dashboardRef:', dashboardRef.value)
  dashboardRef.value?.fetchItems?.()
}

function openCreateDialog() {
  console.log('openCreateDialog called, dashboardRef:', dashboardRef.value)
  dashboardRef.value?.openCreateDialog?.()
}

function setComponentRef(el: any) {
  if (route.name === 'dashboard') {
    dashboardRef.value = el
    console.log('Dashboard ref set:', el)
  }
}

onMounted(async () => {
  await restoreAuth()
})
</script>

<template>
  <v-app>
    <v-app-bar v-if="!isAuthPage" color="primary" density="comfortable" elevation="2">
      <v-app-bar-title>爬虫数据管理</v-app-bar-title>
      <v-spacer />
      <template v-if="isAuthenticated()">
        <v-chip color="primary" variant="flat" class="mr-4" prepend-icon="mdi-account-circle">
          {{ auth.user?.displayName ?? auth.user?.userName ?? '用户' }}
        </v-chip>
        <v-btn
          icon="mdi-refresh"
          :loading="dashboardRef?.loading"
          @click="refreshData"
          variant="text"
        />
        <v-btn color="secondary" class="ml-2" prepend-icon="mdi-plus" @click="openCreateDialog">
          新增记录
        </v-btn>
        <v-btn class="ml-2" variant="text" prepend-icon="mdi-logout" @click="logout">
          退出登录
        </v-btn>
      </template>
    </v-app-bar>

    <v-main>
      <router-view
        v-slot="{ Component }"
      >
        <component
          :is="Component"
          :ref="setComponentRef"
          @snackbar="showSnackbar"
        />
      </router-view>
    </v-main>

    <v-snackbar v-model="snackbar.visible" :color="snackbar.color" timeout="3000">
      {{ snackbar.message }}
      <template #actions>
        <v-btn variant="text" @click="snackbar.visible = false">关闭</v-btn>
      </template>
    </v-snackbar>
  </v-app>
</template>

<style scoped>
.v-application {
  background-color: #f5f7fa;
}
</style>