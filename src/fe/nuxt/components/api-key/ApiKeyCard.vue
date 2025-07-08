<template>
  <div :class="cardClasses">
    <!-- Card Header (Tiêu đề thẻ) -->
    <div class="card-header">
      <div class="flex items-start justify-between">
        <div class="flex-1 min-w-0">
          <div class="flex items-center space-x-3">
            <div class="flex-shrink-0">
              <icon-key :class="keyIconClasses" />
            </div>
            <div class="flex-1 min-w-0">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white truncate">
                {{ apiKey.name }}
              </h3>
              <p v-if="apiKey.description" class="mt-1 text-sm text-gray-600 dark:text-gray-400 line-clamp-2">
                {{ apiKey.description }}
              </p>
            </div>
          </div>
        </div>
        
        <div class="flex items-center space-x-2">
          <StatusBadge :status="apiKey.status" :expires-at="apiKey.expiresAt" />
          
          <div class="dropdown">
            <Popper :placement="'bottom-end'">
              <button class="action-menu-trigger">
                <icon-ellipsis-vertical class="h-5 w-5" />
              </button>
              <template #content="{ close }">
                <div class="action-menu">
                  <button
                    @click="handleEdit(); close()"
                    class="action-menu-item"
                    :disabled="apiKey.status === 'revoked'"
                  >
                    <icon-pencil class="h-4 w-4" />
                    {{ $t('common.edit') }}
                  </button>
                  
                  <button
                    @click="handleRegenerate(); close()"
                    class="action-menu-item"
                    :disabled="apiKey.status === 'revoked'"
                  >
                    <icon-arrow-path class="h-4 w-4" />
                    {{ $t('apiKey.actions.regenerate') }}
                  </button>
                  
                  <button
                    @click="handleCopyKey(); close()"
                    class="action-menu-item"
                  >
                    <icon-clipboard class="h-4 w-4" />
                    {{ $t('apiKey.actions.copyKey') }}
                  </button>
                  
                  <div class="action-menu-divider"></div>
                  
                  <button
                    @click="handleRevoke(); close()"
                    class="action-menu-item text-danger"
                    :disabled="apiKey.status === 'revoked'"
                  >
                    <icon-trash class="h-4 w-4" />
                    {{ $t('apiKey.actions.revoke') }}
                  </button>
                </div>
              </template>
            </Popper>
          </div>
        </div>
      </div>
    </div>

    <!-- Key Information (Thông tin khóa) -->
    <div class="card-content">
      <!-- Key Display Section -->
      <div class="key-display-section">
        <div class="flex items-center justify-between mb-2">
          <label class="text-sm font-medium text-gray-700 dark:text-gray-300">
            {{ $t('apiKey.card.apiKey') }}
          </label>
          <button
            @click="toggleKeyVisibility"
            class="text-sm text-primary hover:text-primary-dark"
          >
            {{ showFullKey ? $t('common.hide') : $t('common.show') }}
          </button>
        </div>
        
        <div class="key-display">
          <code class="key-value">
            {{ showFullKey ? (fullKey || `${apiKey.keyPrefix}${'*'.repeat(32)}`) : `${apiKey.keyPrefix}${'*'.repeat(28)}` }}
          </code>
          <button
            @click="copyToClipboard(showFullKey ? fullKey || apiKey.keyPrefix : apiKey.keyPrefix)"
            class="copy-button"
            :title="$t('common.copy')"
          >
            <icon-clipboard class="h-4 w-4" />
          </button>
        </div>
        
        <div v-if="showFullKey && !fullKey" class="mt-2 p-3 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
          <div class="flex items-start space-x-3">
            <icon-exclamation-triangle class="h-5 w-5 text-yellow-600 mt-0.5 flex-shrink-0" />
            <div class="text-sm text-yellow-700 dark:text-yellow-300">
              <p class="font-medium">{{ $t('apiKey.card.keyNotAvailable') }}</p>
              <p>{{ $t('apiKey.card.keyNotAvailableDescription') }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Stats Grid (Lưới thống kê) -->
      <div class="stats-grid">
        <!-- Usage Statistics -->
        <div class="stat-item">
          <div class="stat-label">
            {{ $t('apiKey.card.todayUsage') }}
          </div>
          <div class="stat-value">
            {{ formatNumber(apiKey.todayUsageCount) }} / {{ formatNumber(apiKey.dailyUsageQuota) }}
          </div>
          <div class="stat-progress">
            <div class="progress-bar">
              <div 
                class="progress-fill"
                :class="getUsageProgressColor(apiKey.todayUsageCount, apiKey.dailyUsageQuota)"
                :style="{ width: getUsagePercentage(apiKey.todayUsageCount, apiKey.dailyUsageQuota) + '%' }"
              />
            </div>
          </div>
        </div>
        
        <!-- Rate Limit -->
        <div class="stat-item">
          <div class="stat-label">
            {{ $t('apiKey.card.rateLimit') }}
          </div>
          <div class="stat-value">
            {{ formatNumber(apiKey.rateLimitPerMinute) }} {{ $t('apiKey.card.perMinute') }}
          </div>
        </div>
        
        <!-- Total Usage -->
        <div class="stat-item">
          <div class="stat-label">
            {{ $t('apiKey.card.totalUsage') }}
          </div>
          <div class="stat-value">
            {{ formatNumber(apiKey.usageCount) }} {{ $t('apiKey.card.requests') }}
          </div>
        </div>
        
        <!-- Last Used -->
        <div class="stat-item">
          <div class="stat-label">
            {{ $t('apiKey.card.lastUsed') }}
          </div>
          <div class="stat-value">
            {{ apiKey.lastUsedAt ? formatRelativeTime(apiKey.lastUsedAt) : $t('common.never') }}
          </div>
        </div>
      </div>

      <!-- Scopes Section (Phần phạm vi) -->
      <div class="scopes-section">
        <div class="flex items-center justify-between mb-3">
          <h4 class="section-subtitle">
            {{ $t('apiKey.card.permissions') }}
          </h4>
          <span class="text-sm text-gray-500 dark:text-gray-400">
            {{ apiKey.scopes.length }} {{ $t('apiKey.card.scopes') }}
          </span>
        </div>
        
        <div class="scopes-container">
          <div class="scopes-list">
            <ScopeBadge
              v-for="scope in visibleScopes"
              :key="scope"
              :scope="scope"
              size="sm"
            />
          </div>
          
          <button
            v-if="apiKey.scopes.length > maxVisibleScopes"
            @click="showAllScopes = !showAllScopes"
            class="scope-toggle-button"
          >
            {{ showAllScopes 
              ? $t('common.showLess') 
              : $t('common.showMore', { count: apiKey.scopes.length - maxVisibleScopes }) 
            }}
          </button>
        </div>
      </div>

      <!-- Security Information (Thông tin bảo mật) -->
      <div v-if="showSecurityInfo" class="security-section">
        <div class="flex items-center justify-between mb-3">
          <h4 class="section-subtitle">
            {{ $t('apiKey.card.security') }}
          </h4>
          <button
            @click="showSecurityDetails = !showSecurityDetails"
            class="text-sm text-primary hover:text-primary/80"
          >
            {{ showSecurityDetails ? $t('common.hide') : $t('common.showDetails') }}
          </button>
        </div>
        
        <div v-if="showSecurityDetails" class="security-details">
          <!-- Security Score -->
          <SecurityScore
            :score="securityScore"
            :security-settings="apiKey.securitySettings"
            :scopes="apiKey.scopes"
            :has-expiration="!!apiKey.expiresAt"
            variant="compact"
            size="sm"
            :show-header="false"
          />
          
          <!-- Security Settings List -->
          <div class="mt-4 space-y-2">
            <div class="security-setting">
              <div class="flex items-center space-x-2">
                <icon-shield-check 
                  :class="apiKey.securitySettings.requireHttps ? 'text-success' : 'text-gray-400'" 
                  class="h-4 w-4" 
                />
                <span class="text-sm text-gray-700 dark:text-gray-300">
                  {{ $t('apiKey.security.httpsRequired') }}
                </span>
              </div>
              <span :class="apiKey.securitySettings.requireHttps ? 'text-success' : 'text-gray-500'" class="text-sm">
                {{ apiKey.securitySettings.requireHttps ? $t('common.enabled') : $t('common.disabled') }}
              </span>
            </div>
            
            <div class="security-setting">
              <div class="flex items-center space-x-2">
                <icon-globe-alt 
                  :class="apiKey.securitySettings.enableIpValidation ? 'text-success' : 'text-gray-400'" 
                  class="h-4 w-4" 
                />
                <span class="text-sm text-gray-700 dark:text-gray-300">
                  {{ $t('apiKey.security.ipValidation') }}
                </span>
              </div>
              <span :class="apiKey.securitySettings.enableIpValidation ? 'text-success' : 'text-gray-500'" class="text-sm">
                {{ apiKey.securitySettings.enableIpValidation 
                  ? $t('common.enabled') + ` (${apiKey.ipWhitelist.length})` 
                  : $t('common.disabled') 
                }}
              </span>
            </div>
            
            <div class="security-setting">
              <div class="flex items-center space-x-2">
                <icon-clock 
                  :class="apiKey.securitySettings.enableRateLimiting ? 'text-success' : 'text-gray-400'" 
                  class="h-4 w-4" 
                />
                <span class="text-sm text-gray-700 dark:text-gray-300">
                  {{ $t('apiKey.security.rateLimiting') }}
                </span>
              </div>
              <span :class="apiKey.securitySettings.enableRateLimiting ? 'text-success' : 'text-gray-500'" class="text-sm">
                {{ apiKey.securitySettings.enableRateLimiting ? $t('common.enabled') : $t('common.disabled') }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Metadata Section (Phần metadata) -->
      <div class="metadata-section">
        <div class="metadata-grid">
          <div class="metadata-item">
            <span class="metadata-label">{{ $t('apiKey.card.created') }}</span>
            <span class="metadata-value" :title="formatFullDate(apiKey.createdAt)">
              {{ formatRelativeTime(apiKey.createdAt) }}
            </span>
          </div>
          
          <div v-if="apiKey.expiresAt" class="metadata-item">
            <span class="metadata-label">{{ $t('apiKey.card.expires') }}</span>
            <span 
              class="metadata-value"
              :class="getExpiryStatusClasses(apiKey.expiresAt)"
              :title="formatFullDate(apiKey.expiresAt)"
            >
              {{ formatRelativeTime(apiKey.expiresAt) }}
            </span>
          </div>
          
          <div class="metadata-item">
            <span class="metadata-label">{{ $t('apiKey.card.keyId') }}</span>
            <span class="metadata-value font-mono text-xs">
              {{ apiKey.id.substring(0, 8) }}...
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Card Footer Actions (Hành động footer thẻ) -->
    <div v-if="showActions" class="card-footer">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-4">
          <button
            @click="$emit('view-usage')"
            class="footer-action-button"
          >
            <icon-chart-bar class="h-4 w-4" />
            {{ $t('apiKey.actions.viewUsage') }}
          </button>
          
          <button
            v-if="apiKey.securitySettings.enableUsageAnalytics"
            @click="$emit('view-analytics')"
            class="footer-action-button"
          >
            <icon-presentation-chart-line class="h-4 w-4" />
            {{ $t('apiKey.actions.analytics') }}
          </button>
        </div>
        
        <div class="flex items-center space-x-2">
          <button
            @click="handleEdit"
            class="footer-action-button-primary"
            :disabled="apiKey.status === 'revoked'"
          >
            <icon-pencil class="h-4 w-4" />
            {{ $t('common.edit') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ApiKey } from '~/types/api-key'

interface Props {
  apiKey: ApiKey
  fullKey?: string
  variant?: 'default' | 'compact' | 'detailed'
  showActions?: boolean
  showSecurityInfo?: boolean
  clickable?: boolean
  selected?: boolean
}

interface Emits {
  (e: 'click'): void
  (e: 'edit'): void
  (e: 'revoke'): void
  (e: 'regenerate'): void
  (e: 'copy-key'): void
  (e: 'view-usage'): void
  (e: 'view-analytics'): void
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  showActions: true,
  showSecurityInfo: true,
  clickable: false,
  selected: false
})

const emit = defineEmits<Emits>()

/**
 * Reactive state (Trạng thái reactive)
 */
const showFullKey = ref(false)
const showAllScopes = ref(false)
const showSecurityDetails = ref(false)
const maxVisibleScopes = ref(6)

/**
 * Computed properties (Thuộc tính computed)
 */
const cardClasses = computed(() => [
  'api-key-card',
  'panel',
  {
    'compact': props.variant === 'compact',
    'detailed': props.variant === 'detailed',
    'clickable': props.clickable,
    'selected': props.selected,
    'revoked': props.apiKey.status === 'revoked'
  }
])

const keyIconClasses = computed(() => {
  const isExpiring = props.apiKey.expiresAt && props.apiKey.status === 'active' && 
    new Date(props.apiKey.expiresAt) <= new Date(Date.now() + 7 * 24 * 60 * 60 * 1000) // 7 days

  return [
    'h-6 w-6',
    {
      'text-success': props.apiKey.status === 'active' && !isExpiring,
      'text-warning': isExpiring,
      'text-danger': props.apiKey.status === 'revoked' || props.apiKey.status === 'expired',
      'text-gray-400': false // No inactive status in ApiKeyStatus enum
    }
  ]
})

const visibleScopes = computed(() => {
  return showAllScopes.value 
    ? props.apiKey.scopes 
    : props.apiKey.scopes.slice(0, maxVisibleScopes.value)
})

const securityScore = computed(() => {
  // Calculate security score based on settings
  let score = 0
  
  if (props.apiKey.securitySettings.requireHttps) score += 20
  if (props.apiKey.securitySettings.enableIpValidation && props.apiKey.ipWhitelist.length > 0) score += 25
  if (props.apiKey.securitySettings.enableRateLimiting) score += 15
  if (props.apiKey.expiresAt) score += 15
  if (!props.apiKey.scopes.includes('admin')) score += 10
  else score += 5
  if (props.apiKey.securitySettings.enableUsageAnalytics) score += 10
  
  return Math.min(score, 100)
})

/**
 * Event handlers (Xử lý sự kiện)
 */
const handleEdit = (): void => {
  emit('edit')
}

const handleRevoke = (): void => {
  emit('revoke')
}

const handleRegenerate = (): void => {
  emit('regenerate')
}

const handleCopyKey = (): void => {
  emit('copy-key')
}

const toggleKeyVisibility = (): void => {
  showFullKey.value = !showFullKey.value
}

/**
 * Utility functions (Hàm tiện ích)
 */
const copyToClipboard = async (text: string): Promise<void> => {
  try {
    await navigator.clipboard.writeText(text)
    // Show success notification
    console.log('Copied to clipboard:', text)
  } catch (error) {
    console.error('Failed to copy to clipboard:', error)
  }
}

const formatNumber = (num: number): string => {
  return num.toLocaleString()
}

const formatRelativeTime = (date: string): string => {
  const now = new Date()
  const target = new Date(date)
  const diffInSeconds = Math.floor((now.getTime() - target.getTime()) / 1000)
  
  if (Math.abs(diffInSeconds) < 60) return 'Just now'
  if (Math.abs(diffInSeconds) < 3600) return `${Math.floor(Math.abs(diffInSeconds) / 60)}m ${diffInSeconds < 0 ? 'from now' : 'ago'}`
  if (Math.abs(diffInSeconds) < 86400) return `${Math.floor(Math.abs(diffInSeconds) / 3600)}h ${diffInSeconds < 0 ? 'from now' : 'ago'}`
  if (Math.abs(diffInSeconds) < 2592000) return `${Math.floor(Math.abs(diffInSeconds) / 86400)}d ${diffInSeconds < 0 ? 'from now' : 'ago'}`
  
  return target.toLocaleDateString()
}

const formatFullDate = (date: string): string => {
  return new Date(date).toLocaleString()
}

const getUsagePercentage = (current: number, limit: number): number => {
  return Math.min((current / limit) * 100, 100)
}

const getUsageProgressColor = (current: number, limit: number): string => {
  const percentage = (current / limit) * 100
  if (percentage >= 90) return 'bg-danger'
  if (percentage >= 75) return 'bg-warning'
  return 'bg-success'
}

const getExpiryStatusClasses = (expiresAt: string): string => {
  const now = new Date()
  const expiry = new Date(expiresAt)
  const daysUntilExpiry = Math.floor((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
  
  if (daysUntilExpiry < 0) return 'text-danger'
  if (daysUntilExpiry < 7) return 'text-warning'
  return 'text-gray-600 dark:text-gray-400'
}
</script>

<style scoped>
.api-key-card {
  @apply bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden transition-all duration-200;
}

.api-key-card.clickable {
  @apply cursor-pointer hover:shadow-md hover:border-primary;
}

.api-key-card.selected {
  @apply border-primary shadow-md;
}

.api-key-card.revoked {
  @apply opacity-75;
}

.card-header {
  @apply p-6 border-b border-gray-200 dark:border-gray-700;
}

.card-content {
  @apply p-6 space-y-6;
}

.card-footer {
  @apply px-6 py-4 bg-gray-50 dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700;
}

.action-menu-trigger {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors;
}

.action-menu {
  @apply bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg py-1 min-w-[160px];
}

.action-menu-item {
  @apply flex items-center space-x-2 w-full px-3 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed;
}

.action-menu-divider {
  @apply border-t border-gray-200 dark:border-gray-700 my-1;
}

.key-display-section {
  @apply space-y-2;
}

.key-display {
  @apply flex items-center space-x-2 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg border;
}

.key-value {
  @apply flex-1 text-sm font-mono text-gray-900 dark:text-white break-all;
}

.copy-button {
  @apply p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors;
}

.stats-grid {
  @apply grid grid-cols-2 lg:grid-cols-4 gap-4;
}

.stat-item {
  @apply space-y-2;
}

.stat-label {
  @apply text-sm font-medium text-gray-600 dark:text-gray-400;
}

.stat-value {
  @apply text-lg font-semibold text-gray-900 dark:text-white;
}

.stat-progress {
  @apply w-full;
}

.progress-bar {
  @apply w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden;
}

.progress-fill {
  @apply h-full transition-all duration-500;
}

.scopes-section,
.security-section {
  @apply space-y-3;
}

.section-subtitle {
  @apply text-base font-semibold text-gray-900 dark:text-white;
}

.scopes-container {
  @apply space-y-3;
}

.scopes-list {
  @apply flex flex-wrap gap-2;
}

  .scope-toggle-button {
    @apply text-sm text-primary hover:text-primary/80;
  }

.security-details {
  @apply space-y-4;
}

.security-setting {
  @apply flex items-center justify-between;
}

.metadata-section {
  @apply pt-4 border-t border-gray-200 dark:border-gray-700;
}

.metadata-grid {
  @apply grid grid-cols-1 md:grid-cols-3 gap-4;
}

.metadata-item {
  @apply space-y-1;
}

.metadata-label {
  @apply block text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider;
}

.metadata-value {
  @apply block text-sm text-gray-900 dark:text-white;
}

.footer-action-button {
  @apply inline-flex items-center space-x-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors;
}

  .footer-action-button-primary {
    @apply inline-flex items-center space-x-2 px-3 py-1.5 bg-primary text-white text-sm font-medium rounded-lg hover:bg-primary/80 transition-colors disabled:opacity-50 disabled:cursor-not-allowed;
  }

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Compact variant adjustments */
.api-key-card.compact .card-content {
  @apply p-4 space-y-4;
}

.api-key-card.compact .stats-grid {
  @apply grid-cols-2;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .card-header,
  .card-content,
  .card-footer {
    @apply px-4;
  }
  
  .stats-grid {
    @apply grid-cols-1 gap-3;
  }
  
  .metadata-grid {
    @apply grid-cols-1 gap-2;
  }
}
</style> 