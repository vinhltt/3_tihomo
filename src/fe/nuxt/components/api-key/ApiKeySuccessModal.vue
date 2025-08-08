<template>
  <!-- Success Modal -->
  <Teleport to="body">
    <div
      v-if="show"
      class="fixed inset-0 z-[9999] overflow-y-auto"
      @click.self="handleClose"
    >
      <div class="flex min-h-screen items-center justify-center p-4">
        <!-- Backdrop -->
        <div class="fixed inset-0 bg-black/50 transition-opacity"></div>
        
        <!-- Modal -->
        <div class="relative bg-white dark:bg-gray-800 rounded-xl shadow-xl w-full max-w-2xl mx-4 p-6">
          <!-- Header -->
          <div class="flex items-center justify-between mb-6">
            <div class="flex items-center">
              <div class="w-12 h-12 bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center mr-4">
                <icon-checks class="w-6 h-6 text-green-600 dark:text-green-400" />
              </div>
              <div>
                <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
                  API Key Created Successfully!
                </h3>
                <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Your API key has been generated and is ready to use.
                </p>
              </div>
            </div>
            <button
              @click="handleClose"
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              <icon-x class="w-6 h-6" />
            </button>
          </div>

          <!-- API Key Display -->
          <div class="space-y-4">
            <!-- Key Name -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                API Key Name
              </label>
              <div class="px-3 py-2 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <span class="text-gray-900 dark:text-white font-medium">{{ apiKeyData?.name }}</span>
              </div>
            </div>

            <!-- API Key -->
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                API Key <span class="text-red-500 text-xs">(Copy now - shown only once!)</span>
              </label>
              <div class="relative">
                <div class="px-3 py-2 bg-gray-50 dark:bg-gray-700 rounded-lg border-2 border-dashed border-yellow-300 dark:border-yellow-600">
                  <code class="text-sm font-mono text-gray-900 dark:text-white break-all">
                    {{ apiKeyData?.apiKey }}
                  </code>
                </div>
                <button
                  @click="copyToClipboard"
                  class="absolute right-2 top-2 p-1 text-gray-500 hover:text-gray-700 dark:hover:text-gray-300"
                  :title="copied ? 'Copied!' : 'Copy to clipboard'"
                >
                  <icon-clipboard-text v-if="!copied" class="w-4 h-4" />
                  <icon-checks v-else class="w-4 h-4 text-green-600" />
                </button>
              </div>
            </div>

            <!-- Key Details -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Key Prefix
                </label>
                <div class="text-sm text-gray-600 dark:text-gray-400">
                  {{ apiKeyData?.keyPrefix }}
                </div>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Created At
                </label>
                <div class="text-sm text-gray-600 dark:text-gray-400">
                  {{ formattedCreatedAt }}
                </div>
              </div>
            </div>
          </div>

          <!-- Warning Box -->
          <div class="mt-6 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg p-4">
            <div class="flex items-start">
              <icon-info-triangle class="h-5 w-5 text-yellow-600 dark:text-yellow-400 mt-0.5 mr-3 flex-shrink-0" />
              <div class="text-sm">
                <h6 class="font-medium text-yellow-900 dark:text-yellow-200 mb-1">Important Security Notice</h6>
                <ul class="text-yellow-700 dark:text-yellow-300 space-y-1 text-xs">
                  <li>• <strong>Copy this API key now</strong> - it will not be shown again for security reasons</li>
                  <li>• Store it securely in your application or password manager</li>
                  <li>• Never share your API key or commit it to version control</li>
                  <li>• If you lose it, you'll need to generate a new one</li>
                </ul>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-center justify-between mt-6 pt-4 border-t border-gray-200 dark:border-gray-700">
            <div class="text-sm text-gray-500 dark:text-gray-400">
              You can manage this API key in your dashboard
            </div>
            <div class="flex items-center space-x-3">
              <button
                @click="copyToClipboard"
                class="btn btn-outline-primary btn-sm"
              >
                <icon-clipboard-text class="w-4 h-4 mr-1" />
                {{ copied ? 'Copied!' : 'Copy Key' }}
              </button>
              <button
                @click="handleClose"
                class="btn btn-primary btn-sm"
              >
                Done
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import type { CreateApiKeyResponse } from '~/types/api-key'

// Props
interface Props {
  show: boolean
  apiKeyData: CreateApiKeyResponse | null
}

// Emits
interface Emits {
  (e: 'close'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// State
const copied = ref(false)

// Computed
const formattedCreatedAt = computed(() => {
  if (!props.apiKeyData?.createdAt) return ''
  return new Date(props.apiKeyData.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
})

// Methods
const copyToClipboard = async () => {
  if (!props.apiKeyData?.apiKey) return
  
  try {
    await navigator.clipboard.writeText(props.apiKeyData.apiKey)
    copied.value = true
    
    // Reset copied state after 2 seconds
    setTimeout(() => {
      copied.value = false
    }, 2000)
    
    // Show success toast
    // You can replace this with your toast system
    console.log('API key copied to clipboard!')
  } catch (err) {
    console.error('Failed to copy to clipboard:', err)
    // Fallback for older browsers
    fallbackCopyTextToClipboard(props.apiKeyData.apiKey)
  }
}

const fallbackCopyTextToClipboard = (text: string) => {
  const textArea = document.createElement('textarea')
  textArea.value = text
  textArea.style.top = '0'
  textArea.style.left = '0'
  textArea.style.position = 'fixed'
  
  document.body.appendChild(textArea)
  textArea.focus()
  textArea.select()
  
  try {
    const successful = document.execCommand('copy')
    if (successful) {
      copied.value = true
      setTimeout(() => { copied.value = false }, 2000)
    }
  } catch (err) {
    console.error('Fallback: Oops, unable to copy', err)
  }
  
  document.body.removeChild(textArea)
}

const handleClose = () => {
  copied.value = false
  emit('close')
}

// Handle ESC key
const handleEscKey = (event: KeyboardEvent) => {
  if (event.key === 'Escape' && props.show) {
    handleClose()
  }
}

// Lifecycle
onMounted(() => {
  document.addEventListener('keydown', handleEscKey)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleEscKey)
})

// Reset copied state when modal is hidden
watch(() => props.show, (newShow) => {
  if (!newShow) {
    copied.value = false
  }
})
</script>