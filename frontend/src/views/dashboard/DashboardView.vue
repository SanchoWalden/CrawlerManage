<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { VDataTable } from 'vuetify/components'
import type { ScrapedItem, PaginatedResponse, SnackbarColor } from '@/types'
import { apiBaseUrl } from '@/services/api'
import { authorizedFetch, isAuthenticated } from '@/services/auth'
import { formatDateTime } from '@/utils/formatters'

const router = useRouter()

const items = ref<ScrapedItem[]>([])
const totalItems = ref(0)
const loading = ref(false)

const dialog = ref(false)
const formRef = ref<any>(null)
const formValid = ref(false)
const form = reactive({
  title: '',
  url: '',
  source: '',
  summary: '',
})

const emit = defineEmits<{
  snackbar: [message: string, color?: SnackbarColor]
}>()

const headers = [
  { title: '标题', key: 'title' },
  { title: '来源', key: 'source' },
  { title: '采集时间', key: 'collectedAtDisplay' },
  { title: '摘要', key: 'summary' },
] as const

const displayItems = computed(() =>
  items.value.map(item => ({
    ...item,
    source: item.source ?? '—',
    summary: item.summary ?? '—',
    collectedAtDisplay: formatDateTime(item.collectedAt),
  })),
)

const rules = {
  required: (value: string) => (!!value && value.trim().length > 0) || '该字段为必填项',
  url: (value: string) => {
    const trimmed = value?.trim()
    if (!trimmed) {
      return '请输入合法的 URL'
    }
    try {
      new URL(trimmed)
      return true
    } catch {
      return '请输入合法的 URL'
    }
  },
}

function resetForm() {
  form.title = ''
  form.url = ''
  form.source = ''
  form.summary = ''
  formRef.value?.resetValidation?.()
}

async function fetchItems() {
  if (!isAuthenticated()) {
    router.push('/login')
    return
  }

  loading.value = true
  try {
    const params = new URLSearchParams({ page: '1', pageSize: '50' })
    const response = await authorizedFetch(`${apiBaseUrl}/api/scraped-items?${params.toString()}`)
    if (!response.ok) {
      throw new Error('加载数据失败')
    }
    const data = (await response.json()) as PaginatedResponse
    items.value = data.items ?? []
    totalItems.value = data.total ?? items.value.length
  } catch (error) {
    if ((error as Error).message === 'UNAUTHORIZED') {
      router.push('/login')
      return
    }
    console.error(error)
    emit('snackbar', error instanceof Error ? error.message : '加载数据失败', 'error')
  } finally {
    loading.value = false
  }
}

async function submit() {
  const validation = await formRef.value?.validate?.()
  if (!validation?.valid) {
    return
  }

  try {
    const payload = {
      title: form.title.trim(),
      url: form.url.trim(),
      source: form.source.trim() || undefined,
      summary: form.summary.trim() || undefined,
    }

    const response = await authorizedFetch(`${apiBaseUrl}/api/scraped-items`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || '保存失败')
    }

    emit('snackbar', '记录已保存')
    dialog.value = false
    resetForm()
    await fetchItems()
  } catch (error) {
    if ((error as Error).message === 'UNAUTHORIZED') {
      router.push('/login')
      return
    }
    console.error(error)
    emit('snackbar', error instanceof Error ? error.message : '保存失败', 'error')
  }
}

function openCreateDialog() {
  resetForm()
  dialog.value = true
}

function openExternalLink(url: string) {
  window.open(url, '_blank', 'noreferrer noopener')
}

onMounted(async () => {
  await fetchItems()
})

defineExpose({
  fetchItems,
  openCreateDialog,
  loading,
})
</script>

<template>
  <v-container class="py-8" fluid>
    <v-card elevation="2">
      <VDataTable
        :headers="headers"
        :items="displayItems"
        :loading="loading"
        item-key="id"
        hover
        class="elevation-0"
        no-data-text="暂无数据"
        loading-text="正在加载..."
      >
        <template #item.title="{ item }">
          <div class="d-flex flex-column align-start">
            <span class="font-weight-medium">{{ item.title }}</span>
            <v-btn
              variant="text"
              size="small"
              prepend-icon="mdi-open-in-new"
              class="px-0"
              @click="openExternalLink(item.url)"
            >
              {{ item.url }}
            </v-btn>
          </div>
        </template>
      </VDataTable>
    </v-card>

    <v-dialog v-model="dialog" max-width="520">
      <v-card>
        <v-card-title class="text-h6">新增记录</v-card-title>
        <v-card-text>
          <v-form ref="formRef" v-model="formValid" lazy-validation>
            <v-text-field
              v-model="form.title"
              :rules="[rules.required]"
              label="标题"
              variant="outlined"
              required
            />
            <v-text-field
              v-model="form.url"
              :rules="[rules.required, rules.url]"
              label="URL"
              variant="outlined"
              required
            />
            <v-text-field
              v-model="form.source"
              label="来源"
              variant="outlined"
            />
            <v-textarea
              v-model="form.summary"
              label="摘要"
              rows="3"
              variant="outlined"
            />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="dialog = false">取消</v-btn>
          <v-btn color="primary" @click="submit">保存</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>