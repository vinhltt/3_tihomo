<template>
  <div>
    <!-- Page Header (Tiêu đề trang) -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
      <div>
        <h5 class="font-semibold text-lg dark:text-white-light">API Keys</h5>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
          Create and manage API keys to access TiHoMo services
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
        
        <!-- Single Create Button -->
        <button
          @click="handleOpenCreateModal"
          class="btn btn-primary"
        >
          <icon-plus class="h-4 w-4 mr-2" />
          Create API Key
        </button>
      </div>
    </div>

    <!-- Quick Stats (Simplified) -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
      <div class="panel !p-4">
        <div class="flex items-center space-x-3">
          <div class="flex-shrink-0">
            <icon-lock class="h-8 w-8 text-primary" />
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Total API Keys</p>
            <p class="text-xl font-semibold text-gray-900 dark:text-white">
              {{ apiKeys.length }}
            </p>
          </div>
        </div>
      </div>
      
      <div class="panel !p-4">
        <div class="flex items-center space-x-3">
          <div class="flex-shrink-0">
            <icon-circle-check class="h-8 w-8 text-success" />
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Active Keys</p>
            <p class="text-xl font-semibold text-success">
              {{ activeKeysCount }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Area - API Keys List -->
    <div class="space-y-6">
      <!-- API Keys List -->
      <div v-if="!loading && apiKeys.length > 0">
        <ApiKeyList
          :api-keys="filteredApiKeys"
          :loading="loading"
          :selected-key-id="selectedKeyId"
          @select-key="handleSelectKey"
          @copy-key-prefix="handleCopyKeyPrefix"
          @revoke-key="handleRevokeKey"
          @regenerate-key="handleRegenerateKey"
        />
      </div>

      <!-- Empty State -->
      <div v-else-if="!loading && apiKeys.length === 0" class="text-center py-20">
        <div class="w-20 h-20 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-4">
          <icon-lock class="h-10 w-10 text-gray-400" />
        </div>
        <h3 class="text-lg font-semibold mb-2 text-gray-900 dark:text-white">No API keys yet</h3>
        <p class="text-gray-500 mb-6 max-w-md mx-auto">
          Create your first API key to start integrating with TiHoMo's services. 
          API keys allow secure access to your data and functionality.
        </p>
        <button @click="handleOpenCreateModal" class="btn btn-primary">
          <icon-plus class="h-4 w-4 mr-2" />
          Create your first API Key
        </button>
      </div>

      <!-- Loading State -->
      <div v-else-if="loading" class="text-center py-20">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
        <p class="text-gray-500">Loading API keys...</p>
      </div>
    </div>

    <!-- Modals -->

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
          <icon-circle-check class="h-5 w-5" />
          <span>{{ successMessage }}</span>
          <button @click="showSuccessMessage = false" class="ml-4 text-white/80 hover:text-white">
            <icon-x class="h-4 w-4" />
          </button>
        </div>
      </div>
    </Transition>

    <!-- Simple Create API Key Modal -->
    <div v-if="showSimpleCreateModal" class="fixed inset-0 z-[9999] overflow-y-auto">
      <div class="flex min-h-screen items-center justify-center px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <!-- Background overlay -->
        <div 
          class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          @click="showSimpleCreateModal = false"
        ></div>
        
        <!-- Modal panel -->
        <div class="inline-block align-bottom bg-white dark:bg-gray-900 rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full sm:p-6">
          <SimpleApiKeyForm
            :loading="createLoading"
            @submit="handleCreateSimpleApiKey"
            @cancel="showSimpleCreateModal = false"
          />
        </div>
      </div>
    </div>

    <!-- API Key Success Modal -->
    <ApiKeySuccessModal
      :show="showSuccessModal"
      :api-key-data="createdApiKeyData"
      @close="showSuccessModal = false; createdApiKeyData = null"
    />
  </div>
</template>

<script setup lang="ts">
import type { ApiKey } from '~/types/api-key'
// Import composables
import { useApiKeys } from '~/composables/useApiKeys'
// Import components explicitly
import SimpleApiKeyForm from '~/components/api-key/SimpleApiKeyForm.vue'
import ApiKeySuccessModal from '~/components/api-key/ApiKeySuccessModal.vue'

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
  createSimpleApiKey,
  revokeApiKey,
  rotateApiKey
} = useApiKeys()

// Reactive state  
const selectedKeyId = ref<string>('')
const createLoading = ref(false)
const showSuccessMessage = ref(false)
const successMessage = ref('')

// Simple API Key creation state
const showSimpleCreateModal = ref(false)
const showSuccessModal = ref(false)
const createdApiKeyData = ref<any>(null)

// Breadcrumb navigation
const breadcrumbs = ref([
  { name: 'Apps', href: '/apps' },
  { name: 'API Keys', href: '/apps/api-keys' }
])

/**
 * Computed properties (Thuộc tính computed)
 */
const filteredApiKeys = computed(() => {
  // Filter out revoked API keys from display (Lọc bỏ API keys đã bị thu hồi)
  return apiKeys.value.filter(apiKey => apiKey.status !== 'revoked')
})

const activeKeysCount = computed(() => {
  return apiKeys.value.filter((key: ApiKey) => key.status === 'active').length
})

/**
 * Event handlers (Xử lý sự kiện)
 */
const handleOpenCreateModal = (): void => {
  console.log('Opening create modal...')
  showSimpleCreateModal.value = true
}
const handleSelectKey = (keyId: string): void => {
  selectedKeyId.value = selectedKeyId.value === keyId ? '' : keyId
}

const handleCopyKeyPrefix = async (apiKey: ApiKey): Promise<void> => {
  try {
    // Copy the API key prefix to clipboard
    const textToCopy = apiKey.keyPrefix
    await navigator.clipboard.writeText(textToCopy)
    
    // Show success message
    showSuccessToast(`API Key prefix "${textToCopy}" copied to clipboard!`)
    
    console.log('Copied API key prefix:', textToCopy)
  } catch (error) {
    console.error('Failed to copy API key prefix:', error)
    
    // Fallback for older browsers
    try {
      const textArea = document.createElement('textarea')
      textArea.value = apiKey.keyPrefix
      textArea.style.position = 'fixed'
      textArea.style.left = '-999999px'
      textArea.style.top = '-999999px'
      document.body.appendChild(textArea)
      textArea.focus()
      textArea.select()
      document.execCommand('copy')
      document.body.removeChild(textArea)
      
      showSuccessToast(`API Key prefix "${apiKey.keyPrefix}" copied to clipboard!`)
    } catch (fallbackError) {
      console.error('Fallback copy failed:', fallbackError)
      // Could show an error toast here
    }
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

// Simple API key creation handler
const handleCreateSimpleApiKey = async (request: import('~/types/api-key').CreateSimpleApiKeyRequest): Promise<void> => {
  try {
    createLoading.value = true
    const response = await createSimpleApiKey(request)
    
    // Store the response for success modal
    createdApiKeyData.value = response
    
    // Close create modal and show success modal
    showSimpleCreateModal.value = false
    showSuccessModal.value = true
    
    // Refresh the list
    await refreshApiKeys()
  } catch (error) {
    console.error('Failed to create simple API key:', error)
    // Show error toast (you can implement toast system)
  } finally {
    createLoading.value = false
  }
}

// Removed complex API key creation handlers - only simple creation now

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
      if (showSimpleCreateModal.value) {
        showSimpleCreateModal.value = false
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