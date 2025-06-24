<template>
  <div class="relative">
    <label v-if="label" class="block text-sm font-medium mb-2">{{ label }}</label>
    <select 
      :value="modelValue" 
      @change="handleChange"
      :disabled="disabled"
      :class="[
        'form-select w-full',
        disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
        error ? 'border-red-500' : ''
      ]"
    >
      <!-- Option "Tất cả tài khoản" -->
      <option value="">{{ allAccountsText }}</option>
      
      <!-- Danh sách accounts -->
      <option 
        v-for="account in availableAccounts" 
        :key="account.id" 
        :value="account.id"
        :disabled="!account.isActive"
      >
        {{ account.name }}
        <span v-if="showBalance && account.currentBalance !== null">
          ({{ formatCurrency(account.currentBalance) }})
        </span>
      </option>
    </select>
    
    <!-- Error message -->
    <p v-if="error" class="mt-1 text-sm text-red-500">
      {{ error }}
    </p>
    
    <!-- Helper text -->
    <p v-if="helperText && !error" class="mt-1 text-sm text-gray-500">
      {{ helperText }}
    </p>
  </div>
</template>

<script setup lang="ts">
// Types cho AccountDropdown component
type Account = {
  id: string
  name: string
  currentBalance: number | null
  isActive: boolean
  type?: string
}

type AccountDropdownProps = {
  modelValue: string | null
  accounts: Account[]
  label?: string
  allAccountsText?: string
  showBalance?: boolean
  disabled?: boolean
  error?: string
  helperText?: string
  filterActiveOnly?: boolean
}

type AccountDropdownEmits = {
  'update:modelValue': [value: string | null]
  'change': [value: string | null, account: Account | null]
}

// Props với default values
const props = withDefaults(defineProps<AccountDropdownProps>(), {
  label: '',
  allAccountsText: 'Tất cả tài khoản',
  showBalance: false,
  disabled: false,
  error: '',
  helperText: '',
  filterActiveOnly: true
})

// Emits
const emit = defineEmits<AccountDropdownEmits>()

// Computed properties
const availableAccounts = computed(() => {
  let filtered = props.accounts
  
  // Filter active accounts nếu filterActiveOnly = true
  if (props.filterActiveOnly) {
    filtered = filtered.filter(account => account.isActive)
  }
  
  // Sort by name
  return filtered.sort((a, b) => a.name.localeCompare(b.name))
})

const selectedAccount = computed(() => {
  if (!props.modelValue) return null
  return props.accounts.find(account => account.id === props.modelValue) || null
})

// Methods
function handleChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const value = target.value || null
  const account = value ? props.accounts.find(a => a.id === value) || null : null
  
  // Emit update:modelValue cho v-model
  emit('update:modelValue', value)
  
  // Emit change event với additional info
  emit('change', value, account)
}

function formatCurrency(amount: number | null): string {
  if (amount === null) return '-'
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    minimumFractionDigits: 0
  }).format(amount)
}

// Expose methods cho parent component
defineExpose({
  selectedAccount,
  availableAccounts
})
</script>

<style scoped>
/* Component-specific styles */
.form-select {
  display: block;
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  background-color: white;
  color: #374151;
}

.form-select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 1px #3b82f6;
}

.form-select:disabled {
  background-color: #f3f4f6;
  cursor: not-allowed;
}

.dark .form-select {
  background-color: #374151;
  border-color: #4b5563;
  color: white;
}

.dark .form-select:disabled {
  background-color: #1f2937;
}

option:disabled {
  color: #9ca3af;
}
</style>
