<template>
  <div class="space-y-6">
    <!-- Current IP Whitelist -->
    <div>
      <div class="flex items-center justify-between mb-4">
        <div>
          <h4 class="text-lg font-medium text-gray-900 dark:text-white">IP Whitelist Management</h4>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Manage allowed IP addresses and CIDR ranges for enhanced security
          </p>
        </div>
        <div class="flex items-center space-x-2">
          <span :class="[
            'px-2 py-1 text-xs rounded-full font-medium',
            apiKey?.securitySettings?.enableIpValidation
              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
              : 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300'
          ]">
            {{ apiKey?.securitySettings?.enableIpValidation ? 'IP Validation ON' : 'IP Validation OFF' }}
          </span>
        </div>
      </div>

      <!-- Toggle IP Validation -->
      <div class="mb-6 p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
        <div class="flex items-center justify-between">
          <div>
            <h5 class="font-medium text-blue-900 dark:text-blue-100">Enable IP Validation</h5>
            <p class="text-sm text-blue-700 dark:text-blue-300 mt-1">
              Restrict API key usage to specific IP addresses or ranges
            </p>
          </div>
          <label class="switch">
            <input
              v-model="ipValidationEnabled"
              type="checkbox"
              @change="toggleIpValidation"
            />
            <span class="slider round"></span>
          </label>
        </div>
      </div>

      <!-- Add New IP -->
      <div v-if="ipValidationEnabled" class="mb-6">
        <h5 class="font-medium text-gray-900 dark:text-white mb-3">Add IP Address or CIDR Range</h5>
        <div class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                IP Address or CIDR Range
              </label>
              <input
                v-model="newIpInput"
                type="text"
                placeholder="e.g., 192.168.1.100 or 10.0.0.0/24"
                class="form-input"
                @keyup.enter="addIpAddress"
              />
              <p class="text-xs text-gray-500 mt-1">
                Supports IPv4, IPv6, and CIDR notation (e.g., 192.168.1.0/24)
              </p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Description (Optional)
              </label>
              <input
                v-model="newIpDescription"
                type="text"
                placeholder="e.g., Office network"
                class="form-input"
                @keyup.enter="addIpAddress"
              />
            </div>
          </div>
          
          <div class="flex items-center space-x-3">
            <button
              @click="addIpAddress"
              :disabled="!newIpInput || !isValidIpOrCidr(newIpInput)"
              class="btn btn-primary btn-sm"
            >
              <icon-plus class="h-4 w-4 mr-2" />
              Add IP Address
            </button>
            
            <!-- Quick Add Buttons -->
            <div class="flex items-center space-x-2">
              <button
                @click="addCurrentIp"
                class="btn btn-outline-secondary btn-sm"
                :disabled="addingCurrentIp"
              >
                <icon-globe-alt class="h-4 w-4 mr-2" />
                {{ addingCurrentIp ? 'Detecting...' : 'Add Current IP' }}
              </button>
              
              <button
                @click="showCommonRanges = true"
                class="btn btn-outline-info btn-sm"
              >
                <icon-list-bullet class="h-4 w-4 mr-2" />
                Common Ranges
              </button>
            </div>
          </div>

          <!-- Validation Error -->
          <div v-if="newIpInput && !isValidIpOrCidr(newIpInput)" class="text-sm text-red-600 dark:text-red-400">
            <icon-exclamation-triangle class="h-4 w-4 inline mr-1" />
            Please enter a valid IP address or CIDR range
          </div>
        </div>
      </div>

      <!-- Current Whitelist -->
      <div v-if="ipValidationEnabled">
        <div class="flex items-center justify-between mb-3">
          <h5 class="font-medium text-gray-900 dark:text-white">
            Current Whitelist ({{ ipWhitelist.length }})
          </h5>
          <div class="flex items-center space-x-2">
            <!-- Bulk Actions -->
            <button
              v-if="selectedIps.length > 0"
              @click="bulkDeleteIps"
              class="btn btn-outline-danger btn-sm"
            >
              <icon-trash class="h-4 w-4 mr-2" />
              Delete Selected ({{ selectedIps.length }})
            </button>
            
            <button
              @click="exportWhitelist"
              class="btn btn-outline-secondary btn-sm"
            >
              <icon-arrow-down-tray class="h-4 w-4 mr-2" />
              Export
            </button>
          </div>
        </div>

        <!-- IP List -->
        <div v-if="ipWhitelist.length > 0" class="space-y-2">
          <div
            v-for="ip in ipWhitelist"
            :key="ip.id"
            class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            <div class="flex items-center space-x-3">
              <input
                v-model="selectedIps"
                :value="ip.id"
                type="checkbox"
                class="form-checkbox"
              />
              <div>
                <div class="flex items-center space-x-2">
                  <span class="font-mono text-sm font-medium">{{ ip.address }}</span>
                  <span :class="[
                    'px-2 py-1 text-xs rounded-full font-medium',
                    ip.type === 'single' ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300' :
                    'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300'
                  ]">
                    {{ ip.type === 'single' ? 'Single IP' : 'CIDR Range' }}
                  </span>
                  <span v-if="ip.isCurrentIp" class="px-2 py-1 text-xs rounded-full font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300">
                    Current IP
                  </span>
                </div>
                <div class="flex items-center space-x-4 mt-1">
                  <span v-if="ip.description" class="text-sm text-gray-600 dark:text-gray-400">
                    {{ ip.description }}
                  </span>
                  <span class="text-xs text-gray-500">
                    Added {{ formatDate(ip.createdAt) }}
                  </span>
                  <span v-if="ip.lastUsed" class="text-xs text-gray-500">
                    Last used {{ formatDate(ip.lastUsed) }}
                  </span>
                </div>
              </div>
            </div>
            
            <div class="flex items-center space-x-2">
              <!-- Usage Stats -->
              <div v-if="ip.usageCount > 0" class="text-right">
                <span class="text-sm font-medium text-gray-900 dark:text-white">
                  {{ ip.usageCount.toLocaleString() }}
                </span>
                <span class="text-xs text-gray-500 block">requests</span>
              </div>
              
              <!-- Actions -->
              <div class="flex items-center space-x-1">
                <button
                  @click="editIp(ip)"
                  class="btn btn-outline-secondary btn-xs"
                >
                  <icon-pencil class="h-3 w-3" />
                </button>
                <button
                  @click="deleteIp(ip.id)"
                  class="btn btn-outline-danger btn-xs"
                >
                  <icon-trash class="h-3 w-3" />
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div v-else class="text-center py-8 border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg">
          <icon-shield-exclamation class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-500">No IP addresses in whitelist</p>
          <p class="text-sm text-gray-400 mt-1">Add IP addresses to restrict access</p>
        </div>
      </div>

      <!-- IP Validation Disabled Message -->
      <div v-else class="text-center py-8 border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg">
        <icon-globe-alt class="h-12 w-12 text-gray-400 mx-auto mb-2" />
        <p class="text-gray-500">IP Validation is disabled</p>
        <p class="text-sm text-gray-400 mt-1">Enable IP validation to restrict access to specific addresses</p>
      </div>
    </div>

    <!-- Recent IP Activity -->
    <div v-if="ipValidationEnabled">
      <h5 class="font-medium text-gray-900 dark:text-white mb-3">Recent IP Activity</h5>
      <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-4">
        <div class="space-y-2">
          <div v-for="activity in recentIpActivity" :key="activity.id" class="flex items-center justify-between">
            <div class="flex items-center space-x-3">
              <div :class="[
                'w-2 h-2 rounded-full',
                activity.status === 'allowed' ? 'bg-green-500' :
                activity.status === 'blocked' ? 'bg-red-500' : 'bg-yellow-500'
              ]"></div>
              <span class="font-mono text-sm">{{ activity.ip }}</span>
              <span :class="[
                'px-2 py-1 text-xs rounded-full font-medium',
                activity.status === 'allowed' ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300' :
                activity.status === 'blocked' ? 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300' :
                'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300'
              ]">
                {{ activity.status }}
              </span>
            </div>
            <div class="text-right">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                {{ formatDate(activity.timestamp) }}
              </span>
            </div>
          </div>
        </div>
        
        <div v-if="recentIpActivity.length === 0" class="text-center py-4">
          <p class="text-sm text-gray-500">No recent IP activity</p>
        </div>
      </div>
    </div>

    <!-- Action Buttons -->
    <div class="flex items-center justify-end space-x-3 pt-4 border-t border-gray-200 dark:border-gray-700">
      <button
        @click="$emit('cancel')"
        class="btn btn-outline-secondary"
      >
        Cancel
      </button>
      <button
        @click="saveChanges"
        :disabled="!hasChanges || saving"
        class="btn btn-primary"
      >
        <icon-check class="h-4 w-4 mr-2" />
        {{ saving ? 'Saving...' : 'Save Changes' }}
      </button>
    </div>

    <!-- Common Ranges Modal -->
    <div v-if="showCommonRanges" class="fixed inset-0 z-[10000] overflow-y-auto">
      <div class="flex min-h-screen items-center justify-center px-4">
        <div class="fixed inset-0 bg-gray-500 bg-opacity-75" @click="showCommonRanges = false"></div>
        
        <div class="bg-white dark:bg-gray-900 rounded-lg p-6 max-w-md w-full relative">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Common IP Ranges</h3>
            <button @click="showCommonRanges = false" class="text-gray-400 hover:text-gray-600">
              <icon-x-mark class="h-5 w-5" />
            </button>
          </div>
          
          <div class="space-y-2">
            <button
              v-for="range in commonIpRanges"
              :key="range.cidr"
              @click="selectCommonRange(range)"
              class="w-full text-left p-3 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800"
            >
              <div class="font-mono text-sm">{{ range.cidr }}</div>
              <div class="text-xs text-gray-500">{{ range.description }}</div>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ApiKey } from '~/types/api-key'

// Props
interface Props {
  apiKey: ApiKey | null
}

const props = defineProps<Props>()

// Emits
interface Emits {
  (e: 'updated'): void
  (e: 'cancel'): void
}

const emit = defineEmits<Emits>()

// Reactive state
const ipValidationEnabled = ref(props.apiKey?.securitySettings?.enableIpValidation || false)
const newIpInput = ref('')
const newIpDescription = ref('')
const selectedIps = ref<string[]>([])
const showCommonRanges = ref(false)
const addingCurrentIp = ref(false)
const saving = ref(false)
const hasChanges = ref(false)

// Mock IP whitelist data
const ipWhitelist = ref([
  {
    id: '1',
    address: '192.168.1.100',
    type: 'single',
    description: 'Development machine',
    createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    lastUsed: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    usageCount: 1234,
    isCurrentIp: false
  },
  {
    id: '2',
    address: '10.0.0.0/24',
    type: 'range',
    description: 'Office network',
    createdAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(),
    lastUsed: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
    usageCount: 5678,
    isCurrentIp: false
  },
  {
    id: '3',
    address: '203.162.4.191',
    type: 'single',
    description: 'Production server',
    createdAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
    lastUsed: new Date().toISOString(),
    usageCount: 890,
    isCurrentIp: true
  }
])

const recentIpActivity = ref([
  {
    id: '1',
    ip: '192.168.1.100',
    status: 'allowed',
    timestamp: new Date(Date.now() - 5 * 60 * 1000).toISOString()
  },
  {
    id: '2',
    ip: '45.33.32.156',
    status: 'blocked',
    timestamp: new Date(Date.now() - 15 * 60 * 1000).toISOString()
  },
  {
    id: '3',
    ip: '203.162.4.191',
    status: 'allowed',
    timestamp: new Date(Date.now() - 30 * 60 * 1000).toISOString()
  }
])

const commonIpRanges = ref([
  { cidr: '10.0.0.0/8', description: 'Private network (Class A)' },
  { cidr: '172.16.0.0/12', description: 'Private network (Class B)' },
  { cidr: '192.168.0.0/16', description: 'Private network (Class C)' },
  { cidr: '127.0.0.0/8', description: 'Loopback addresses' },
  { cidr: '0.0.0.0/0', description: 'All IPv4 addresses (not recommended)' }
])

/**
 * Methods
 */
const isValidIpOrCidr = (input: string): boolean => {
  // Basic IP and CIDR validation
  const ipRegex = /^(\d{1,3}\.){3}\d{1,3}(\/\d{1,2})?$/
  const ipv6Regex = /^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$/
  
  if (ipRegex.test(input)) {
    const parts = input.split('/')
    const ip = parts[0].split('.')
    
    // Validate IP octets
    if (ip.some(octet => parseInt(octet) > 255)) return false
    
    // Validate CIDR suffix
    if (parts[1] && (parseInt(parts[1]) < 0 || parseInt(parts[1]) > 32)) return false
    
    return true
  }
  
  return ipv6Regex.test(input)
}

const addIpAddress = (): void => {
  if (!newIpInput.value || !isValidIpOrCidr(newIpInput.value)) return
  
  const newIp = {
    id: Date.now().toString(),
    address: newIpInput.value.trim(),
    type: newIpInput.value.includes('/') ? 'range' : 'single',
    description: newIpDescription.value.trim(),
    createdAt: new Date().toISOString(),
    lastUsed: null,
    usageCount: 0,
    isCurrentIp: false
  }
  
  ipWhitelist.value.push(newIp)
  newIpInput.value = ''
  newIpDescription.value = ''
  hasChanges.value = true
}

const addCurrentIp = async (): Promise<void> => {
  addingCurrentIp.value = true
  try {
    // Mock getting current IP
    await new Promise(resolve => setTimeout(resolve, 1000))
    const currentIp = '203.162.4.191' // Mock current IP
    
    newIpInput.value = currentIp
    newIpDescription.value = 'Current IP (auto-detected)'
    addIpAddress()
  } catch (error) {
    console.error('Failed to detect current IP:', error)
  } finally {
    addingCurrentIp.value = false
  }
}

const selectCommonRange = (range: typeof commonIpRanges.value[0]): void => {
  newIpInput.value = range.cidr
  newIpDescription.value = range.description
  showCommonRanges.value = false
}

const editIp = (ip: typeof ipWhitelist.value[0]): void => {
  newIpInput.value = ip.address
  newIpDescription.value = ip.description
  deleteIp(ip.id)
}

const deleteIp = (ipId: string): void => {
  ipWhitelist.value = ipWhitelist.value.filter(ip => ip.id !== ipId)
  hasChanges.value = true
}

const bulkDeleteIps = (): void => {
  if (confirm(`Are you sure you want to delete ${selectedIps.value.length} IP addresses?`)) {
    ipWhitelist.value = ipWhitelist.value.filter(ip => !selectedIps.value.includes(ip.id))
    selectedIps.value = []
    hasChanges.value = true
  }
}

const exportWhitelist = (): void => {
  const exportData = ipWhitelist.value.map(ip => ({
    address: ip.address,
    description: ip.description,
    type: ip.type,
    createdAt: ip.createdAt,
    usageCount: ip.usageCount
  }))
  
  const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `api-key-${props.apiKey?.id}-ip-whitelist.json`
  a.click()
  URL.revokeObjectURL(url)
}

const toggleIpValidation = (): void => {
  hasChanges.value = true
}

const saveChanges = async (): Promise<void> => {
  saving.value = true
  try {
    // Mock API call to save changes
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // Update the API key with new settings
    if (props.apiKey) {
      props.apiKey.securitySettings.enableIpValidation = ipValidationEnabled.value
      props.apiKey.ipWhitelist = ipWhitelist.value.map(ip => ip.address)
    }
    
    hasChanges.value = false
    emit('updated')
  } catch (error) {
    console.error('Failed to save IP management settings:', error)
  } finally {
    saving.value = false
  }
}

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMins / 60)
  const diffDays = Math.floor(diffHours / 24)
  
  if (diffMins < 1) return 'just now'
  if (diffMins < 60) return `${diffMins} minutes ago`
  if (diffHours < 24) return `${diffHours} hours ago`
  if (diffDays < 7) return `${diffDays} days ago`
  
  return date.toLocaleDateString('vi-VN')
}

/**
 * Watchers
 */
watch([ipValidationEnabled, ipWhitelist], () => {
  hasChanges.value = true
}, { deep: true })
</script>

<style scoped>
/* Toggle Switch Styles */
.switch {
  position: relative;
  display: inline-block;
  width: 50px;
  height: 24px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc;
  transition: .4s;
}

.slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: .4s;
}

input:checked + .slider {
  background-color: #00ab55;
}

input:checked + .slider:before {
  transform: translateX(26px);
}

.slider.round {
  border-radius: 24px;
}

.slider.round:before {
  border-radius: 50%;
}
</style> 