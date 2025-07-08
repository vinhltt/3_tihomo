<template>
  <div class="api-key-list">
    <!-- Header với Create Button và View Toggle (Tiêu đề với nút tạo và toggle xem) -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
      <div class="flex items-center space-x-3">
        <icon-key class="h-6 w-6 text-primary" />
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
          {{ $t('apiKey.management.title') }}
        </h2>
        <div v-if="!loading" class="text-sm text-gray-500 dark:text-gray-400">
          ({{ filteredApiKeys.length }} {{ $t('apiKey.management.keys') }})
        </div>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- View Mode Toggle -->
        <div class="flex rounded-lg bg-gray-100 dark:bg-gray-800 p-1">
          <button
            v-for="mode in viewModes"
            :key="mode.value"
            @click="viewMode = mode.value"
            :class="[
              'px-3 py-1.5 text-sm font-medium rounded-md transition-all duration-200',
              viewMode === mode.value
                ? 'bg-white dark:bg-gray-700 text-primary shadow-sm'
                : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
            ]"
          >
            <component :is="mode.icon" class="h-4 w-4" />
          </button>
        </div>
        
        <!-- Advanced Columns Toggle -->
        <button
          @click="showAdvancedColumns = !showAdvancedColumns"
          :class="[
            'inline-flex items-center px-3 py-2 text-sm font-medium rounded-lg transition-colors',
            showAdvancedColumns
              ? 'bg-primary text-white'
              : 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700'
          ]"
        >
          <icon-adjustments-horizontal class="h-4 w-4 mr-2" />
          {{ $t('apiKey.list.advancedColumns') }}
        </button>
        
        <!-- Create Button -->
        <button
          @click="$emit('create-key')"
          class="inline-flex items-center px-4 py-2 bg-primary text-white text-sm font-medium rounded-lg hover:bg-primary-dark transition-colors"
        >
          <icon-plus class="h-4 w-4 mr-2" />
          {{ $t('apiKey.actions.create') }}
        </button>
      </div>
    </div>

    <!-- Filters Section (Phần bộ lọc) -->
    <div class="panel mb-6 p-4">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <!-- Search Filter -->
        <div class="relative">
          <icon-magnifying-glass class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
          <input
            v-model="filters.search"
            type="text"
            :placeholder="$t('apiKey.filters.searchPlaceholder')"
            class="pl-10 form-input w-full"
          />
        </div>
        
        <!-- Status Filter -->
        <select
          v-model="filters.status"
          class="form-select"
        >
          <option value="">{{ $t('apiKey.filters.allStatuses') }}</option>
          <option value="active">{{ $t('apiKey.status.active') }}</option>
          <option value="revoked">{{ $t('apiKey.status.revoked') }}</option>
          <option value="expired">{{ $t('apiKey.status.expired') }}</option>
        </select>
        
        <!-- Scope Filter -->
        <select
          v-model="filters.scope"
          class="form-select"
        >
          <option value="">{{ $t('apiKey.filters.allScopes') }}</option>
          <option v-for="scope in availableScopes" :key="scope" :value="scope">
            {{ $t(`apiKey.scopes.${scope}`) }}
          </option>
        </select>
        
        <!-- Sort Options -->
        <select
          v-model="sortBy"
          class="form-select"
        >
          <option value="name">{{ $t('apiKey.sorting.name') }}</option>
          <option value="createdAt">{{ $t('apiKey.sorting.created') }}</option>
          <option value="lastUsedAt">{{ $t('apiKey.sorting.lastUsed') }}</option>
          <option value="usageCount">{{ $t('apiKey.sorting.usage') }}</option>
        </select>
      </div>
    </div>

    <!-- Table Container (Container bảng) -->
    <div class="panel">
      <!-- Loading Overlay -->
      <div v-if="loading" class="relative">
        <div class="absolute inset-0 bg-white dark:bg-gray-900 bg-opacity-50 flex items-center justify-center z-10">
          <div class="flex items-center space-x-3">
            <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
            <span class="text-gray-600 dark:text-gray-400">{{ $t('common.loading') }}...</span>
          </div>
        </div>
      </div>

      <!-- Table View -->
      <div v-if="viewMode === 'table'" class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th scope="col" class="table-header">
                <button @click="toggleSort('name')" class="flex items-center space-x-1 group">
                  <span>{{ $t('apiKey.table.name') }}</span>
                  <icon-chevron-up-down class="h-4 w-4 text-gray-400 group-hover:text-gray-600" />
                </button>
              </th>
              <th scope="col" class="table-header">{{ $t('apiKey.table.keyPrefix') }}</th>
              <th scope="col" class="table-header">{{ $t('apiKey.table.scopes') }}</th>
              <th scope="col" class="table-header">{{ $t('apiKey.table.status') }}</th>
              <th scope="col" class="table-header">{{ $t('apiKey.table.usage') }}</th>
              <th v-if="showAdvancedColumns" scope="col" class="table-header">
                <button @click="toggleSort('createdAt')" class="flex items-center space-x-1 group">
                  <span>{{ $t('apiKey.table.created') }}</span>
                  <icon-chevron-up-down class="h-4 w-4 text-gray-400 group-hover:text-gray-600" />
                </button>
              </th>
              <th v-if="showAdvancedColumns" scope="col" class="table-header">{{ $t('apiKey.table.expires') }}</th>
              <th scope="col" class="table-header text-right">{{ $t('apiKey.table.actions') }}</th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
            <tr
              v-for="apiKey in paginatedApiKeys"
              :key="apiKey.id"
              @click="$emit('select-key', apiKey.id)"
              :class="[
                'cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors',
                selectedKeyId === apiKey.id && 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800'
              ]"
            >
              <!-- Name Column -->
              <td class="table-cell">
                <div class="space-y-1">
                  <div class="font-medium text-gray-900 dark:text-white">
                    {{ apiKey.name }}
                  </div>
                  <div v-if="apiKey.description" class="text-sm text-gray-500 dark:text-gray-400 line-clamp-2">
                    {{ apiKey.description }}
                  </div>
                </div>
              </td>
              
              <!-- Key Prefix Column -->
              <td class="table-cell">
                <div class="flex items-center space-x-2">
                  <code class="px-2 py-1 text-xs font-mono bg-gray-100 dark:bg-gray-800 rounded">
                    {{ apiKey.keyPrefix }}***
                  </code>
                  <button
                    @click.stop="copyToClipboard(apiKey.keyPrefix)"
                    class="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
                    :title="$t('common.copy')"
                  >
                    <icon-clipboard class="h-4 w-4" />
                  </button>
                </div>
              </td>
              
              <!-- Scopes Column -->
              <td class="table-cell">
                <div class="flex flex-wrap gap-1">
                  <ScopeBadge
                    v-for="scope in apiKey.scopes.slice(0, 2)"
                    :key="scope"
                    :scope="scope"
                    size="sm"
                  />
                  <span
                    v-if="apiKey.scopes.length > 2"
                    class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300"
                  >
                    +{{ apiKey.scopes.length - 2 }}
                  </span>
                </div>
              </td>
              
              <!-- Status Column -->
              <td class="table-cell">
                <StatusBadge :status="apiKey.status" :expires-at="apiKey.expiresAt" />
              </td>
              
              <!-- Usage Column -->
              <td class="table-cell">
                <UsageIndicator
                  :current="apiKey.todayUsageCount"
                  :limit="apiKey.dailyUsageQuota"
                  :rate-limit="apiKey.rateLimitPerMinute"
                  variant="compact"
                  size="sm"
                />
              </td>
              
              <!-- Created Column (Advanced) -->
              <td v-if="showAdvancedColumns" class="table-cell">
                <div class="text-sm text-gray-600 dark:text-gray-400">
                  <time :datetime="apiKey.createdAt" :title="formatFullDate(apiKey.createdAt)">
                    {{ formatRelativeTime(apiKey.createdAt) }}
                  </time>
                </div>
              </td>
              
              <!-- Expires Column (Advanced) -->
              <td v-if="showAdvancedColumns" class="table-cell">
                <div v-if="apiKey.expiresAt" class="text-sm">
                  <time 
                    :datetime="apiKey.expiresAt"
                    :title="formatFullDate(apiKey.expiresAt)"
                    :class="getExpiryClasses(apiKey.expiresAt)"
                  >
                    {{ formatRelativeTime(apiKey.expiresAt) }}
                  </time>
                </div>
                <span v-else class="text-sm text-gray-500 dark:text-gray-400">
                  {{ $t('apiKey.expiry.never') }}
                </span>
              </td>
              
              <!-- Actions Column -->
              <td class="table-cell text-right">
                <div class="flex items-center justify-end space-x-2">
                  <button
                    @click.stop="$emit('edit-key', apiKey.id)"
                    class="p-1.5 text-gray-400 hover:text-primary transition-colors"
                    :title="$t('common.edit')"
                  >
                    <icon-pencil class="h-4 w-4" />
                  </button>
                  
                  <button
                    @click.stop="$emit('regenerate-key', apiKey.id)"
                    class="p-1.5 text-gray-400 hover:text-warning transition-colors"
                    :title="$t('apiKey.actions.regenerate')"
                  >
                    <icon-arrow-path class="h-4 w-4" />
                  </button>
                  
                  <button
                    @click.stop="$emit('revoke-key', apiKey.id)"
                    class="p-1.5 text-gray-400 hover:text-danger transition-colors"
                    :title="$t('apiKey.actions.revoke')"
                    :disabled="apiKey.status === 'revoked'"
                  >
                    <icon-trash class="h-4 w-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Card View -->
      <div v-else-if="viewMode === 'cards'" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 p-4">
        <div
          v-for="apiKey in paginatedApiKeys"
          :key="apiKey.id"
          @click="$emit('select-key', apiKey.id)"
          :class="[
            'p-4 border border-gray-200 dark:border-gray-700 rounded-lg cursor-pointer hover:border-primary transition-colors',
            selectedKeyId === apiKey.id && 'border-primary bg-blue-50 dark:bg-blue-900/20'
          ]"
        >
          <div class="space-y-3">
            <!-- Header -->
            <div class="flex items-start justify-between">
              <div class="space-y-1">
                <h3 class="font-medium text-gray-900 dark:text-white">{{ apiKey.name }}</h3>
                <p v-if="apiKey.description" class="text-sm text-gray-500 dark:text-gray-400 line-clamp-2">
                  {{ apiKey.description }}
                </p>
              </div>
              <StatusBadge :status="apiKey.status" :expires-at="apiKey.expiresAt" />
            </div>
            
            <!-- Key Prefix -->
            <div class="flex items-center space-x-2">
              <code class="flex-1 px-2 py-1 text-xs font-mono bg-gray-100 dark:bg-gray-800 rounded">
                {{ apiKey.keyPrefix }}***
              </code>
              <button
                @click.stop="copyToClipboard(apiKey.keyPrefix)"
                class="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
              >
                <icon-clipboard class="h-4 w-4" />
              </button>
            </div>
            
            <!-- Scopes -->
            <div class="space-y-2">
              <div class="text-sm font-medium text-gray-700 dark:text-gray-300">
                {{ $t('apiKey.table.scopes') }}
              </div>
              <div class="flex flex-wrap gap-1">
                <ScopeBadge
                  v-for="scope in apiKey.scopes"
                  :key="scope"
                  :scope="scope"
                  size="sm"
                />
              </div>
            </div>
            
            <!-- Usage -->
            <div class="space-y-2">
              <div class="text-sm font-medium text-gray-700 dark:text-gray-300">
                {{ $t('apiKey.table.usage') }}
              </div>
              <UsageIndicator
                :current="apiKey.todayUsageCount"
                :limit="apiKey.dailyUsageQuota"
                :rate-limit="apiKey.rateLimitPerMinute"
                variant="compact"
              />
            </div>
            
            <!-- Actions -->
            <div class="flex items-center justify-between pt-2 border-t border-gray-200 dark:border-gray-700">
              <div class="text-xs text-gray-500 dark:text-gray-400">
                {{ formatRelativeTime(apiKey.createdAt) }}
              </div>
              <div class="flex items-center space-x-1">
                <button
                  @click.stop="$emit('edit-key', apiKey.id)"
                  class="p-1.5 text-gray-400 hover:text-primary transition-colors"
                >
                  <icon-pencil class="h-4 w-4" />
                </button>
                <button
                  @click.stop="$emit('regenerate-key', apiKey.id)"
                  class="p-1.5 text-gray-400 hover:text-warning transition-colors"
                >
                  <icon-arrow-path class="h-4 w-4" />
                </button>
                <button
                  @click.stop="$emit('revoke-key', apiKey.id)"
                  class="p-1.5 text-gray-400 hover:text-danger transition-colors"
                  :disabled="apiKey.status === 'revoked'"
                >
                  <icon-trash class="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State (Trạng thái trống) -->
      <div v-if="!loading && filteredApiKeys.length === 0" class="text-center py-12">
        <icon-key class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-4 text-lg font-medium text-gray-900 dark:text-white">
          {{ hasFilters ? $t('apiKey.empty.noResults') : $t('apiKey.empty.noKeys') }}
        </h3>
        <p class="mt-2 text-gray-500 dark:text-gray-400">
          {{ hasFilters ? $t('apiKey.empty.tryDifferentFilter') : $t('apiKey.empty.createFirst') }}
        </p>
        <div class="mt-6">
          <button
            v-if="hasFilters"
            @click="clearFilters"
            class="btn-secondary mr-3"
          >
            {{ $t('common.clearFilters') }}
          </button>
          <button
            @click="$emit('create-key')"
            class="btn-primary"
          >
            <icon-plus class="h-4 w-4 mr-2" />
            {{ $t('apiKey.actions.create') }}
          </button>
        </div>
      </div>

      <!-- Pagination (Phân trang) -->
      <div v-if="!loading && filteredApiKeys.length > pageSize" class="flex items-center justify-between px-4 py-3 border-t border-gray-200 dark:border-gray-700">
        <div class="text-sm text-gray-700 dark:text-gray-300">
          {{ $t('common.showing') }} {{ (currentPage - 1) * pageSize + 1 }} {{ $t('common.to') }} 
          {{ Math.min(currentPage * pageSize, filteredApiKeys.length) }} {{ $t('common.of') }} 
          {{ filteredApiKeys.length }} {{ $t('apiKey.management.keys') }}
        </div>
        
        <div class="flex items-center space-x-2">
          <button
            @click="currentPage--"
            :disabled="currentPage === 1"
            class="btn-secondary px-3 py-1 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ $t('common.previous') }}
          </button>
          
          <span class="text-sm text-gray-700 dark:text-gray-300">
            {{ currentPage }} / {{ totalPages }}
          </span>
          
          <button
            @click="currentPage++"
            :disabled="currentPage === totalPages"
            class="btn-secondary px-3 py-1 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ $t('common.next') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ApiKey } from '~/types/api-key'
import { ApiKeyScope } from '~/types/api-key'

interface Props {
  apiKeys: ApiKey[]
  loading?: boolean
  selectedKeyId?: string
  showAdvancedColumns?: boolean
}

interface Emits {
  (e: 'select-key', keyId: string): void
  (e: 'create-key'): void
  (e: 'edit-key', keyId: string): void
  (e: 'revoke-key', keyId: string): void
  (e: 'regenerate-key', keyId: string): void
}

const props = withDefaults(defineProps<Props>(), {
  apiKeys: () => [],
  loading: false,
  selectedKeyId: '',
  showAdvancedColumns: false
})

const emit = defineEmits<Emits>()

/**
 * Reactive data (Dữ liệu reactive)
 */
const viewMode = ref<'table' | 'cards'>('table')
const showAdvancedColumns = ref(props.showAdvancedColumns)
const currentPage = ref(1)
const pageSize = ref(10)
const sortBy = ref<string>('name')
const sortDirection = ref<'asc' | 'desc'>('asc')

const filters = reactive({
  search: '',
  status: '',
  scope: ''
})

/**
 * View mode options (Tùy chọn chế độ xem)
 */
const viewModes = [
  { value: 'table', icon: 'icon-table-cells' },
  { value: 'cards', icon: 'icon-squares-2x2' }
]

/**
 * Available scopes for filtering (Scope có sẵn để lọc)
 */
const availableScopes = computed(() => {
  const allScopes = new Set<string>()
  props.apiKeys.forEach(key => {
    key.scopes.forEach(scope => allScopes.add(scope))
  })
  return Array.from(allScopes).sort()
})

/**
 * Check if any filters are active (Kiểm tra có filter nào đang hoạt động)
 */
const hasFilters = computed(() => {
  return filters.search || filters.status || filters.scope
})

/**
 * Filtered API keys (Danh sách API key đã lọc)
 */
const filteredApiKeys = computed(() => {
  let result = [...props.apiKeys]
  
  // Search filter
  if (filters.search) {
    const searchTerm = filters.search.toLowerCase()
    result = result.filter(key => 
      key.name.toLowerCase().includes(searchTerm) ||
      key.description?.toLowerCase().includes(searchTerm) ||
      key.keyPrefix.toLowerCase().includes(searchTerm)
    )
  }
  
  // Status filter  
  if (filters.status) {
    result = result.filter(key => key.status === filters.status)
  }
  
  // Scope filter
  if (filters.scope) {
    result = result.filter(key => key.scopes.includes(filters.scope))
  }
  
  // Sorting
  result.sort((a, b) => {
    let aValue = a[sortBy.value as keyof ApiKey]
    let bValue = b[sortBy.value as keyof ApiKey]
    
    // Handle different data types
    if (typeof aValue === 'string' && typeof bValue === 'string') {
      aValue = aValue.toLowerCase()
      bValue = bValue.toLowerCase()
    }
    
    if (aValue < bValue) return sortDirection.value === 'asc' ? -1 : 1
    if (aValue > bValue) return sortDirection.value === 'asc' ? 1 : -1
    return 0
  })
  
  return result
})

/**
 * Paginated API keys (Danh sách API key đã phân trang)
 */
const paginatedApiKeys = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredApiKeys.value.slice(start, end)
})

/**
 * Total pages for pagination (Tổng số trang)
 */
const totalPages = computed(() => {
  return Math.ceil(filteredApiKeys.value.length / pageSize.value)
})

/**
 * Toggle sort direction (Chuyển đổi hướng sắp xếp)
 */
const toggleSort = (field: string): void => {
  if (sortBy.value === field) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = field
    sortDirection.value = 'asc'
  }
  currentPage.value = 1
}

/**
 * Clear all filters (Xóa tất cả bộ lọc)
 */
const clearFilters = (): void => {
  filters.search = ''
  filters.status = ''
  filters.scope = ''
  currentPage.value = 1
}

/**
 * Copy text to clipboard (Sao chép văn bản vào clipboard)
 */
const copyToClipboard = async (text: string): Promise<void> => {
  try {
    await navigator.clipboard.writeText(text)
    // Show success toast/notification
    console.log('Copied to clipboard:', text)
  } catch (error) {
    console.error('Failed to copy to clipboard:', error)
  }
}

/**
 * Format date for display (Định dạng ngày để hiển thị)
 */
const formatRelativeTime = (date: string): string => {
  const now = new Date()
  const target = new Date(date)
  const diffInSeconds = Math.floor((now.getTime() - target.getTime()) / 1000)
  
  if (diffInSeconds < 60) return 'Just now'
  if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`
  if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`
  if (diffInSeconds < 2592000) return `${Math.floor(diffInSeconds / 86400)}d ago`
  
  return target.toLocaleDateString()
}

/**
 * Format full date with time (Định dạng ngày đầy đủ với thời gian)
 */
const formatFullDate = (date: string): string => {
  return new Date(date).toLocaleString()
}

/**
 * Get expiry status classes (Lấy class trạng thái hết hạn)
 */
const getExpiryClasses = (expiresAt: string): string => {
  const now = new Date()
  const expiry = new Date(expiresAt)
  const daysUntilExpiry = Math.floor((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
  
  if (daysUntilExpiry < 0) return 'text-danger'
  if (daysUntilExpiry < 7) return 'text-warning'
  return 'text-gray-600 dark:text-gray-400'
}

/**
 * Watch for changes in filters to reset pagination (Theo dõi thay đổi filter để reset phân trang)
 */
watch(filters, () => {
  currentPage.value = 1
}, { deep: true })
</script>

<style scoped>
.table-header {
  @apply px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider;
}

.table-cell {
  @apply px-6 py-4 whitespace-nowrap;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Responsive table adjustments (Điều chỉnh bảng responsive) */
@media (max-width: 768px) {
  .table-header,
  .table-cell {
    @apply px-3 py-2;
  }
  
  .table-header {
    @apply text-xs;
  }
}
</style> 