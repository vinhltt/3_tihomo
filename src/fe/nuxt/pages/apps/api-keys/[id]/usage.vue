<template>
  <div>
    <!-- Page Header -->
    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 mb-6">
      <div>
        <div class="flex items-center space-x-3 mb-2">
          <button
            @click="$router.back()"
            class="btn btn-outline-secondary btn-sm"
          >
            <icon-arrow-left class="h-4 w-4 mr-2" />
            Back
          </button>
          <h5 class="font-semibold text-lg dark:text-white-light">API Key Usage Analytics</h5>
        </div>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          Detailed usage statistics for: <span class="font-medium text-primary">{{ apiKey?.name || keyId }}</span>
        </p>
      </div>

      <!-- Breadcrumb -->
      <NavigationBreadcrumb :items="breadcrumbs" />
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
        <p class="text-gray-500">Loading usage analytics...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="panel !p-8 text-center">
      <div class="w-16 h-16 bg-red-100 dark:bg-red-900 rounded-full flex items-center justify-center mx-auto mb-4">
        <icon-exclamation-triangle class="h-8 w-8 text-red-600" />
      </div>
      <h3 class="text-lg font-semibold mb-2 text-gray-900 dark:text-white">Failed to load analytics</h3>
      <p class="text-gray-500 mb-4">{{ error }}</p>
      <button @click="refreshData" class="btn btn-primary">
        <icon-arrow-path class="h-4 w-4 mr-2" />
        Retry
      </button>
    </div>

    <!-- Main Content -->
    <div v-else class="space-y-6">
      <!-- Quick Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="panel !p-4">
          <div class="flex items-center space-x-3">
            <div class="flex-shrink-0">
              <icon-chart-bar class="h-8 w-8 text-primary" />
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Total Requests</p>
              <p class="text-xl font-semibold text-gray-900 dark:text-white">
                {{ usageStats?.totalRequests?.toLocaleString() || '0' }}
              </p>
            </div>
          </div>
        </div>
        
        <div class="panel !p-4">
          <div class="flex items-center space-x-3">
            <div class="flex-shrink-0">
              <icon-clock class="h-8 w-8 text-success" />
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Today's Requests</p>
              <p class="text-xl font-semibold text-success">
                {{ usageStats?.todayRequests?.toLocaleString() || '0' }}
              </p>
            </div>
          </div>
        </div>
        
        <div class="panel !p-4">
          <div class="flex items-center space-x-3">
            <div class="flex-shrink-0">
              <icon-signal class="h-8 w-8 text-info" />
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Success Rate</p>
              <p class="text-xl font-semibold text-info">
                {{ usageStats?.successRate?.toFixed(1) || '0' }}%
              </p>
            </div>
          </div>
        </div>
        
        <div class="panel !p-4">
          <div class="flex items-center space-x-3">
            <div class="flex-shrink-0">
              <icon-bolt class="h-8 w-8 text-warning" />
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Avg Response Time</p>
              <p class="text-xl font-semibold text-warning">
                {{ usageStats?.avgResponseTime || '0' }}ms
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Time Range Selector -->
      <div class="panel !p-4">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <h6 class="font-medium text-gray-900 dark:text-white">Usage Analytics</h6>
          
          <div class="flex items-center space-x-3">
            <!-- Time Range Buttons -->
            <div class="btn-group">
              <button
                v-for="range in timeRanges"
                :key="range.value"
                @click="selectedTimeRange = range.value"
                :class="[
                  'btn btn-sm',
                  selectedTimeRange === range.value ? 'btn-primary' : 'btn-outline-primary'
                ]"
              >
                {{ range.label }}
              </button>
            </div>
            
            <!-- Refresh Button -->
            <button
              @click="refreshData"
              class="btn btn-outline-secondary btn-sm"
              :disabled="refreshing"
            >
              <icon-arrow-path :class="['h-4 w-4', refreshing && 'animate-spin']" />
            </button>
          </div>
        </div>
      </div>

      <!-- Charts Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Requests Over Time Chart -->
        <div class="panel">
          <div class="mb-5 flex items-center justify-between">
            <h5 class="text-lg font-semibold dark:text-white-light">Requests Over Time</h5>
          </div>
          
          <client-only>
            <apexchart
              v-if="requestsChartData"
              height="300"
              type="line"
              :options="requestsChartOptions"
              :series="requestsChartData"
            />
            <div v-else class="flex items-center justify-center h-64">
              <p class="text-gray-500">No data available</p>
            </div>
          </client-only>
        </div>

        <!-- Status Code Distribution -->
        <div class="panel">
          <div class="mb-5 flex items-center justify-between">
            <h5 class="text-lg font-semibold dark:text-white-light">Response Status Distribution</h5>
          </div>
          
          <client-only>
            <apexchart
              v-if="statusChartData"
              height="300"
              type="donut"
              :options="statusChartOptions"
              :series="statusChartData"
            />
            <div v-else class="flex items-center justify-center h-64">
              <p class="text-gray-500">No data available</p>
            </div>
          </client-only>
        </div>
      </div>

      <!-- Rate Limiting Progress -->
      <div class="panel" v-if="apiKey?.securitySettings?.enableRateLimiting">
        <div class="mb-5">
          <h5 class="text-lg font-semibold dark:text-white-light mb-2">Rate Limiting Status</h5>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Current usage against configured rate limits
          </p>
        </div>
        
        <div class="space-y-4">
          <!-- Per Minute Limit -->
          <div v-if="apiKey.rateLimitPerMinute">
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
                Per Minute Limit
              </span>
              <span class="text-sm text-gray-500">
                {{ usageStats?.currentMinuteRequests || 0 }} / {{ apiKey.rateLimitPerMinute }}
              </span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2 dark:bg-gray-700">
              <div
                class="bg-primary h-2 rounded-full transition-all duration-300"
                :style="{ width: `${Math.min((usageStats?.currentMinuteRequests || 0) / apiKey.rateLimitPerMinute * 100, 100)}%` }"
              ></div>
            </div>
          </div>

          <!-- Per Day Limit -->
          <div v-if="apiKey.rateLimitPerDay">
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
                Per Day Limit
              </span>
              <span class="text-sm text-gray-500">
                {{ usageStats?.todayRequests || 0 }} / {{ apiKey.rateLimitPerDay }}
              </span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2 dark:bg-gray-700">
              <div
                class="bg-info h-2 rounded-full transition-all duration-300"
                :style="{ width: `${Math.min((usageStats?.todayRequests || 0) / apiKey.rateLimitPerDay * 100, 100)}%` }"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Requests Table -->
      <div class="panel">
        <div class="mb-5 flex items-center justify-between">
          <h5 class="text-lg font-semibold dark:text-white-light">Recent Requests</h5>
          <div class="flex items-center space-x-2">
            <button
              @click="toggleAutoRefresh"
              :class="[
                'btn btn-sm',
                autoRefresh ? 'btn-success' : 'btn-outline-secondary'
              ]"
            >
              <icon-arrow-path :class="['h-4 w-4 mr-2', autoRefresh && 'animate-spin']" />
              {{ autoRefresh ? 'Auto Refresh ON' : 'Auto Refresh OFF' }}
            </button>
          </div>
        </div>
        
        <div class="table-responsive">
          <table class="table-hover">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Method</th>
                <th>Endpoint</th>
                <th>Status</th>
                <th>Response Time</th>
                <th>IP Address</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="request in recentRequests" :key="request.id">
                <td>
                  <span class="text-sm text-gray-600 dark:text-gray-400">
                    {{ formatDateTime(request.timestamp) }}
                  </span>
                </td>
                <td>
                  <span :class="[
                    'px-2 py-1 text-xs rounded font-medium',
                    getMethodColor(request.method)
                  ]">
                    {{ request.method }}
                  </span>
                </td>
                <td>
                  <span class="font-mono text-sm">{{ request.endpoint }}</span>
                </td>
                <td>
                  <span :class="[
                    'px-2 py-1 text-xs rounded font-medium',
                    getStatusColor(request.statusCode)
                  ]">
                    {{ request.statusCode }}
                  </span>
                </td>
                <td>
                  <span class="text-sm">{{ request.responseTime }}ms</span>
                </td>
                <td>
                  <span class="font-mono text-sm text-gray-600 dark:text-gray-400">
                    {{ request.ipAddress }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
          
          <!-- Empty state for recent requests -->
          <div v-if="!recentRequests?.length" class="text-center py-8">
            <icon-document-chart-bar class="h-12 w-12 text-gray-400 mx-auto mb-2" />
            <p class="text-gray-500">No recent requests</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ApiKey } from '~/types/api-key'

// Meta tags
useHead({
  title: 'API Key Usage Analytics - TiHoMo'
})

// Route parameters
const route = useRoute()
const keyId = route.params.id as string

// Composables
const { getApiKey } = useApiKeys()

// Reactive state
const loading = ref(true)
const refreshing = ref(false)
const error = ref<string>('')
const apiKey = ref<ApiKey | null>(null)
const autoRefresh = ref(false)
const autoRefreshInterval = ref<NodeJS.Timeout>()

// Time range selection
const selectedTimeRange = ref('7d')
const timeRanges = [
  { label: '1D', value: '1d' },
  { label: '7D', value: '7d' },
  { label: '30D', value: '30d' },
  { label: '90D', value: '90d' }
]

// Mock data - Replace with real API calls
const usageStats = ref({
  totalRequests: 12456,
  todayRequests: 234,
  successRate: 99.2,
  avgResponseTime: 145,
  currentMinuteRequests: 3
})

const recentRequests = ref([
  {
    id: '1',
    timestamp: new Date().toISOString(),
    method: 'GET',
    endpoint: '/api/transactions',
    statusCode: 200,
    responseTime: 123,
    ipAddress: '192.168.1.100'
  },
  {
    id: '2',
    timestamp: new Date(Date.now() - 60000).toISOString(),
    method: 'POST',
    endpoint: '/api/accounts',
    statusCode: 201,
    responseTime: 256,
    ipAddress: '10.0.0.1'
  },
  {
    id: '3',
    timestamp: new Date(Date.now() - 120000).toISOString(),
    method: 'GET',
    endpoint: '/api/transactions/search',
    statusCode: 400,
    responseTime: 89,
    ipAddress: '192.168.1.100'
  }
])

// Breadcrumb
const breadcrumbs = computed(() => [
  { name: 'Apps', href: '/apps' },
  { name: 'API Keys', href: '/apps/api-keys' },
  { name: 'Usage Analytics', href: `#` }
])

// Chart data
const requestsChartData = computed(() => {
  // Mock data - replace with real API data
  return [{
    name: 'Requests',
    data: [65, 78, 90, 85, 95, 88, 92, 89, 102, 95, 88, 96]
  }]
})

const requestsChartOptions = computed(() => ({
  chart: {
    toolbar: { show: false },
    background: 'transparent'
  },
  colors: ['#00ab55'],
  stroke: { width: 3, curve: 'smooth' },
  xaxis: {
    categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
  },
  grid: { borderColor: '#e0e6ed' },
  theme: { mode: 'light' }
}))

const statusChartData = computed(() => [85, 10, 3, 2])
const statusChartOptions = computed(() => ({
  chart: { toolbar: { show: false } },
  colors: ['#00ab55', '#e7515a', '#e2a03f', '#805dca'],
  labels: ['2xx Success', '4xx Client Error', '5xx Server Error', '3xx Redirect'],
  legend: { position: 'bottom' }
}))

/**
 * Methods
 */
const loadApiKey = async (): Promise<void> => {
  try {
    loading.value = true
    // Mock API call - replace with real implementation
    apiKey.value = {
      id: keyId,
      name: `API Key ${keyId}`,
      keyPrefix: 'tihomo_',
      status: 'active',
      scopes: ['read:transactions', 'read:accounts'],
      createdAt: new Date().toISOString(),
      expiresAt: null,
      lastUsedAt: new Date().toISOString(),
      todayUsageCount: 234,
      totalUsageCount: 12456,
      rateLimitPerMinute: 100,
      rateLimitPerDay: 10000,
      ipWhitelist: [],
      securitySettings: {
        requireHttps: true,
        enableIpValidation: false,
        enableRateLimiting: true,
        enableUsageAnalytics: true,
        corsOrigins: []
      }
    } as ApiKey
  } catch (err) {
    error.value = 'Failed to load API key data'
    console.error('Error loading API key:', err)
  } finally {
    loading.value = false
  }
}

const refreshData = async (): Promise<void> => {
  refreshing.value = true
  try {
    await loadApiKey()
    // Refresh usage stats and recent requests
    await new Promise(resolve => setTimeout(resolve, 500)) // Mock delay
  } catch (err) {
    console.error('Error refreshing data:', err)
  } finally {
    refreshing.value = false
  }
}

const toggleAutoRefresh = (): void => {
  autoRefresh.value = !autoRefresh.value
  
  if (autoRefresh.value) {
    autoRefreshInterval.value = setInterval(() => {
      refreshData()
    }, 30000) // Refresh every 30 seconds
  } else {
    if (autoRefreshInterval.value) {
      clearInterval(autoRefreshInterval.value)
    }
  }
}

/**
 * Utility functions
 */
const formatDateTime = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

const getMethodColor = (method: string): string => {
  const colors = {
    GET: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
    POST: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
    PUT: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
    DELETE: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
    PATCH: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300'
  }
  return colors[method as keyof typeof colors] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300'
}

const getStatusColor = (statusCode: number): string => {
  if (statusCode >= 200 && statusCode < 300) {
    return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
  } else if (statusCode >= 300 && statusCode < 400) {
    return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300'
  } else if (statusCode >= 400 && statusCode < 500) {
    return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300'
  } else {
    return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'
  }
}

/**
 * Lifecycle
 */
onMounted(() => {
  loadApiKey()
})

onUnmounted(() => {
  if (autoRefreshInterval.value) {
    clearInterval(autoRefreshInterval.value)
  }
})

// Watch time range changes
watch(selectedTimeRange, () => {
  refreshData()
})
</script>

<style scoped>
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
</style> 