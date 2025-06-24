<template>
  <div>    <!-- Page Header -->
    <div class="flex items-center justify-between mb-5">
      <h5 class="font-semibold text-lg dark:text-white-light">{{ pageTitle }}</h5>
      
      <!-- Breadcrumb Navigation -->
      <NavigationBreadcrumb 
        :items="breadcrumbs"
        @item-click="handleBreadcrumbClick"
      />
    </div>

    <!-- Filters Section -->
    <div class="panel mb-5">
      <div class="flex flex-col sm:flex-row gap-4 mb-5">        <!-- Account Filter -->
        <div class="sm:w-1/3">
          <AccountDropdown
            v-model="selectedAccountId"
            :accounts="accountOptions"
            label="Tài khoản"
            all-accounts-text="Tất cả tài khoản"
            :show-balance="true"
            @change="onAccountFilterChange"
          />
        </div>

        <!-- Date Range Filter -->
        <div class="sm:w-1/3">
          <label class="block text-sm font-medium mb-2">Từ ngày</label>
          <input 
            v-model="dateFrom" 
            type="date" 
            class="form-input"
            @change="onDateRangeChange"
          />
        </div>
        <div class="sm:w-1/3">
          <label class="block text-sm font-medium mb-2">Đến ngày</label>
          <input 
            v-model="dateTo" 
            type="date" 
            class="form-input"
            @change="onDateRangeChange"
          />
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex gap-2 mb-4">        <button 
          @click="openAddTransactionForm('Revenue')"
          class="btn btn-success"
        >
          <span class="mr-2">+</span>
          Giao dịch Thu
        </button>
        <button 
          @click="openAddTransactionForm('Spent')"
          class="btn btn-danger"
        >
          <span class="mr-2">-</span>
          Giao dịch Chi
        </button>
      </div>
    </div>

    <!-- Main Content Layout -->
    <div class="grid grid-cols-1 gap-5" :class="{ 'lg:grid-cols-2': isDetailPaneOpen }">      <!-- Transaction List -->
      <div class="panel" :class="{ 'lg:col-span-1': isDetailPaneOpen }">
        <TransactionList
          :transactions="transactions"
          :loading="isLoading"
          :title="'Danh sách giao dịch'"
          @transaction-select="selectTransaction"
          @transaction-edit="editTransaction"
          @add-transaction="openAddTransactionForm"
          @refresh="refreshTransactions"        />
      </div>

      <!-- Transaction Detail Pane -->
      <div v-if="isDetailPaneOpen" class="panel lg:col-span-1">
        <div class="flex items-center justify-between mb-5">
          <h6 class="text-lg font-semibold">
            {{ isAddMode ? 'Thêm giao dịch' : 'Chi tiết giao dịch' }}
          </h6>          <button 
            @click="closeDetailPane"
            class="btn btn-sm btn-outline-danger"
          >
            <span class="w-4 h-4">✕</span>
          </button>
        </div>

        <!-- Transaction Form/Detail Component would go here -->
        <div class="space-y-4">
          <p class="text-gray-600">Transaction detail/form component will be implemented here</p>
          <div v-if="selectedTransaction">
            <pre class="text-xs bg-gray-100 p-4 rounded">{{ JSON.stringify(selectedTransaction, null, 2) }}</pre>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useTransactionFilterStore } from '@/stores/transactionFilter'

// Date utilities (thay vì date-fns để tránh dependencies)
function subDays(date: Date, days: number): Date {
  const result = new Date(date)
  result.setDate(result.getDate() - days)
  return result
}

function formatDate(date: Date, format: 'yyyy-MM-dd' | 'dd/MM/yyyy' | 'dd/MM HH:mm'): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  
  switch (format) {
    case 'yyyy-MM-dd':
      return `${year}-${month}-${day}`
    case 'dd/MM/yyyy':
      return `${day}/${month}/${year}`
    case 'dd/MM HH:mm':
      return `${day}/${month} ${hours}:${minutes}`
    default:
      return date.toISOString()
  }
}

// Types - Transaction Navigation Filtering Feature
type RouteQuery = {
  accountId?: string
  accountName?: string
}

type TransactionDirection = 'Revenue' | 'Spent'

type Transaction = {
  id: string
  transactionDate: string
  description: string
  revenueAmount: number
  spentAmount: number
  balance: number | null
  accountId: string
  accountName: string
}

type Account = {
  id: string
  name: string
  currentBalance: number
  isActive: boolean
}

// Composables và stores
const route = useRoute()
const router = useRouter()
const transactionFilterStore = useTransactionFilterStore()

// Reactive data
const isLoading = ref(false)
const isDetailPaneOpen = ref(false)
const isAddMode = ref(false)
const selectedTransactionId = ref<string | null>(null)
const selectedTransaction = ref<Transaction | null>(null)

// Filter states - bound to store
const selectedAccountId = computed({
  get: () => transactionFilterStore.selectedAccountId,
  set: (value) => transactionFilterStore.setAccountFilter(value, getAccountNameById(value))
})

const dateFrom = computed({
  get: () => formatDate(transactionFilterStore.dateFrom, 'yyyy-MM-dd'),
  set: (value) => transactionFilterStore.setDateRange(new Date(value), transactionFilterStore.dateTo)
})

const dateTo = computed({
  get: () => formatDate(transactionFilterStore.dateTo, 'yyyy-MM-dd'),
  set: (value) => transactionFilterStore.setDateRange(transactionFilterStore.dateFrom, new Date(value))
})

// Mock data - will be replaced with API calls
const transactions = ref<Transaction[]>([])
const accountOptions = ref<Account[]>([
  { id: '1', name: 'Techcombank', currentBalance: 5000000, isActive: true },
  { id: '2', name: 'VCB', currentBalance: 2000000, isActive: true },
  { id: '3', name: 'ACB', currentBalance: 1500000, isActive: true }
])

// Computed properties
const pageTitle = computed(() => {
  const accountName = transactionFilterStore.selectedAccountName
  return accountName && accountName !== 'Tất cả tài khoản' 
    ? `Giao dịch - ${accountName}` 
    : 'Giao dịch'
})

const breadcrumbs = computed(() => {
  const { accountId, accountName } = route.query as RouteQuery
  
  if (accountId && accountName) {
    // Navigation from Account page
    return [
      { name: 'Dashboard', path: '/' },
      { name: 'Accounts', path: '/accounts' },
      { name: accountName, path: `/accounts?highlight=${accountId}` },
      { name: 'Transactions', path: '' }
    ]
  } else {
    // Direct navigation from menu
    return [
      { name: 'Dashboard', path: '/' },
      { name: 'Transactions', path: '' }
    ]
  }
})

// Methods
function getAccountNameById(accountId: string | null): string {
  if (!accountId) return 'Tất cả tài khoản'
  const account = accountOptions.value.find(a => a.id === accountId)
  return account?.name || 'Tất cả tài khoản'
}

async function handleNavigationContext() {
  const { accountId, accountName } = route.query as RouteQuery
  
  if (accountId && accountName) {
    // Case 1: Navigation from Account page
    await handleAccountNavigation(accountId, accountName)
  } else {
    // Case 2: Direct navigation from menu
    await handleDirectNavigation()
  }
}

async function handleAccountNavigation(accountId: string, accountName: string) {
  // Set filter state
  transactionFilterStore.setAccountFilter(accountId, accountName)
  
  // Set page title via head
  useHead({
    title: `Giao dịch - ${accountName}`
  })
  
  // Load transactions for specific account (30 days)
  await loadTransactions({
    accountId: accountId,
    dateFrom: subDays(new Date(), 30),
    dateTo: new Date()
  })
}

async function handleDirectNavigation() {
  // Set default filter state
  transactionFilterStore.setAccountFilter(null, 'Tất cả tài khoản')
  
  // Set page title
  useHead({
    title: 'Giao dịch'
  })
  
  // Load all transactions (30 days)
  await loadTransactions({
    accountId: null,
    dateFrom: subDays(new Date(), 30),
    dateTo: new Date()
  })
}

async function loadTransactions(filters: {
  accountId: string | null
  dateFrom: Date
  dateTo: Date
}) {
  isLoading.value = true
  
  try {
    // TODO: Replace with actual API call
    // const response = await $fetch('/api/core-finance/transaction', { query: filters })
    
    // Mock data for now
    await new Promise(resolve => setTimeout(resolve, 1000)) // Simulate loading
    
    transactions.value = [
      {
        id: '1',
        transactionDate: new Date().toISOString(),
        description: 'Mua sắm tại Vinmart',
        revenueAmount: 0,
        spentAmount: 250000,
        balance: 4750000,
        accountId: '1',
        accountName: 'Techcombank'
      },
      // Add more mock data as needed
    ]
  } catch (error) {
    console.error('Error loading transactions:', error)
    // TODO: Show error toast
  } finally {
    isLoading.value = false
  }
}

function onAccountFilterChange() {
  // Update URL to reflect current selection
  const accountId = selectedAccountId.value
  if (accountId) {
    const accountName = getAccountNameById(accountId)
    router.replace({
      query: {
        ...route.query,
        accountId: accountId,
        accountName: accountName
      }
    })
  } else {
    // Remove account params from URL
    const { accountId: _, accountName: __, ...restQuery } = route.query
    router.replace({ query: restQuery })
  }
  
  // Reload transactions with new filter
  loadTransactions({
    accountId: selectedAccountId.value,
    dateFrom: transactionFilterStore.dateFrom,
    dateTo: transactionFilterStore.dateTo
  })
}

function onDateRangeChange() {
  // Reload transactions with new date range
  loadTransactions({
    accountId: selectedAccountId.value,
    dateFrom: transactionFilterStore.dateFrom,
    dateTo: transactionFilterStore.dateTo
  })
}

function openAddTransactionForm(direction: TransactionDirection) {
  isAddMode.value = true
  isDetailPaneOpen.value = true
  selectedTransaction.value = null
  selectedTransactionId.value = null
  
  // TODO: Pre-select direction and account in form
  console.log('Opening add form with direction:', direction)
}

function selectTransaction(transaction: Transaction) {
  isAddMode.value = false
  isDetailPaneOpen.value = true
  selectedTransaction.value = transaction
  selectedTransactionId.value = transaction.id
}

function closeDetailPane() {
  isDetailPaneOpen.value = false
  isAddMode.value = false
  selectedTransaction.value = null
  selectedTransactionId.value = null
}

function formatDateTime(dateString: string): string {
  return formatDate(new Date(dateString), 'dd/MM HH:mm')
}

function formatAmount(transaction: Transaction): string {
  if (transaction.revenueAmount > 0) {
    return `+${formatCurrency(transaction.revenueAmount)}`
  } else {
    return `-${formatCurrency(transaction.spentAmount)}`
  }
}

function formatCurrency(amount: number | null): string {
  if (amount === null) return '-'
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

function getAmountColorClass(transaction: Transaction): string {
  return transaction.revenueAmount > 0 ? 'text-success' : 'text-danger'
}

function handleBreadcrumbClick(item: any, index: number) {
  console.log('Breadcrumb clicked:', item, index)
  // Navigation handling is already done by NavigationBreadcrumb component
}

function editTransaction(transaction: Transaction) {
  // Open transaction in edit mode
  selectedTransaction.value = transaction
  selectedTransactionId.value = transaction.id
  isAddMode.value = false
  isDetailPaneOpen.value = true
}

async function refreshTransactions() {
  isLoading.value = true
  try {
    // In a real app, this would refetch data from API
    // For now, we'll just toggle loading state
    await new Promise(resolve => setTimeout(resolve, 1000))
    console.log('Transactions refreshed')
  } finally {
    isLoading.value = false
  }
}

// Lifecycle
onMounted(async () => {
  await handleNavigationContext()
})

// Handle browser back/forward navigation
watch(() => route.query, async () => {
  await handleNavigationContext()
})

// SEO và Meta
useHead({
  title: pageTitle.value
})
</script>

<style scoped>
/* Component-specific styles if needed */
.table-responsive {
  overflow-x: auto;
}

.table-hover tbody tr:hover {
  background-color: rgb(249 250 251);
}

.dark .table-hover tbody tr:hover {
  background-color: rgb(31 41 55);
}
</style>
