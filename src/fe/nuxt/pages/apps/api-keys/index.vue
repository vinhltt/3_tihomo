<template>
  <div>
    <!-- Page Header (Tiêu đề trang) -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
      <div>
        <h5 class="font-semibold text-lg dark:text-white-light">API Key Management</h5>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
          Manage your API keys for third-party integrations
        </p>
      </div>

      <!-- Breadcrumb -->
      <NavigationBreadcrumb :items="breadcrumbs" />
    </div>

    <!-- Action Bar (Thanh hành động) -->
    <div class="panel mb-6 !p-4">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="flex items-center space-x-3">
          <h6 class="font-medium text-gray-900 dark:text-white">
            Your API Keys ({{ filteredApiKeys.length }})
          </h6>
          <div v-if="loading" class="flex items-center space-x-2">
            <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-primary"></div>
            <span class="text-sm text-gray-500">Loading...</span>
          </div>
        </div>
        
        <div class="flex items-center space-x-3">
          <!-- View Toggle -->
          <div class="btn-group">
            <button
              @click="viewMode = 'table'"
              :class="[
                'btn btn-sm',
                viewMode === 'table' ? 'btn-primary' : 'btn-outline-primary'
              ]"
            >
              <icon-table-cells class="h-4 w-4" />
            </button>
            <button
              @click="viewMode = 'cards'"
              :class="[
                'btn btn-sm',
                viewMode === 'cards' ? 'btn-primary' : 'btn-outline-primary'
              ]"
            >
              <icon-squares-2x2 class="h-4 w-4" />
            </button>
          </div>
          
          <!-- Refresh Button -->
          <button
            @click="refreshApiKeys"
            class="btn btn-outline-secondary btn-sm"
            :disabled="loading"
          >
            <icon-arrow-path :class="['h-4 w-4', loading && 'animate-spin']" />
            Refresh
          </button>
          
          <!-- Create Button -->
          <button
            @click="showCreateModal = true"
            class="btn btn-primary btn-sm"
          >
            <icon-plus class="h-4 w-4 mr-2" />
            Create API Key
          </button>
        </div>
      </div>
    </div>

    <!-- Quick Stats (Thống kê nhanh) -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div class="panel !p-4">
        <div class="flex items-center space-x-3">
          <div class="flex-shrink-0">
            <icon-key class="h-8 w-8 text-primary" />
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Total Keys</p>
            <p class="text-xl font-semibold text-gray-900 dark:text-white">
              {{ apiKeys.length }}
            </p>
          </div>
        </div>
      </div>
      
      <div class="panel !p-4">
        <div class="flex items-center space-x-3">
          <div class="flex-shrink-0">
            <icon-check-circle class="h-8 w-8 text-success" />
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Active Keys</p>
            <p class="text-xl font-semibold text-success">
              {{ activeKeysCount }}
            </p>
          </div>
        </div>
      </div>
      
      <div class="panel !p-4">
        <div class="flex items-center space-x-3">
          <div class="flex-shrink-0">
            <icon-chart-bar class="h-8 w-8 text-info" />
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Today's Requests</p>
            <p class="text-xl font-semibold text-info">
              {{ totalTodayUsage.toLocaleString() }}
            </p>
          </div>
        </div>
      </div>
      
      <div class="panel !p-4">
        <div class="flex items-center space-x-3">
          <div class="flex-shrink-0">
            <icon-shield-check class="h-8 w-8 text-warning" />
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Avg Security Score</p>
            <p class="text-xl font-semibold text-warning">
              {{ averageSecurityScore }}%
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Area (Khu vực nội dung chính) -->
    <div class="space-y-6">
      <!-- Table View -->
      <div v-if="viewMode === 'table'">
        <ApiKeyList
          :api-keys="filteredApiKeys"
          :loading="loading"
          :selected-key-id="selectedKeyId"
          @select-key="handleSelectKey"
          @create-key="showCreateModal = true"
          @edit-key="handleEditKey"
          @revoke-key="handleRevokeKey"
          @regenerate-key="handleRegenerateKey"
        />
      </div>

      <!-- Cards View -->
      <div v-else-if="viewMode === 'cards'" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <ApiKeyCard
          v-for="apiKey in filteredApiKeys"
          :key="apiKey.id"
          :api-key="apiKey"
          :selected="selectedKeyId === apiKey.id"
          @click="handleSelectKey(apiKey.id)"
          @edit="handleEditKey(apiKey.id)"
          @revoke="handleRevokeKey(apiKey.id)"
          @regenerate="handleRegenerateKey(apiKey.id)"
          @view-usage="handleViewUsage(apiKey.id)"
          @view-analytics="handleViewAnalytics(apiKey.id)"
        />
        
        <!-- Create New Card -->
        <div
          @click="showCreateModal = true"
          class="panel border-2 border-dashed border-gray-300 dark:border-gray-600 flex items-center justify-center min-h-64 cursor-pointer hover:border-primary transition-colors"
        >
          <div class="text-center">
            <div class="w-12 h-12 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-3">
              <icon-plus class="h-6 w-6 text-gray-400" />
            </div>
            <p class="text-gray-500 mb-2">Create new API key</p>
            <button class="btn btn-sm btn-primary">Create API Key</button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!loading && apiKeys.length === 0" class="text-center py-20">
        <div class="w-20 h-20 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-4">
          <icon-key class="h-10 w-10 text-gray-400" />
        </div>
        <h3 class="text-lg font-semibold mb-2 text-gray-900 dark:text-white">No API keys yet</h3>
        <p class="text-gray-500 mb-6 max-w-md mx-auto">
          Create your first API key to start integrating with TiHoMo's services. 
          API keys allow secure access to your data and functionality.
        </p>
        <button @click="showCreateModal = true" class="btn btn-primary">
          <icon-plus class="h-4 w-4 mr-2" />
          Create your first API Key
        </button>
      </div>
    </div>

    <!-- Modals (Các modal) -->
    
    <!-- Create API Key Modal -->
    <div v-if="showCreateModal" class="fixed inset-0 z-[9999] overflow-y-auto">
      <div class="flex min-h-screen items-center justify-center px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <!-- Background overlay -->
        <div 
          class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          @click="showCreateModal = false"
        ></div>
        
        <!-- Modal panel -->
        <div class="inline-block align-bottom bg-white dark:bg-gray-900 rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-4xl sm:w-full sm:p-6">
          <!-- Modal header -->
          <div class="flex items-center justify-between mb-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
              Create New API Key
            </h3>
            <button
              @click="showCreateModal = false"
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              <icon-x-mark class="h-6 w-6" />
            </button>
          </div>
          
          <!-- Modal content -->
          <ApiKeyForm
            :loading="createLoading"
            @submit="handleCreateApiKey"
            @cancel="showCreateModal = false"
          />
        </div>
      </div>
    </div>

    <!-- Edit API Key Modal -->
    <div v-if="showEditModal && editingApiKey" class="fixed inset-0 z-[9999] overflow-y-auto">
      <div class="flex min-h-screen items-center justify-center px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <!-- Background overlay -->
        <div 
          class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          @click="showEditModal = false"
        ></div>
        
        <!-- Modal panel -->
        <div class="inline-block align-bottom bg-white dark:bg-gray-900 rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-4xl sm:w-full sm:p-6">
          <!-- Modal header -->
          <div class="flex items-center justify-between mb-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
              Edit API Key: {{ editingApiKey.name }}
            </h3>
            <button
              @click="showEditModal = false"
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              <icon-x-mark class="h-6 w-6" />
            </button>
          </div>
          
          <!-- Modal content -->
          <ApiKeyForm
            :api-key="editingApiKey"
            :loading="updateLoading"
            @submit="handleUpdateApiKey"
            @cancel="showEditModal = false"
          />
        </div>
      </div>
    </div>

    <!-- Success Message Toast -->
    <Transition
      enter-active-class="transition duration-300 ease-out"
      enter-from-class="transform scale-95 opacity-0"
      enter-to-class="transform scale-100 opacity-100"
      leave-active-class="transition duration-200 ease-in"
      leave-from-class="transform scale-100 opacity-100"
      leave-to-class="transform scale-95 opacity-0"
    >
      <div v-if="showSuccessMessage" class="fixed top-4 right-4 z-50">
        <div class="bg-success text-white px-6 py-3 rounded-lg shadow-lg flex items-center space-x-3">
          <icon-check-circle class="h-5 w-5" />
          <span>{{ successMessage }}</span>
          <button @click="showSuccessMessage = false" class="ml-4 text-white/80 hover:text-white">
            <icon-x-mark class="h-4 w-4" />
          </button>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import type { ApiKey, CreateApiKeyRequest, UpdateApiKeyRequest } from '~/types/api-key'
// Import composables
import { useApiKeys } from '~/composables/useApiKeys'

// Meta tags and page info
useHead({
  title: 'API Key Management - TiHoMo',
  meta: [
    { name: 'description', content: 'Manage your API keys for third-party integrations with TiHoMo services' }
  ]
})

// Composables
const { 
  apiKeys, 
  loading, 
  error,
  fetchApiKeys,
  createApiKey,
  updateApiKey,
  revokeApiKey,
  rotateApiKey
} = useApiKeys()

// Reactive state
const viewMode = ref<'table' | 'cards'>('table')
const selectedKeyId = ref<string>('')
const showCreateModal = ref(false)
const showEditModal = ref(false)
const editingApiKey = ref<ApiKey | null>(null)
const createLoading = ref(false)
const updateLoading = ref(false)
const showSuccessMessage = ref(false)
const successMessage = ref('')

// Breadcrumb navigation
const breadcrumbs = ref([
  { name: 'Apps', href: '/apps' },
  { name: 'API Keys', href: '/apps/api-keys' }
])

/**
 * Computed properties (Thuộc tính computed)
 */
const filteredApiKeys = computed(() => {
  // Có thể thêm filtering logic ở đây nếu cần
  return apiKeys.value
})

const activeKeysCount = computed(() => {
  return apiKeys.value.filter((key: ApiKey) => key.status === 'active').length
})

const totalTodayUsage = computed(() => {
  return apiKeys.value.reduce((total: number, key: ApiKey) => total + key.todayUsageCount, 0)
})

const averageSecurityScore = computed(() => {
  if (apiKeys.value.length === 0) return 0
  
  const totalScore = apiKeys.value.reduce((total: number, key: ApiKey) => {
    // Simple security score calculation
    let score = 0
    if (key.securitySettings.requireHttps) score += 20
    if (key.securitySettings.enableIpValidation && key.ipWhitelist.length > 0) score += 25
    if (key.securitySettings.enableRateLimiting) score += 15
    if (key.expiresAt) score += 15
    if (!key.scopes.includes('admin')) score += 10
    else score += 5
    if (key.securitySettings.enableUsageAnalytics) score += 10
    
    return total + Math.min(score, 100)
  }, 0)
  
  return Math.round(totalScore / apiKeys.value.length)
})

/**
 * Event handlers (Xử lý sự kiện)
 */
const handleSelectKey = (keyId: string): void => {
  selectedKeyId.value = selectedKeyId.value === keyId ? '' : keyId
}

const handleEditKey = (keyId: string): void => {
  const apiKey = apiKeys.value.find(key => key.id === keyId)
  if (apiKey) {
    editingApiKey.value = apiKey
    showEditModal.value = true
  }
}

const handleRevokeKey = async (keyId: string): Promise<void> => {
  // Show confirmation dialog
  const confirmed = confirm('Are you sure you want to revoke this API key? This action cannot be undone.')
  if (!confirmed) return
  
  try {
    await revokeApiKey(keyId)
    showSuccessToast('API key revoked successfully')
    await refreshApiKeys()
  } catch (error) {
    console.error('Failed to revoke API key:', error)
    // Show error toast
  }
}

const handleRegenerateKey = async (keyId: string): Promise<void> => {
  // Show confirmation dialog
  const confirmed = confirm('Are you sure you want to regenerate this API key? The old key will stop working immediately.')
  if (!confirmed) return
  
  try {
    await rotateApiKey(keyId)
    showSuccessToast('API key regenerated successfully')
    await refreshApiKeys()
  } catch (error) {
    console.error('Failed to regenerate API key:', error)
    // Show error toast
  }
}

const handleCreateApiKey = async (request: CreateApiKeyRequest): Promise<void> => {
  try {
    createLoading.value = true
    await createApiKey(request)
    showCreateModal.value = false
    showSuccessToast('API key created successfully')
    await refreshApiKeys()
  } catch (error) {
    console.error('Failed to create API key:', error)
    // Show error toast
  } finally {
    createLoading.value = false
  }
}

const handleUpdateApiKey = async (request: UpdateApiKeyRequest): Promise<void> => {
  if (!editingApiKey.value) return
  
  try {
    updateLoading.value = true
    await updateApiKey(editingApiKey.value.id, request)
    showEditModal.value = false
    editingApiKey.value = null
    showSuccessToast('API key updated successfully')
    await refreshApiKeys()
  } catch (error) {
    console.error('Failed to update API key:', error)
    // Show error toast
  } finally {
    updateLoading.value = false
  }
}

const handleViewUsage = (keyId: string): void => {
  // Navigate to usage analytics page
  navigateTo(`/apps/api-keys/${keyId}/usage`)
}

const handleViewAnalytics = (keyId: string): void => {
  // Navigate to detailed analytics page
  navigateTo(`/apps/api-keys/${keyId}/analytics`)
}

const refreshApiKeys = async (): Promise<void> => {
  await fetchApiKeys()
}

const showSuccessToast = (message: string): void => {
  successMessage.value = message
  showSuccessMessage.value = true
  setTimeout(() => {
    showSuccessMessage.value = false
  }, 5000)
}

/**
 * Lifecycle hooks (Hooks vòng đời)
 */
onMounted(async () => {
  await fetchApiKeys()
})

// Handle escape key to close modals
onMounted(() => {
  const handleEscape = (event: KeyboardEvent) => {
    if (event.key === 'Escape') {
      if (showCreateModal.value) {
        showCreateModal.value = false
      } else if (showEditModal.value) {
        showEditModal.value = false
      }
    }
  }
  
  document.addEventListener('keydown', handleEscape)
  onUnmounted(() => {
    document.removeEventListener('keydown', handleEscape)
  })
})
</script>

<style scoped>
/* Custom styles for modal transitions */
.btn-group {
  @apply inline-flex rounded-lg;
}

.btn-group .btn {
  @apply rounded-none border-r-0;
}

.btn-group .btn:first-child {
  @apply rounded-l-lg;
}

.btn-group .btn:last-child {
  @apply rounded-r-lg border-r;
}

/* Modal overlay improvements */
.modal-overlay {
  backdrop-filter: blur(4px);
}

/* Responsive grid adjustments */
@media (max-width: 768px) {
  .grid-cols-1.md\:grid-cols-2.lg\:grid-cols-3 {
    grid-template-columns: 1fr;
  }
}

@media (min-width: 768px) and (max-width: 1024px) {
  .grid-cols-1.md\:grid-cols-2.lg\:grid-cols-3 {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style> 