<template>
  <div>
    <!-- Page Header -->
    <div class="flex items-center justify-between mb-5">
      <h5 class="font-semibold text-lg dark:text-white-light">Quản lý tài khoản</h5>

      <!-- Breadcrumb -->
      <NavigationBreadcrumb :items="breadcrumbs" />
    </div>

    <!-- Account Actions -->
    <div class="panel mb-5">
      <div class="flex gap-2 mb-4">
        <button class="btn btn-primary">
          <span class="mr-2">+</span>
          Thêm tài khoản
        </button>
        <button class="btn btn-secondary">
          <span class="mr-2">↻</span>
          Làm mới
        </button>
      </div>
    </div>

    <!-- Accounts Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
      <div v-for="account in accounts" :key="account.id" :class="[
        'panel cursor-pointer transition-all duration-200 hover:shadow-lg',
        highlightAccountId === account.id ? 'ring-2 ring-primary bg-primary/5' : ''
      ]" @click="navigateToTransactions(account)">
        <!-- Account Header -->
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center space-x-3">
            <div :class="[
              'w-10 h-10 rounded-full flex items-center justify-center text-white font-semibold',
              getAccountTypeColor(account.type)
            ]">
              {{ getAccountInitials(account.name) }}
            </div>
            <div>
              <h6 class="font-semibold text-base">{{ account.name }}</h6>
              <p class="text-sm text-gray-500">{{ getAccountTypeName(account.type) }}</p>
            </div>
          </div>

          <!-- Status Badge -->
          <span :class="[
            'px-2 py-1 text-xs rounded-full font-medium',
            account.isActive
              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300'
              : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'
          ]">
            {{ account.isActive ? 'Hoạt động' : 'Tạm khóa' }}
          </span>
        </div>

        <!-- Account Details -->
        <div class="space-y-3">
          <!-- Current Balance -->
          <div class="flex justify-between items-center">
            <span class="text-sm text-gray-600">Số dư hiện tại:</span>
            <span :class="[
              'font-semibold',
              account.currentBalance >= 0 ? 'text-green-600' : 'text-red-600'
            ]">
              {{ formatCurrency(account.currentBalance) }}
            </span>
          </div>

          <!-- Available Limit (if applicable) -->
          <div v-if="account.availableLimit !== null" class="flex justify-between items-center">
            <span class="text-sm text-gray-600">Hạn mức khả dụng:</span>
            <span class="font-medium text-blue-600">
              {{ formatCurrency(account.availableLimit) }}
            </span>
          </div>

          <!-- Last Transaction Date -->
          <div v-if="account.lastTransactionDate" class="flex justify-between items-center">
            <span class="text-sm text-gray-600">Giao dịch cuối:</span>
            <span class="text-sm text-gray-500">
              {{ formatDate(account.lastTransactionDate, 'dd/MM/yyyy') }}
            </span>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex gap-2 mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
          <button @click.stop="navigateToTransactions(account)" class="flex-1 btn btn-sm btn-outline-primary">
            Xem giao dịch
          </button>
          <button @click.stop="editAccount(account)" class="btn btn-sm btn-outline-secondary">
            Sửa
          </button>
          <button @click.stop="showAccountDetails(account)" class="btn btn-sm btn-outline-info">
            Chi tiết
          </button>
        </div>
      </div>

      <!-- Add Account Card -->
      <div
        class="panel border-2 border-dashed border-gray-300 dark:border-gray-600 flex items-center justify-center min-h-64 cursor-pointer hover:border-primary transition-colors">
        <div class="text-center">
          <div
            class="w-12 h-12 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-3">
            <span class="text-2xl text-gray-400">+</span>
          </div>
          <p class="text-gray-500 mb-2">Thêm tài khoản mới</p>
          <button class="btn btn-sm btn-primary">Tạo tài khoản</button>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="accounts.length === 0" class="text-center py-20">
      <div class="w-20 h-20 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-4">
        <span class="text-3xl">🏦</span>
      </div>
      <h3 class="text-lg font-semibold mb-2">Chưa có tài khoản nào</h3>
      <p class="text-gray-500 mb-6">Tạo tài khoản đầu tiên để bắt đầu quản lý tài chính</p>
      <button class="btn btn-primary">
        <span class="mr-2">+</span>
        Tạo tài khoản đầu tiên
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
// Types cho Account Management
type AccountType = 'bank' | 'credit' | 'savings' | 'investment' | 'cash'

type Account = {
  id: string
  name: string
  type: AccountType
  currentBalance: number
  availableLimit: number | null
  isActive: boolean
  lastTransactionDate: string | null
  accountNumber?: string
  bankName?: string
  description?: string
}

// Composables
const route = useRoute()
const router = useRouter()

// Reactive data
const accounts = ref<Account[]>([
  {
    id: '1',
    name: 'Techcombank',
    type: 'bank',
    currentBalance: 5000000,
    availableLimit: null,
    isActive: true,
    lastTransactionDate: '2024-06-20',
    accountNumber: '19036661234567',
    bankName: 'Techcombank'
  },
  {
    id: '2',
    name: 'VCB Credit',
    type: 'credit',
    currentBalance: 2000000,
    availableLimit: 50000000,
    isActive: true,
    lastTransactionDate: '2024-06-18',
    accountNumber: '4111111111111111',
    bankName: 'Vietcombank'
  },
  {
    id: '3',
    name: 'ACB Savings',
    type: 'savings',
    currentBalance: 15000000,
    availableLimit: null,
    isActive: true,
    lastTransactionDate: '2024-06-15',
    accountNumber: '123456789',
    bankName: 'ACB'
  },
  {
    id: '4',
    name: 'Tiền mặt',
    type: 'cash',
    currentBalance: 500000,
    availableLimit: null,
    isActive: true,
    lastTransactionDate: '2024-06-24'
  }
])

// Computed properties
const highlightAccountId = computed(() => {
  return route.query.highlight as string || null
})

const breadcrumbs = computed(() => [
  { name: 'Dashboard', path: '/' },
  { name: 'Accounts' }
])

// Date utility function
function formatDate(dateString: string, format: 'dd/MM/yyyy' | 'dd/MM HH:mm'): string {
  const date = new Date(dateString)
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')

  switch (format) {
    case 'dd/MM/yyyy':
      return `${day}/${month}/${year}`
    case 'dd/MM HH:mm':
      return `${day}/${month} ${hours}:${minutes}`
    default:
      return date.toLocaleDateString('vi-VN')
  }
}

// Methods
function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    minimumFractionDigits: 0
  }).format(amount)
}

function getAccountInitials(name: string): string {
  return name.split(' ')
    .map(word => word.charAt(0))
    .join('')
    .toUpperCase()
    .slice(0, 2)
}

function getAccountTypeColor(type: AccountType): string {
  const colors = {
    bank: 'bg-blue-500',
    credit: 'bg-orange-500',
    savings: 'bg-green-500',
    investment: 'bg-purple-500',
    cash: 'bg-gray-500'
  }
  return colors[type] || 'bg-gray-500'
}

function getAccountTypeName(type: AccountType): string {
  const types = {
    bank: 'Tài khoản ngân hàng',
    credit: 'Thẻ tín dụng',
    savings: 'Tài khoản tiết kiệm',
    investment: 'Tài khoản đầu tư',
    cash: 'Tiền mặt'
  }
  return types[type] || 'Không xác định'
}

/**
 * Navigate đến Transaction page với account context
 * Implementation của Task #TNF-F004
 */
function navigateToTransactions(account: Account) {
  router.push({
    path: '/transactions',
    query: {
      accountId: account.id,
      accountName: account.name
    }
  })
}

function editAccount(account: Account) {
  console.log('Edit account:', account)
  // TODO: Implement edit account functionality
}

function showAccountDetails(account: Account) {
  console.log('Show account details:', account)
  // TODO: Implement account details modal/page
}

// SEO và Meta
useHead({
  title: 'Quản lý tài khoản - TiHoMo'
})
</script>

<style scoped>
/* Component-specific styles */
.panel {
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  border: 1px solid #e5e7eb;
}

.dark .panel {
  background: #1f2937;
  border-color: #374151;
  color: white;
}

/* Button styles */
.btn {
  padding: 0.5rem 1rem;
  border-radius: 0.375rem;
  font-weight: 500;
  transition: all 0.2s;
  border: 1px solid transparent;
  cursor: pointer;
}

.btn-primary {
  background-color: #3b82f6;
  color: white;
}

.btn-primary:hover {
  background-color: #2563eb;
}

.btn-secondary {
  background-color: #6b7280;
  color: white;
}

.btn-outline-primary {
  border-color: #3b82f6;
  color: #3b82f6;
  background: transparent;
}

.btn-outline-primary:hover {
  background-color: #3b82f6;
  color: white;
}

.btn-outline-secondary {
  border-color: #6b7280;
  color: #6b7280;
  background: transparent;
}

.btn-outline-info {
  border-color: #06b6d4;
  color: #06b6d4;
  background: transparent;
}

.btn-sm {
  padding: 0.25rem 0.75rem;
  font-size: 0.875rem;
}
</style>
