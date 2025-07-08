<template>
  <form @submit.prevent="handleSubmit" class="api-key-form space-y-6">
    <!-- Basic Information Section (Phần thông tin cơ bản) -->
    <div class="form-section">
      <div class="section-header">
        <h3 class="section-title">
          <icon-information-circle class="h-5 w-5" />
          {{ $t('apiKey.form.basicInformation') }}
        </h3>
        <p class="section-description">
          {{ $t('apiKey.form.basicDescription') }}
        </p>
      </div>
      
      <div class="section-content">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Name Field -->
          <div class="form-field">
            <label for="name" class="form-label required">
              {{ $t('apiKey.form.name') }}
            </label>
            <input
              id="name"
              v-model="form.name"
              type="text"
              :placeholder="$t('apiKey.form.namePlaceholder')"
              class="form-input"
              :class="{ 'error': errors.name }"
              required
            />
            <div v-if="errors.name" class="form-error">
              {{ errors.name }}
            </div>
            <div class="form-hint">
              {{ $t('apiKey.form.nameHint') }}
            </div>
          </div>
          
          <!-- Expires At Field -->
          <div class="form-field">
            <label for="expiresAt" class="form-label">
              {{ $t('apiKey.form.expiresAt') }}
            </label>
            <input
              id="expiresAt"
              v-model="form.expiresAt"
              type="datetime-local"
              class="form-input"
              :class="{ 'error': errors.expiresAt }"
              :min="minExpiryDate"
            />
            <div v-if="errors.expiresAt" class="form-error">
              {{ errors.expiresAt }}
            </div>
            <div class="form-hint">
              {{ $t('apiKey.form.expiresAtHint') }}
            </div>
          </div>
        </div>
        
        <!-- Description Field -->
        <div class="form-field">
          <label for="description" class="form-label">
            {{ $t('apiKey.form.description') }}
          </label>
          <textarea
            id="description"
            v-model="form.description"
            rows="3"
            :placeholder="$t('apiKey.form.descriptionPlaceholder')"
            class="form-textarea"
            :class="{ 'error': errors.description }"
          />
          <div v-if="errors.description" class="form-error">
            {{ errors.description }}
          </div>
        </div>
      </div>
    </div>

    <!-- Permissions Section (Phần quyền hạn) -->
    <div class="form-section">
      <div class="section-header">
        <h3 class="section-title">
          <icon-shield-check class="h-5 w-5" />
          {{ $t('apiKey.form.permissions') }}
        </h3>
        <p class="section-description">
          {{ $t('apiKey.form.permissionsDescription') }}
        </p>
      </div>
      
      <div class="section-content">
        <!-- Scope Templates -->
        <div class="form-field">
          <label class="form-label">
            {{ $t('apiKey.form.scopeTemplates') }}
          </label>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
            <button
              v-for="template in scopeTemplates"
              :key="template.id"
              type="button"
              @click="applyScopeTemplate(template)"
              :class="[
                'p-4 border rounded-lg text-left transition-all hover:border-primary',
                selectedTemplate?.id === template.id
                  ? 'border-primary bg-primary/5'
                  : 'border-gray-200 dark:border-gray-700'
              ]"
            >
              <div class="flex items-center space-x-2 mb-2">
                <component :is="template.icon" class="h-5 w-5 text-primary" />
                <span class="font-medium text-gray-900 dark:text-white">
                  {{ template.name }}
                </span>
              </div>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-3">
                {{ template.description }}
              </p>
              <div class="flex flex-wrap gap-1">
                <ScopeBadge
                  v-for="scope in template.scopes.slice(0, 3)"
                  :key="scope"
                  :scope="scope"
                  size="sm"
                />
                <span v-if="template.scopes.length > 3" class="text-xs text-gray-500">
                  +{{ template.scopes.length - 3 }}
                </span>
              </div>
            </button>
          </div>
        </div>
        
        <!-- Custom Scopes -->
        <div class="form-field">
          <label class="form-label">
            {{ $t('apiKey.form.customScopes') }}
          </label>
          <div class="space-y-3">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
              <label
                v-for="scope in availableScopes"
                :key="scope"
                class="flex items-center space-x-3 p-3 border border-gray-200 dark:border-gray-700 rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800"
              >
                <input
                  v-model="form.scopes"
                  type="checkbox"
                  :value="scope"
                  class="form-checkbox"
                />
                <div class="flex-1">
                  <ScopeBadge :scope="scope" size="sm" />
                  <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                    {{ getScopeDescription(scope) }}
                  </p>
                </div>
              </label>
            </div>
            
            <div v-if="errors.scopes" class="form-error">
              {{ errors.scopes }}
            </div>
          </div>
        </div>
        
        <!-- Selected Scopes Summary -->
        <div v-if="form.scopes.length > 0" class="form-field">
          <label class="form-label">
            {{ $t('apiKey.form.selectedScopes') }} ({{ form.scopes.length }})
          </label>
          <div class="flex flex-wrap gap-2 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <ScopeBadge
              v-for="scope in form.scopes"
              :key="scope"
              :scope="scope"
              size="sm"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Rate Limiting Section (Phần giới hạn tốc độ) -->
    <div class="form-section">
      <div class="section-header">
        <h3 class="section-title">
          <icon-clock class="h-5 w-5" />
          {{ $t('apiKey.form.rateLimiting') }}
        </h3>
        <p class="section-description">
          {{ $t('apiKey.form.rateLimitingDescription') }}
        </p>
      </div>
      
      <div class="section-content">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Rate Limit Per Minute -->
          <div class="form-field">
            <label for="rateLimitPerMinute" class="form-label">
              {{ $t('apiKey.form.rateLimitPerMinute') }}
            </label>
            <div class="relative">
              <input
                id="rateLimitPerMinute"
                v-model.number="form.rateLimitPerMinute"
                type="number"
                min="1"
                max="1000"
                class="form-input pr-20"
                :class="{ 'error': errors.rateLimitPerMinute }"
              />
              <span class="absolute right-3 top-1/2 transform -translate-y-1/2 text-sm text-gray-500">
                req/min
              </span>
            </div>
            <div v-if="errors.rateLimitPerMinute" class="form-error">
              {{ errors.rateLimitPerMinute }}
            </div>
            <div class="form-hint">
              {{ $t('apiKey.form.rateLimitHint') }}
            </div>
          </div>
          
          <!-- Daily Usage Quota -->
          <div class="form-field">
            <label for="dailyUsageQuota" class="form-label">
              {{ $t('apiKey.form.dailyUsageQuota') }}
            </label>
            <div class="relative">
              <input
                id="dailyUsageQuota"
                v-model.number="form.dailyUsageQuota"
                type="number"
                min="100"
                max="1000000"
                class="form-input pr-20"
                :class="{ 'error': errors.dailyUsageQuota }"
              />
              <span class="absolute right-3 top-1/2 transform -translate-y-1/2 text-sm text-gray-500">
                req/day
              </span>
            </div>
            <div v-if="errors.dailyUsageQuota" class="form-error">
              {{ errors.dailyUsageQuota }}
            </div>
            <div class="form-hint">
              {{ $t('apiKey.form.dailyQuotaHint') }}
            </div>
          </div>
        </div>
        
        <!-- Rate Limit Suggestions -->
        <div class="p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
          <div class="flex items-start space-x-3">
            <icon-light-bulb class="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
            <div>
              <h4 class="font-medium text-blue-900 dark:text-blue-100">
                {{ $t('apiKey.form.rateLimitSuggestion') }}
              </h4>
              <p class="text-sm text-blue-700 dark:text-blue-300 mt-1">
                {{ getRateLimitSuggestionText() }}
              </p>
              <button
                type="button"
                @click="applyRateLimitSuggestion"
                class="text-sm text-blue-600 dark:text-blue-400 underline hover:no-underline mt-2"
              >
                {{ $t('common.apply') }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Advanced Security Section (Phần bảo mật nâng cao) -->
    <div class="form-section">
      <div class="section-header">
        <button
          type="button"
          @click="showAdvancedSecurity = !showAdvancedSecurity"
          class="flex items-center justify-between w-full text-left"
        >
          <div class="flex items-center space-x-2">
            <icon-shield-exclamation class="h-5 w-5" />
            <h3 class="section-title">
              {{ $t('apiKey.form.advancedSecurity') }}
            </h3>
          </div>
          <icon-chevron-down
            :class="[
              'h-5 w-5 text-gray-400 transition-transform',
              showAdvancedSecurity && 'transform rotate-180'
            ]"
          />
        </button>
        <p class="section-description">
          {{ $t('apiKey.form.advancedSecurityDescription') }}
        </p>
      </div>
      
      <div v-show="showAdvancedSecurity" class="section-content">
        <!-- IP Whitelist -->
        <div class="form-field">
          <label for="ipWhitelist" class="form-label">
            {{ $t('apiKey.form.ipWhitelist') }}
          </label>
          <textarea
            id="ipWhitelist"
            v-model="ipWhitelistText"
            rows="4"
            :placeholder="$t('apiKey.form.ipWhitelistPlaceholder')"
            class="form-textarea font-mono text-sm"
            :class="{ 'error': errors.ipWhitelist }"
          />
          <div v-if="errors.ipWhitelist" class="form-error">
            {{ errors.ipWhitelist }}
          </div>
          <div class="form-hint">
            {{ $t('apiKey.form.ipWhitelistHint') }}
          </div>
        </div>
        
        <!-- Security Options -->
        <div class="form-field">
          <label class="form-label">
            {{ $t('apiKey.form.securityOptions') }}
          </label>
          <div class="space-y-4">
            <label class="flex items-start space-x-3 cursor-pointer">
              <input
                v-model="form.securitySettings.requireHttps"
                type="checkbox"
                class="form-checkbox mt-1"
              />
              <div>
                <span class="font-medium text-gray-900 dark:text-white">
                  {{ $t('apiKey.form.requireHttps') }}
                </span>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ $t('apiKey.form.requireHttpsDescription') }}
                </p>
              </div>
            </label>
            
            <label class="flex items-start space-x-3 cursor-pointer">
              <input
                v-model="form.securitySettings.enableIpValidation"
                type="checkbox"
                class="form-checkbox mt-1"
              />
              <div>
                <span class="font-medium text-gray-900 dark:text-white">
                  {{ $t('apiKey.form.enableIpValidation') }}
                </span>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ $t('apiKey.form.enableIpValidationDescription') }}
                </p>
              </div>
            </label>
            
            <label class="flex items-start space-x-3 cursor-pointer">
              <input
                v-model="form.securitySettings.enableRateLimiting"
                type="checkbox"
                class="form-checkbox mt-1"
              />
              <div>
                <span class="font-medium text-gray-900 dark:text-white">
                  {{ $t('apiKey.form.enableRateLimiting') }}
                </span>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ $t('apiKey.form.enableRateLimitingDescription') }}
                </p>
              </div>
            </label>
            
            <label class="flex items-start space-x-3 cursor-pointer">
              <input
                v-model="form.securitySettings.enableUsageAnalytics"
                type="checkbox"
                class="form-checkbox mt-1"
              />
              <div>
                <span class="font-medium text-gray-900 dark:text-white">
                  {{ $t('apiKey.form.enableUsageAnalytics') }}
                </span>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ $t('apiKey.form.enableUsageAnalyticsDescription') }}
                </p>
              </div>
            </label>
          </div>
        </div>
      </div>
    </div>

    <!-- Security Score Section (Phần điểm bảo mật) -->
    <div class="form-section">
      <div class="section-header">
        <h3 class="section-title">
          <icon-chart-bar class="h-5 w-5" />
          {{ $t('apiKey.form.securityScore') }}
        </h3>
        <p class="section-description">
          {{ $t('apiKey.form.securityScoreDescription') }}
        </p>
      </div>
      
      <div class="section-content">
        <SecurityScore
          :score="securityScore"
          :warnings="securityWarnings"
          :security-settings="form.securitySettings"
          :scopes="form.scopes"
          :has-expiration="!!form.expiresAt"
          variant="detailed"
          show-breakdown
          show-suggestions
        />
      </div>
    </div>

    <!-- Form Actions (Hành động form) -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 pt-6 border-t border-gray-200 dark:border-gray-700">
      <div class="flex items-center space-x-4">
        <button
          type="button"
          @click="$emit('cancel')"
          class="btn-secondary"
          :disabled="loading"
        >
          {{ $t('common.cancel') }}
        </button>
        
        <button
          type="button"
          @click="resetForm"
          class="btn-secondary"
          :disabled="loading"
        >
          {{ $t('common.reset') }}
        </button>
      </div>
      
      <div class="flex items-center space-x-4">
        <div v-if="loading" class="flex items-center space-x-2 text-sm text-gray-600 dark:text-gray-400">
          <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-primary"></div>
          <span>{{ isEditMode ? $t('apiKey.form.updating') : $t('apiKey.form.creating') }}...</span>
        </div>
        
        <button
          type="submit"
          class="btn-primary"
          :disabled="loading || !isFormValid"
        >
          <icon-plus v-if="!isEditMode" class="h-4 w-4 mr-2" />
          <icon-pencil v-else class="h-4 w-4 mr-2" />
          {{ isEditMode ? $t('common.update') : $t('common.create') }}
        </button>
      </div>
    </div>
  </form>
</template>

<script setup lang="ts">
import type { 
  CreateApiKeyRequest, 
  UpdateApiKeyRequest, 
  ApiKey,
  ScopeTemplate 
} from '~/types/api-key'
import { 
  DEFAULT_API_KEY_REQUEST, 
  SCOPE_TEMPLATES,
  ApiKeyScope 
} from '~/types/api-key'

interface Props {
  apiKey?: ApiKey
  loading?: boolean
}

interface Emits {
  (e: 'submit', data: CreateApiKeyRequest | UpdateApiKeyRequest): void
  (e: 'cancel'): void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<Emits>()

/**
 * Form state và validation (Trạng thái form và xác thực)
 */
const { 
  formState, 
  validateForm, 
  calculateSecurityScore,
  getScopeTemplates,
  applyScopeTemplate: applyTemplate,
  validateIpAddress,
  calculateRateLimitSuggestion
} = useApiKeyManager()

const form = ref({ ...DEFAULT_API_KEY_REQUEST })
const errors = ref<Record<string, string>>({})
const showAdvancedSecurity = ref(false)
const selectedTemplate = ref<ScopeTemplate | null>(null)

/**
 * Computed properties (Thuộc tính computed)
 */
const isEditMode = computed(() => !!props.apiKey)

const minExpiryDate = computed(() => {
  const date = new Date()
  date.setMinutes(date.getMinutes() + 30) // Minimum 30 minutes from now
  return date.toISOString().slice(0, 16)
})

const scopeTemplates = computed(() => SCOPE_TEMPLATES)

const availableScopes = computed(() => Object.values(ApiKeyScope))

const ipWhitelistText = computed({
  get: () => form.value.ipWhitelist.join('\n'),
  set: (value: string) => {
    form.value.ipWhitelist = value
      .split('\n')
      .map(ip => ip.trim())
      .filter(ip => ip.length > 0)
  }
})

const securityScore = computed(() => {
  return calculateSecurityScore(form.value)
})

const securityWarnings = computed(() => {
  const warnings: string[] = []
  
  if (!form.value.securitySettings.requireHttps) {
    warnings.push('HTTPS not required - consider enabling for better security')
  }
  
  if (!form.value.securitySettings.enableIpValidation || form.value.ipWhitelist.length === 0) {
    warnings.push('No IP restrictions - consider adding IP whitelist')
  }
  
  if (!form.value.securitySettings.enableRateLimiting) {
    warnings.push('Rate limiting disabled - this could allow abuse')
  }
  
  if (!form.value.expiresAt) {
    warnings.push('No expiration date - consider setting an expiry for better security')
  }
  
  if (form.value.scopes.includes(ApiKeyScope.Admin)) {
    warnings.push('Admin scope granted - ensure this is necessary')
  }
  
  if (!form.value.securitySettings.enableUsageAnalytics) {
    warnings.push('Usage analytics disabled - you won\'t be able to monitor key usage')
  }
  
  return warnings
})

const isFormValid = computed(() => {
  return form.value.name.trim().length >= 3 && 
         form.value.scopes.length > 0 &&
         Object.keys(errors.value).length === 0
})

/**
 * Initialize form with API key data if editing (Khởi tạo form với dữ liệu API key nếu đang chỉnh sửa)
 */
const initializeForm = (): void => {
  if (props.apiKey) {
    form.value = {
      name: props.apiKey.name,
      description: props.apiKey.description || '',
      scopes: [...props.apiKey.scopes],
      expiresAt: props.apiKey.expiresAt,
      rateLimitPerMinute: props.apiKey.rateLimitPerMinute,
      dailyUsageQuota: props.apiKey.dailyUsageQuota,
      ipWhitelist: [...props.apiKey.ipWhitelist],
      securitySettings: { ...props.apiKey.securitySettings }
    }
  } else {
    form.value = { ...DEFAULT_API_KEY_REQUEST }
  }
  errors.value = {}
}

/**
 * Apply scope template (Áp dụng mẫu scope)
 */
const applyScopeTemplate = (template: ScopeTemplate): void => {
  form.value.scopes = [...template.scopes]
  selectedTemplate.value = template
  
  // Apply suggested rate limits based on template
  const suggestedRateLimit = calculateRateLimitSuggestion(template.scopes)
  form.value.rateLimitPerMinute = suggestedRateLimit
  form.value.dailyUsageQuota = suggestedRateLimit * 60 * 24 // 24 hours worth
}

/**
 * Get scope description (Lấy mô tả scope)
 */
const getScopeDescription = (scope: string): string => {
  // This would typically come from i18n translations
  const descriptions: Record<string, string> = {
    [ApiKeyScope.Read]: 'Read access to resources',
    [ApiKeyScope.Write]: 'Write access to resources',
    [ApiKeyScope.AccountsRead]: 'Read access to accounts',
    [ApiKeyScope.AccountsWrite]: 'Write access to accounts',
    [ApiKeyScope.TransactionsRead]: 'Read access to transactions',
    [ApiKeyScope.TransactionsWrite]: 'Write access to transactions',
    [ApiKeyScope.Admin]: 'Full administrative access'
  }
  return descriptions[scope] || 'Custom scope'
}

/**
 * Get rate limit suggestion text (Lấy văn bản gợi ý giới hạn tốc độ)
 */
const getRateLimitSuggestionText = (): string => {
  const suggestion = calculateRateLimitSuggestion(form.value.scopes)
  return `Based on your selected scopes, we recommend ${suggestion} requests per minute`
}

/**
 * Apply rate limit suggestion (Áp dụng gợi ý giới hạn tốc độ)
 */
const applyRateLimitSuggestion = (): void => {
  const suggestion = calculateRateLimitSuggestion(form.value.scopes)
  form.value.rateLimitPerMinute = suggestion
  form.value.dailyUsageQuota = suggestion * 60 * 24
}

/**
 * Validate form data (Xác thực dữ liệu form)
 */
const validateFormData = (): boolean => {
  const newErrors: Record<string, string> = {}
  
  // Name validation
  if (!form.value.name.trim()) {
    newErrors.name = 'API key name is required'
  } else if (form.value.name.length < 3) {
    newErrors.name = 'Name must be at least 3 characters'
  } else if (form.value.name.length > 100) {
    newErrors.name = 'Name must be less than 100 characters'
  }
  
  // Description validation
  if (form.value.description && form.value.description.length > 500) {
    newErrors.description = 'Description must be less than 500 characters'
  }
  
  // Scopes validation
  if (form.value.scopes.length === 0) {
    newErrors.scopes = 'At least one scope must be selected'
  }
  
  // Rate limiting validation
  if (form.value.rateLimitPerMinute < 1 || form.value.rateLimitPerMinute > 1000) {
    newErrors.rateLimitPerMinute = 'Rate limit must be between 1 and 1000 requests per minute'
  }
  
  if (form.value.dailyUsageQuota < 100) {
    newErrors.dailyUsageQuota = 'Daily quota must be at least 100 requests'
  }
  
  // IP whitelist validation
  if (form.value.securitySettings.enableIpValidation && form.value.ipWhitelist.length > 0) {
    const invalidIps = form.value.ipWhitelist.filter(ip => !validateIpAddress(ip))
    if (invalidIps.length > 0) {
      newErrors.ipWhitelist = `Invalid IP addresses: ${invalidIps.join(', ')}`
    }
  }
  
  // Expiration validation
  if (form.value.expiresAt) {
    const expiryDate = new Date(form.value.expiresAt)
    const now = new Date()
    if (expiryDate <= now) {
      newErrors.expiresAt = 'Expiration date must be in the future'
    }
  }
  
  errors.value = newErrors
  return Object.keys(newErrors).length === 0
}

/**
 * Handle form submission (Xử lý gửi form)
 */
const handleSubmit = (): void => {
  if (!validateFormData()) return
  
  if (isEditMode.value) {
    const updateRequest: UpdateApiKeyRequest = {
      name: form.value.name,
      description: form.value.description,
      scopes: form.value.scopes,
      expiresAt: form.value.expiresAt,
      rateLimitPerMinute: form.value.rateLimitPerMinute,
      dailyUsageQuota: form.value.dailyUsageQuota,
      ipWhitelist: form.value.ipWhitelist,
      securitySettings: form.value.securitySettings
    }
    emit('submit', updateRequest)
  } else {
    emit('submit', form.value)
  }
}

/**
 * Reset form to initial state (Đặt lại form về trạng thái ban đầu)
 */
const resetForm = (): void => {
  initializeForm()
  selectedTemplate.value = null
}

/**
 * Watch for prop changes (Theo dõi thay đổi props)
 */
watch(() => props.apiKey, () => {
  initializeForm()
}, { immediate: true })

/**
 * Initialize form on mount (Khởi tạo form khi mount)
 */
onMounted(() => {
  initializeForm()
})
</script>

<style scoped>
.api-key-form {
  @apply max-w-4xl;
}

.form-section {
  @apply bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden;
}

.section-header {
  @apply px-6 py-4 border-b border-gray-200 dark:border-gray-700;
}

.section-title {
  @apply flex items-center space-x-2 text-lg font-semibold text-gray-900 dark:text-white;
}

.section-description {
  @apply mt-2 text-sm text-gray-600 dark:text-gray-400;
}

.section-content {
  @apply p-6 space-y-6;
}

.form-field {
  @apply space-y-2;
}

.form-label {
  @apply block text-sm font-medium text-gray-700 dark:text-gray-300;
}

.form-label.required::after {
  content: ' *';
  @apply text-red-500;
}

.form-input,
.form-textarea,
.form-select {
  @apply block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg shadow-sm placeholder-gray-400 dark:placeholder-gray-500 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent;
}

.form-input.error,
.form-textarea.error,
.form-select.error {
  @apply border-red-500 focus:ring-red-500;
}

.form-checkbox {
  @apply rounded border-gray-300 dark:border-gray-600 text-primary focus:ring-primary dark:bg-gray-800;
}

.form-error {
  @apply text-sm text-red-600 dark:text-red-400;
}

.form-hint {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

/* Responsive adjustments (Điều chỉnh responsive) */
@media (max-width: 768px) {
  .section-content {
    @apply p-4 space-y-4;
  }
  
  .section-header {
    @apply px-4 py-3;
  }
}
</style> 