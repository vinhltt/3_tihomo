<template>
  <div class="panel">
    <div class="mb-5">
      <h5 class="font-semibold text-lg mb-2">Create API Key</h5>
      <p class="text-sm text-gray-500 dark:text-gray-400">
        Create an API key to access TiHoMo services programmatically.
      </p>
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- API Key Name -->
      <div>
        <label for="apiKeyName" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          API Key Name <span class="text-red-500">*</span>
        </label>
        <input
          id="apiKeyName"
          v-model="form.name"
          type="text"
          placeholder="Enter a descriptive name for your API key"
          class="form-input"
          :class="{ 'border-red-500': errors.name }"
          required
          maxlength="100"
          :disabled="loading"
        />
        <p v-if="errors.name" class="text-red-500 text-xs mt-1">{{ errors.name }}</p>
        <p class="text-xs text-gray-500 mt-1">
          Choose a name that helps you identify what this API key is used for.
        </p>
      </div>

      <!-- Default Settings Info -->
      <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
        <div class="flex items-start">
          <icon-info-circle class="h-5 w-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-3 flex-shrink-0" />
          <div class="text-sm">
            <h6 class="font-medium text-blue-900 dark:text-blue-200 mb-1">Default Settings</h6>
            <p class="text-blue-700 dark:text-blue-300 mb-2">Your API key will be created with these default settings:</p>
            <ul class="list-disc list-inside text-blue-600 dark:text-blue-400 space-y-1 text-xs">
              <li><strong>Access:</strong> Full read and write permissions</li>
              <li><strong>Rate Limit:</strong> 50 requests per minute</li>
              <li><strong>Daily Quota:</strong> 500 requests per day</li>
              <li><strong>Security:</strong> HTTPS required, usage analytics enabled</li>
              <li><strong>Expiration:</strong> Never expires</li>
            </ul>
            <p class="text-blue-600 dark:text-blue-400 text-xs mt-2">
              Advanced settings can be configured later if needed.
            </p>
          </div>
        </div>
      </div>

      <!-- Submit Button -->
      <div class="flex items-center justify-between pt-4">
        <button
          type="button"
          @click="$emit('cancel')"
          class="btn btn-outline-primary"
          :disabled="loading"
        >
          Cancel
        </button>
        
        <button
          type="submit"
          class="btn btn-primary"
          :disabled="loading || !form.name.trim()"
        >
          <icon-lock v-if="!loading" class="w-4 h-4 mr-2" />
          <div v-else class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
          {{ loading ? 'Creating...' : 'Create API Key' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import type { CreateSimpleApiKeyRequest } from '~/types/api-key'

// Props & Emits
interface Props {
  loading?: boolean
}

interface Emits {
  (e: 'submit', data: CreateSimpleApiKeyRequest): void
  (e: 'cancel'): void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<Emits>()

// Form state
const form = reactive<CreateSimpleApiKeyRequest>({
  name: ''
})

const errors = reactive({
  name: ''
})

// Methods
const validateForm = (): boolean => {
  errors.name = ''
  
  if (!form.name.trim()) {
    errors.name = 'API key name is required'
    return false
  }
  
  if (form.name.length < 3) {
    errors.name = 'API key name must be at least 3 characters'
    return false
  }
  
  if (form.name.length > 100) {
    errors.name = 'API key name must not exceed 100 characters'
    return false
  }
  
  return true
}

const handleSubmit = () => {
  if (!validateForm()) return
  
  emit('submit', { ...form })
}

// Reset form when component is created/reset
const resetForm = () => {
  form.name = ''
  errors.name = ''
}

// Debug
onMounted(() => {
  console.log('SimpleApiKeyForm mounted')
})

// Expose reset method
defineExpose({
  resetForm
})
</script>

<style scoped>
.form-input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}
</style>