# API Key Management Component Library

## 📋 Overview

Component library cho API Key Management feature trong TiHoMo application. Các components được thiết kế theo design system và patterns đã established.

---

## 🧩 Core Components

### 1. ApiKeyList Component

#### Purpose
Hiển thị danh sách API keys của user với filtering, sorting, và pagination.

#### Props
```typescript
interface ApiKeyListProps {
  apiKeys: ApiKey[]
  loading?: boolean
  selectedKeyId?: string
  showAdvancedColumns?: boolean
}
```

#### Events
```typescript
interface ApiKeyListEvents {
  'select-key': (keyId: string) => void
  'create-key': () => void
  'edit-key': (keyId: string) => void
  'revoke-key': (keyId: string) => void
  'regenerate-key': (keyId: string) => void
}
```

#### Usage
```vue
<template>
  <ApiKeyList
    :api-keys="apiKeys"
    :loading="isLoading"
    :selected-key-id="selectedKeyId"
    @select-key="handleSelectKey"
    @create-key="showCreateModal = true"
    @revoke-key="handleRevokeKey"
  />
</template>
```

#### Implementation
```vue
<template>
  <div class="api-key-list">
    <!-- Header với Create Button -->
    <div class="list-header">
      <h2 class="text-2xl font-semibold text-gray-900">API Keys</h2>
      <button @click="$emit('create-key')" class="btn-primary">
        <Icon name="plus" class="w-4 h-4 mr-2" />
        Create New API Key
      </button>
    </div>
    
    <!-- Filters -->
    <ApiKeyFilters v-model="filters" />
    
    <!-- Table -->
    <div class="table-container">
      <table class="api-key-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Key Prefix</th>
            <th>Scopes</th>
            <th>Status</th>
            <th>Usage</th>
            <th v-if="showAdvancedColumns">Created</th>
            <th v-if="showAdvancedColumns">Expires</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="apiKey in filteredApiKeys" 
            :key="apiKey.id"
            @click="$emit('select-key', apiKey.id)"
            class="api-key-row"
            :class="{ 'selected': selectedKeyId === apiKey.id }"
          >
            <td class="name-cell">
              <div class="name-content">
                <span class="name">{{ apiKey.name }}</span>
                <span v-if="apiKey.description" class="description">
                  {{ apiKey.description }}
                </span>
              </div>
            </td>
            
            <td class="prefix-cell">
              <code class="key-prefix">{{ apiKey.keyPrefix }}***</code>
              <button @click.stop="copyPrefix(apiKey.keyPrefix)" class="copy-btn">
                <Icon name="copy" class="w-3 h-3" />
              </button>
            </td>
            
            <td class="scopes-cell">
              <div class="scopes-list">
                <ScopeBadge 
                  v-for="scope in apiKey.scopes.slice(0, 3)" 
                  :key="scope"
                  :scope="scope"
                />
                <span v-if="apiKey.scopes.length > 3" class="scope-more">
                  +{{ apiKey.scopes.length - 3 }} more
                </span>
              </div>
            </td>
            
            <td class="status-cell">
              <StatusBadge :status="apiKey.status" />
            </td>
            
            <td class="usage-cell">
              <UsageIndicator 
                :current="apiKey.todayUsageCount"
                :limit="apiKey.dailyUsageQuota"
                :rate-limit="apiKey.rateLimitPerMinute"
              />
            </td>
            
            <td v-if="showAdvancedColumns" class="created-cell">
              <time :datetime="apiKey.createdAt">
                {{ formatRelativeTime(apiKey.createdAt) }}
              </time>
            </td>
            
            <td v-if="showAdvancedColumns" class="expires-cell">
              <time 
                v-if="apiKey.expiresAt"
                :datetime="apiKey.expiresAt"
                :class="getExpiryClass(apiKey.expiresAt)"
              >
                {{ formatRelativeTime(apiKey.expiresAt) }}
              </time>
              <span v-else class="no-expiry">Never</span>
            </td>
            
            <td class="actions-cell">
              <ApiKeyActions
                :api-key="apiKey"
                @edit="$emit('edit-key', apiKey.id)"
                @revoke="$emit('revoke-key', apiKey.id)"
                @regenerate="$emit('regenerate-key', apiKey.id)"
              />
            </td>
          </tr>
        </tbody>
      </table>
      
      <!-- Loading State -->
      <div v-if="loading" class="loading-overlay">
        <LoadingSpinner />
        <span>Loading API keys...</span>
      </div>
      
      <!-- Empty State -->
      <div v-if="!loading && apiKeys.length === 0" class="empty-state">
        <Icon name="key" class="empty-icon" />
        <h3>No API keys yet</h3>
        <p>Create your first API key to start integrating with TiHoMo</p>
        <button @click="$emit('create-key')" class="btn-primary">
          Create API Key
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.api-key-list {
  @apply space-y-6;
}

.list-header {
  @apply flex justify-between items-center;
}

.api-key-table {
  @apply w-full bg-white rounded-lg shadow-sm border border-gray-200;
}

.api-key-row {
  @apply hover:bg-gray-50 cursor-pointer transition-colors;
}

.api-key-row.selected {
  @apply bg-blue-50 border-blue-200;
}

.name-content {
  @apply space-y-1;
}

.name {
  @apply font-medium text-gray-900;
}

.description {
  @apply text-sm text-gray-500;
}

.key-prefix {
  @apply font-mono text-sm bg-gray-100 px-2 py-1 rounded;
}

.copy-btn {
  @apply ml-2 p-1 text-gray-400 hover:text-gray-600 transition-colors;
}

.empty-state {
  @apply text-center py-12 space-y-4;
}

.empty-icon {
  @apply w-12 h-12 mx-auto text-gray-400;
}
</style>
```

---

### 2. CreateApiKeyModal Component

#### Purpose
Modal để tạo API key mới với form validation và security settings.

#### Props
```typescript
interface CreateApiKeyModalProps {
  show: boolean
  loading?: boolean
}
```

#### Events
```typescript
interface CreateApiKeyModalEvents {
  'update:show': (show: boolean) => void
  'create': (request: CreateApiKeyRequest) => void
}
```

#### Usage
```vue
<template>
  <CreateApiKeyModal
    v-model:show="showCreateModal"
    :loading="isCreating"
    @create="handleCreateApiKey"
  />
</template>
```

#### Implementation
```vue
<template>
  <Modal 
    :show="show" 
    @update:show="$emit('update:show', $event)"
    title="Create New API Key" 
    size="large"
  >
    <form @submit.prevent="handleSubmit" class="create-api-key-form">
      <!-- Basic Information Section -->
      <FormSection title="Basic Information">
        <div class="form-grid">
          <FormField
            label="Name"
            required
            :error="errors.name"
          >
            <input
              v-model="form.name"
              type="text"
              placeholder="e.g., Production API, Mobile App"
              class="form-input"
              :class="{ 'error': errors.name }"
            />
          </FormField>
          
          <FormField
            label="Description"
            :error="errors.description"
          >
            <textarea
              v-model="form.description"
              rows="3"
              placeholder="Describe the purpose and usage of this API key..."
              class="form-textarea"
            />
          </FormField>
          
          <FormField
            label="Expires At"
            :error="errors.expiresAt"
          >
            <input
              v-model="form.expiresAt"
              type="datetime-local"
              class="form-input"
            />
            <FormHint>Leave empty for no expiration</FormHint>
          </FormField>
        </div>
      </FormSection>
      
      <!-- Permissions Section -->
      <FormSection title="Permissions">
        <ScopeSelector 
          v-model="form.scopes"
          :error="errors.scopes"
        />
      </FormSection>
      
      <!-- Rate Limiting Section -->
      <FormSection title="Rate Limiting">
        <div class="form-row">
          <FormField
            label="Requests per minute"
            :error="errors.rateLimitPerMinute"
          >
            <input
              v-model.number="form.rateLimitPerMinute"
              type="number"
              min="1"
              max="1000"
              class="form-input"
            />
          </FormField>
          
          <FormField
            label="Daily quota"
            :error="errors.dailyUsageQuota"
          >
            <input
              v-model.number="form.dailyUsageQuota"
              type="number"
              min="100"
              class="form-input"
            />
          </FormField>
        </div>
      </FormSection>
      
      <!-- Advanced Security Section (Collapsible) -->
      <CollapsibleSection 
        title="Advanced Security Settings"
        v-model:open="showAdvancedSecurity"
      >
        <div class="advanced-settings">
          <FormField
            label="IP Whitelist"
            :error="errors.ipWhitelist"
          >
            <textarea
              v-model="ipWhitelistText"
              rows="3"
              placeholder="192.168.1.1&#10;10.0.0.0/8&#10;2001:db8::/32"
              class="form-textarea"
            />
            <FormHint>One IP address or CIDR block per line</FormHint>
          </FormField>
          
          <FormField label="Security Options">
            <div class="checkbox-group">
              <CheckboxField
                v-model="form.securitySettings.requireHttps"
                label="Require HTTPS"
                description="Only allow requests over HTTPS"
              />
              
              <CheckboxField
                v-model="form.securitySettings.allowCorsRequests"
                label="Allow CORS requests"
                description="Enable cross-origin requests"
              />
            </div>
          </FormField>
        </div>
      </CollapsibleSection>
      
      <!-- Form Actions -->
      <div class="form-actions">
        <button 
          type="button" 
          @click="$emit('update:show', false)" 
          class="btn-secondary"
        >
          Cancel
        </button>
        <button 
          type="submit" 
          class="btn-primary" 
          :disabled="!isFormValid || loading"
        >
          <Icon v-if="loading" name="spinner" class="animate-spin w-4 h-4 mr-2" />
          {{ loading ? 'Creating...' : 'Create API Key' }}
        </button>
      </div>
    </form>
  </Modal>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useApiKeyValidation } from '@/composables/useApiKeyValidation'

const props = defineProps<CreateApiKeyModalProps>()
const emit = defineEmits<CreateApiKeyModalEvents>()

// Form state
const form = ref({
  name: '',
  description: '',
  expiresAt: '',
  scopes: [],
  rateLimitPerMinute: 100,
  dailyUsageQuota: 10000,
  ipWhitelist: [],
  securitySettings: {
    requireHttps: true,
    allowCorsRequests: false,
    allowedOrigins: []
  }
})

// UI state
const showAdvancedSecurity = ref(false)
const ipWhitelistText = ref('')

// Validation
const { errors, validateForm, isFormValid } = useApiKeyValidation(form)

// Watch IP whitelist text changes
watch(ipWhitelistText, (value) => {
  form.value.ipWhitelist = value
    .split('\n')
    .map(ip => ip.trim())
    .filter(ip => ip.length > 0)
})

// Form submission
const handleSubmit = async () => {
  if (!validateForm()) return
  
  emit('create', {
    ...form.value,
    ipWhitelist: form.value.ipWhitelist
  })
}

// Reset form when modal closes
watch(() => props.show, (show) => {
  if (!show) {
    // Reset form
    Object.assign(form.value, {
      name: '',
      description: '',
      expiresAt: '',
      scopes: [],
      rateLimitPerMinute: 100,
      dailyUsageQuota: 10000,
      ipWhitelist: [],
      securitySettings: {
        requireHttps: true,
        allowCorsRequests: false,
        allowedOrigins: []
      }
    })
    ipWhitelistText.value = ''
    showAdvancedSecurity.value = false
  }
})
</script>
```

---

### 3. ScopeSelector Component

#### Purpose
Component để chọn API key scopes với templates và individual permissions.

#### Props
```typescript
interface ScopeSelectorProps {
  modelValue: string[]
  error?: string
}
```

#### Events
```typescript
interface ScopeSelectorEvents {
  'update:modelValue': (scopes: string[]) => void
}
```

#### Implementation
```vue
<template>
  <div class="scope-selector">
    <!-- Quick Templates -->
    <div class="scope-templates">
      <h4 class="template-title">Quick Templates</h4>
      <div class="template-buttons">
        <button
          v-for="template in scopeTemplates"
          :key="template.name"
          @click="applyTemplate(template)"
          class="template-button"
          type="button"
        >
          <Icon :name="template.icon" class="w-4 h-4 mr-2" />
          {{ template.name }}
        </button>
      </div>
    </div>
    
    <!-- Individual Scopes -->
    <div class="individual-scopes">
      <h4 class="scopes-title">Individual Permissions</h4>
      
      <div class="scope-groups">
        <div 
          v-for="group in scopeGroups" 
          :key="group.name" 
          class="scope-group"
        >
          <h5 class="group-title">{{ group.name }}</h5>
          <div class="scope-checkboxes">
            <label
              v-for="scope in group.scopes"
              :key="scope.value"
              class="scope-checkbox"
              :class="{
                'dangerous': scope.dangerous,
                'selected': selectedScopes.includes(scope.value)
              }"
            >
              <input
                type="checkbox"
                :value="scope.value"
                v-model="selectedScopes"
                class="scope-input"
              />
              <div class="scope-info">
                <div class="scope-header">
                  <span class="scope-name">{{ scope.name }}</span>
                  <Icon 
                    v-if="scope.dangerous" 
                    name="exclamation-triangle" 
                    class="danger-icon"
                  />
                </div>
                <span class="scope-description">{{ scope.description }}</span>
              </div>
            </label>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Selected Scopes Summary -->
    <div class="selected-summary">
      <h4 class="summary-title">
        Selected Permissions ({{ selectedScopes.length }})
      </h4>
      <div class="selected-scopes">
        <span
          v-for="scope in selectedScopes"
          :key="scope"
          class="selected-scope"
        >
          {{ scope }}
          <button @click="removeScope(scope)" class="remove-scope">×</button>
        </span>
      </div>
      
      <!-- Error Message -->
      <div v-if="error" class="error-message">
        {{ error }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<ScopeSelectorProps>()
const emit = defineEmits<ScopeSelectorEvents>()

const selectedScopes = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const scopeTemplates = [
  {
    name: 'Read Only',
    icon: 'eye',
    description: 'View data only',
    scopes: ['read', 'accounts:read', 'transactions:read']
  },
  {
    name: 'Full Access',
    icon: 'key',
    description: 'Complete access to all features',
    scopes: ['read', 'write', 'delete', 'accounts:read', 'accounts:write', 'transactions:read', 'transactions:write']
  },
  {
    name: 'Transactions Only',
    icon: 'credit-card',
    description: 'Manage transactions only',
    scopes: ['transactions:read', 'transactions:write']
  }
]

const scopeGroups = [
  {
    name: 'General',
    scopes: [
      { 
        value: 'read', 
        name: 'Read', 
        description: 'View all data', 
        dangerous: false 
      },
      { 
        value: 'write', 
        name: 'Write', 
        description: 'Create and modify data', 
        dangerous: false 
      },
      { 
        value: 'delete', 
        name: 'Delete', 
        description: 'Delete data', 
        dangerous: true 
      },
      { 
        value: 'admin', 
        name: 'Admin', 
        description: 'Full administrative access', 
        dangerous: true 
      }
    ]
  },
  {
    name: 'Accounts',
    scopes: [
      { 
        value: 'accounts:read', 
        name: 'View Accounts', 
        description: 'View account information', 
        dangerous: false 
      },
      { 
        value: 'accounts:write', 
        name: 'Manage Accounts', 
        description: 'Create and modify accounts', 
        dangerous: false 
      }
    ]
  },
  {
    name: 'Transactions',
    scopes: [
      { 
        value: 'transactions:read', 
        name: 'View Transactions', 
        description: 'View transaction history', 
        dangerous: false 
      },
      { 
        value: 'transactions:write', 
        name: 'Manage Transactions', 
        description: 'Create and modify transactions', 
        dangerous: false 
      }
    ]
  }
]

const applyTemplate = (template: typeof scopeTemplates[0]) => {
  selectedScopes.value = [...template.scopes]
}

const removeScope = (scope: string) => {
  selectedScopes.value = selectedScopes.value.filter(s => s !== scope)
}
</script>

<style scoped>
.scope-selector {
  @apply space-y-6;
}

.template-buttons {
  @apply flex flex-wrap gap-2;
}

.template-button {
  @apply flex items-center px-3 py-2 bg-gray-100 hover:bg-gray-200 
         text-gray-700 rounded-lg transition-colors;
}

.scope-groups {
  @apply space-y-4;
}

.scope-group {
  @apply border border-gray-200 rounded-lg p-4;
}

.group-title {
  @apply font-medium text-gray-900 mb-3;
}

.scope-checkboxes {
  @apply space-y-2;
}

.scope-checkbox {
  @apply flex items-start space-x-3 p-3 rounded-lg border 
         hover:bg-gray-50 cursor-pointer transition-colors;
}

.scope-checkbox.selected {
  @apply border-blue-300 bg-blue-50;
}

.scope-checkbox.dangerous {
  @apply border-red-200;
}

.scope-checkbox.dangerous.selected {
  @apply border-red-300 bg-red-50;
}

.scope-input {
  @apply mt-1;
}

.scope-info {
  @apply flex-1;
}

.scope-header {
  @apply flex items-center justify-between;
}

.scope-name {
  @apply font-medium text-gray-900;
}

.scope-description {
  @apply text-sm text-gray-500;
}

.danger-icon {
  @apply w-4 h-4 text-red-500;
}

.selected-scopes {
  @apply flex flex-wrap gap-2;
}

.selected-scope {
  @apply inline-flex items-center px-2 py-1 bg-blue-100 
         text-blue-800 rounded-md text-sm;
}

.remove-scope {
  @apply ml-1 text-blue-600 hover:text-blue-800 font-bold;
}

.error-message {
  @apply text-red-600 text-sm mt-2;
}
</style>
```

---

### 4. ApiKeyDetail Component

#### Purpose
Detail pane hiển thị thông tin chi tiết và analytics của API key.

#### Props
```typescript
interface ApiKeyDetailProps {
  apiKey: ApiKey | null
  loading?: boolean
}
```

#### Events
```typescript
interface ApiKeyDetailEvents {
  'edit': (keyId: string) => void
  'revoke': (keyId: string) => void
  'regenerate': (keyId: string) => void
  'close': () => void
}
```

#### Implementation
```vue
<template>
  <div v-if="apiKey" class="api-key-detail">
    <!-- Header -->
    <div class="detail-header">
      <div class="header-content">
        <h3 class="detail-title">{{ apiKey.name }}</h3>
        <StatusBadge :status="apiKey.status" />
      </div>
      <button @click="$emit('close')" class="close-button">
        <Icon name="x" class="w-5 h-5" />
      </button>
    </div>
    
    <!-- API Key Display -->
    <div class="key-display-section">
      <label class="section-label">API Key</label>
      <div class="key-container">
        <code class="masked-key">{{ maskedKey }}</code>
        <button @click="copyKey" class="copy-button">
          <Icon :name="copied ? 'check' : 'copy'" class="w-4 h-4" />
          {{ copied ? 'Copied!' : 'Copy' }}
        </button>
      </div>
      <p class="key-warning">
        Key has been masked for security. Only shown once during creation.
      </p>
    </div>
    
    <!-- Usage Overview -->
    <div class="usage-overview">
      <h4 class="section-title">Usage Overview</h4>
      <div class="usage-cards">
        <div class="usage-card">
          <span class="usage-label">Today</span>
          <span class="usage-value">
            {{ apiKey.todayUsageCount }}/{{ apiKey.dailyUsageQuota }}
          </span>
          <div class="usage-progress">
            <div 
              class="progress-bar" 
              :style="{ width: todayUsagePercentage + '%' }"
              :class="getUsageClass(todayUsagePercentage)"
            ></div>
          </div>
        </div>
        
        <div class="usage-card">
          <span class="usage-label">Rate Limit</span>
          <span class="usage-value">{{ apiKey.rateLimitPerMinute }}/min</span>
        </div>
        
        <div class="usage-card">
          <span class="usage-label">Last Used</span>
          <span class="usage-value">
            {{ apiKey.lastUsedAt ? formatRelativeTime(apiKey.lastUsedAt) : 'Never' }}
          </span>
        </div>
        
        <div class="usage-card">
          <span class="usage-label">Total Requests</span>
          <span class="usage-value">{{ apiKey.usageCount.toLocaleString() }}</span>
        </div>
      </div>
    </div>
    
    <!-- Configuration -->
    <div class="configuration-section">
      <h4 class="section-title">Configuration</h4>
      <div class="config-grid">
        <div class="config-item">
          <label class="config-label">Scopes</label>
          <div class="scopes-list">
            <ScopeBadge 
              v-for="scope in apiKey.scopes" 
              :key="scope"
              :scope="scope"
            />
          </div>
        </div>
        
        <div class="config-item">
          <label class="config-label">Expires</label>
          <span :class="getExpiryClass(apiKey.expiresAt)">
            {{ apiKey.expiresAt ? formatDate(apiKey.expiresAt) : 'Never' }}
          </span>
        </div>
        
        <div v-if="apiKey.ipWhitelist.length > 0" class="config-item">
          <label class="config-label">IP Whitelist</label>
          <div class="ip-list">
            <code 
              v-for="ip in apiKey.ipWhitelist" 
              :key="ip"
              class="ip-address"
            >
              {{ ip }}
            </code>
          </div>
        </div>
        
        <div class="config-item">
          <label class="config-label">Security</label>
          <div class="security-flags">
            <span 
              v-if="apiKey.securitySettings.requireHttps" 
              class="security-flag"
            >
              🔒 HTTPS Required
            </span>
            <span 
              v-if="apiKey.securitySettings.allowCorsRequests" 
              class="security-flag"
            >
              🌐 CORS Enabled
            </span>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Usage Analytics -->
    <div class="analytics-section">
      <h4 class="section-title">Usage Analytics</h4>
      <UsageChart :api-key-id="apiKey.id" />
    </div>
    
    <!-- Actions -->
    <div class="actions-section">
      <h4 class="section-title">Actions</h4>
      <div class="action-buttons">
        <button @click="$emit('edit', apiKey.id)" class="btn-primary">
          <Icon name="edit" class="w-4 h-4 mr-2" />
          Edit Settings
        </button>
        
        <button @click="$emit('regenerate', apiKey.id)" class="btn-secondary">
          <Icon name="refresh" class="w-4 h-4 mr-2" />
          Regenerate Key
        </button>
        
        <button @click="downloadConfig" class="btn-secondary">
          <Icon name="download" class="w-4 h-4 mr-2" />
          Download Config
        </button>
      </div>
      
      <!-- Danger Zone -->
      <div class="danger-zone">
        <h5 class="danger-title">Danger Zone</h5>
        <button @click="$emit('revoke', apiKey.id)" class="btn-danger">
          <Icon name="x-circle" class="w-4 h-4 mr-2" />
          Revoke API Key
        </button>
      </div>
    </div>
  </div>
  
  <!-- Loading State -->
  <div v-else-if="loading" class="detail-loading">
    <LoadingSpinner />
    <span>Loading API key details...</span>
  </div>
  
  <!-- Empty State -->
  <div v-else class="detail-empty">
    <Icon name="key" class="empty-icon" />
    <h3>Select an API key</h3>
    <p>Choose an API key from the list to view details</p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { formatRelativeTime, formatDate } from '@/utils/date'

const props = defineProps<ApiKeyDetailProps>()
const emit = defineEmits<ApiKeyDetailEvents>()

const copied = ref(false)

const maskedKey = computed(() => {
  if (!props.apiKey) return ''
  return `${props.apiKey.keyPrefix}${'*'.repeat(32)}`
})

const todayUsagePercentage = computed(() => {
  if (!props.apiKey) return 0
  return Math.round((props.apiKey.todayUsageCount / props.apiKey.dailyUsageQuota) * 100)
})

const copyKey = async () => {
  await navigator.clipboard.writeText(maskedKey.value)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}

const getUsageClass = (percentage: number) => {
  if (percentage >= 90) return 'bg-red-500'
  if (percentage >= 75) return 'bg-yellow-500'
  return 'bg-green-500'
}

const getExpiryClass = (expiresAt: string | null) => {
  if (!expiresAt) return 'text-gray-500'
  
  const expiry = new Date(expiresAt)
  const now = new Date()
  const daysUntilExpiry = Math.ceil((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
  
  if (daysUntilExpiry < 0) return 'text-red-600 font-semibold' // Expired
  if (daysUntilExpiry <= 7) return 'text-yellow-600 font-medium' // Expires soon
  return 'text-gray-700'
}

const downloadConfig = () => {
  const config = {
    name: props.apiKey?.name,
    keyPrefix: props.apiKey?.keyPrefix,
    scopes: props.apiKey?.scopes,
    baseUrl: 'https://api.tihomo.com',
    rateLimitPerMinute: props.apiKey?.rateLimitPerMinute,
    dailyUsageQuota: props.apiKey?.dailyUsageQuota
  }
  
  const blob = new Blob([JSON.stringify(config, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `tihomo-api-config-${props.apiKey?.name?.toLowerCase().replace(/\s+/g, '-')}.json`
  a.click()
  URL.revokeObjectURL(url)
}
</script>

<style scoped>
.api-key-detail {
  @apply space-y-6 p-6 bg-white border-l border-gray-200 h-full overflow-y-auto;
}

.detail-header {
  @apply flex justify-between items-start;
}

.header-content {
  @apply space-y-2;
}

.detail-title {
  @apply text-xl font-semibold text-gray-900;
}

.close-button {
  @apply p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100;
}

.section-label {
  @apply block text-sm font-medium text-gray-700 mb-2;
}

.section-title {
  @apply text-lg font-medium text-gray-900 mb-4;
}

.key-container {
  @apply flex items-center space-x-2 p-3 bg-gray-50 rounded-lg border;
}

.masked-key {
  @apply flex-1 font-mono text-sm;
}

.copy-button {
  @apply flex items-center px-3 py-1 text-sm bg-white border 
         border-gray-300 rounded hover:bg-gray-50 transition-colors;
}

.key-warning {
  @apply text-sm text-gray-500 mt-2;
}

.usage-cards {
  @apply grid grid-cols-2 gap-4;
}

.usage-card {
  @apply p-4 bg-gray-50 rounded-lg;
}

.usage-label {
  @apply block text-sm text-gray-600;
}

.usage-value {
  @apply block text-lg font-semibold text-gray-900 mt-1;
}

.usage-progress {
  @apply w-full bg-gray-200 rounded-full h-2 mt-2;
}

.progress-bar {
  @apply h-2 rounded-full transition-all duration-300;
}

.config-grid {
  @apply space-y-4;
}

.config-item {
  @apply space-y-2;
}

.config-label {
  @apply block text-sm font-medium text-gray-700;
}

.scopes-list {
  @apply flex flex-wrap gap-2;
}

.ip-list {
  @apply space-y-1;
}

.ip-address {
  @apply inline-block bg-gray-100 px-2 py-1 rounded text-sm font-mono;
}

.security-flags {
  @apply space-y-1;
}

.security-flag {
  @apply inline-block bg-green-100 text-green-800 px-2 py-1 rounded text-sm;
}

.action-buttons {
  @apply space-y-2;
}

.danger-zone {
  @apply mt-6 pt-6 border-t border-gray-200;
}

.danger-title {
  @apply text-sm font-medium text-red-600 mb-2;
}

.detail-loading,
.detail-empty {
  @apply flex flex-col items-center justify-center h-full text-center space-y-4;
}

.empty-icon {
  @apply w-12 h-12 text-gray-400;
}
</style>
```

---

## 🎨 Utility Components

### StatusBadge Component
```vue
<template>
  <span 
    class="status-badge"
    :class="statusClasses"
  >
    {{ statusText }}
  </span>
</template>

<script setup lang="ts">
interface Props {
  status: 'active' | 'revoked' | 'expired'
}

const props = defineProps<Props>()

const statusClasses = computed(() => {
  switch (props.status) {
    case 'active':
      return 'bg-green-100 text-green-800'
    case 'revoked':
      return 'bg-red-100 text-red-800'
    case 'expired':
      return 'bg-yellow-100 text-yellow-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
})

const statusText = computed(() => {
  switch (props.status) {
    case 'active':
      return 'Active'
    case 'revoked':
      return 'Revoked'
    case 'expired':
      return 'Expired'
    default:
      return 'Unknown'
  }
})
</script>

<style scoped>
.status-badge {
  @apply inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium;
}
</style>
```

### ScopeBadge Component
```vue
<template>
  <span 
    class="scope-badge"
    :class="scopeClasses"
  >
    {{ scope }}
  </span>
</template>

<script setup lang="ts">
interface Props {
  scope: string
}

const props = defineProps<Props>()

const scopeClasses = computed(() => {
  if (props.scope.includes('admin')) {
    return 'bg-purple-100 text-purple-800'
  }
  if (props.scope.includes('delete')) {
    return 'bg-red-100 text-red-800'
  }
  if (props.scope.includes('write')) {
    return 'bg-orange-100 text-orange-800'
  }
  if (props.scope.includes('read')) {
    return 'bg-blue-100 text-blue-800'
  }
  return 'bg-gray-100 text-gray-800'
})
</script>

<style scoped>
.scope-badge {
  @apply inline-flex items-center px-2 py-1 rounded-md text-xs font-medium;
}
</style>
```

---

## 📱 Responsive Design

### Breakpoint Behavior
```css
/* Mobile First Approach */
.api-key-list {
  @apply block;
}

/* Tablet và Desktop */
@media (min-width: 768px) {
  .api-key-list.with-detail {
    @apply grid grid-cols-2 gap-6;
  }
}

/* Large Desktop */
@media (min-width: 1024px) {
  .api-key-list.with-detail {
    @apply grid-cols-5;
  }
  
  .api-key-table {
    @apply col-span-3;
  }
  
  .api-key-detail {
    @apply col-span-2;
  }
}
```

### Mobile Optimizations
```css
/* Mobile Table Optimizations */
@media (max-width: 767px) {
  .api-key-table {
    @apply text-sm;
  }
  
  .api-key-row {
    @apply block border-b border-gray-200 p-4;
  }
  
  .api-key-row td {
    @apply block py-1;
  }
  
  .api-key-row td:first-child {
    @apply font-medium;
  }
  
  .api-key-row td:before {
    content: attr(data-label) ": ";
    @apply font-medium text-gray-600;
  }
}
```

---

## ♿ Accessibility Features

### ARIA Labels và Roles
```vue
<template>
  <!-- Table với proper ARIA labels -->
  <table 
    class="api-key-table"
    role="table"
    aria-label="API Keys list"
  >
    <thead role="rowgroup">
      <tr role="row">
        <th role="columnheader" aria-sort="none">Name</th>
        <th role="columnheader">Status</th>
        <!-- ... -->
      </tr>
    </thead>
    <tbody role="rowgroup">
      <tr 
        v-for="apiKey in apiKeys"
        :key="apiKey.id"
        role="row"
        :aria-selected="selectedKeyId === apiKey.id"
        @click="selectKey(apiKey.id)"
        @keydown.enter="selectKey(apiKey.id)"
        @keydown.space.prevent="selectKey(apiKey.id)"
        tabindex="0"
      >
        <!-- ... -->
      </tr>
    </tbody>
  </table>
  
  <!-- Form với proper labels -->
  <form role="form" aria-labelledby="create-api-key-title">
    <h2 id="create-api-key-title">Create API Key</h2>
    
    <div role="group" aria-labelledby="basic-info-title">
      <h3 id="basic-info-title">Basic Information</h3>
      <!-- Form fields với proper labels -->
    </div>
  </form>
</template>
```

### Keyboard Navigation
```typescript
// Keyboard event handlers
const handleKeyDown = (event: KeyboardEvent) => {
  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault()
      selectNextKey()
      break
    case 'ArrowUp':
      event.preventDefault()
      selectPreviousKey()
      break
    case 'Enter':
    case ' ':
      event.preventDefault()
      if (selectedKeyId.value) {
        openKeyDetail(selectedKeyId.value)
      }
      break
    case 'Escape':
      closeDetail()
      break
  }
}
```

---

## 🧪 Testing Guidelines

### Component Testing
```typescript
// ApiKeyList.test.ts
import { mount } from '@vue/test-utils'
import ApiKeyList from '@/components/ApiKeyList.vue'

describe('ApiKeyList', () => {
  it('renders API keys correctly', () => {
    const apiKeys = [
      {
        id: '1',
        name: 'Test Key',
        status: 'active',
        scopes: ['read', 'write']
      }
    ]
    
    const wrapper = mount(ApiKeyList, {
      props: { apiKeys }
    })
    
    expect(wrapper.text()).toContain('Test Key')
    expect(wrapper.find('[data-testid="status-badge"]').text()).toBe('Active')
  })
  
  it('emits select-key event when row clicked', async () => {
    const wrapper = mount(ApiKeyList, {
      props: { apiKeys: [mockApiKey] }
    })
    
    await wrapper.find('[data-testid="api-key-row"]').trigger('click')
    
    expect(wrapper.emitted('select-key')).toBeTruthy()
    expect(wrapper.emitted('select-key')[0]).toEqual(['1'])
  })
})
```

### Integration Testing
```typescript
// api-key-management.e2e.ts
describe('API Key Management', () => {
  it('creates new API key successfully', () => {
    cy.visit('/settings/api-keys')
    cy.get('[data-testid="create-api-key-btn"]').click()
    
    // Fill form
    cy.get('[data-testid="name-input"]').type('Test API Key')
    cy.get('[data-testid="scope-read"]').check()
    cy.get('[data-testid="submit-btn"]').click()
    
    // Verify success
    cy.get('[data-testid="success-modal"]').should('be.visible')
    cy.get('[data-testid="api-key-value"]').should('contain', 'pfm_')
  })
})
```

---

Các components này tạo nên foundation cho API Key Management feature với focus vào security, usability, và accessibility. Tất cả components đều follow design system và có thể reuse trong các parts khác của application. 