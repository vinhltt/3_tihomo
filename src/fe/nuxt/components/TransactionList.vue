<template>
  <div class="transaction-list">
    <!-- List Header -->
    <div class="flex items-center justify-between mb-4">
      <h6 class="text-lg font-semibold">{{ title }}</h6>
      
      <!-- View Options -->
      <div class="flex items-center gap-2">
        <button 
          @click="toggleViewMode"
          class="btn btn-sm btn-outline-secondary"
          :title="isCompactView ? 'Chế độ chi tiết' : 'Chế độ đơn giản'"
        >
          {{ isCompactView ? '📋' : '📄' }}
        </button>
        
        <button 
          @click="refreshList"
          :disabled="loading"
          class="btn btn-sm btn-outline-primary"
          title="Làm mới danh sách"
        >
          <span :class="{ 'animate-spin': loading }">↻</span>
        </button>
      </div>
    </div>    <!-- Loading State -->
    <div v-if="loading" class="space-y-4">
      <div v-for="n in skeletonRows" :key="n" class="animate-pulse">
        <div class="flex items-center space-x-4 p-4 border-b border-gray-200 dark:border-gray-700">
          <div class="w-16 h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
          <div class="flex-1 space-y-2">
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
            <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
          </div>
          <div class="w-20 h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
          <div class="w-24 h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
          <div class="w-16 h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
        </div>
      </div>
    </div>

    <!-- Transaction Table -->
    <div v-else-if="displayTransactions.length > 0" class="table-responsive">
      <table class="table-hover w-full">
        <thead class="bg-gray-50 dark:bg-gray-800">
          <tr>
            <!-- Always visible columns -->
            <th @click="sortBy('transactionDate')" class="sortable-header">
              Ngày giờ
              <span v-if="sortField === 'transactionDate'">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('description')" class="sortable-header">
              Mô tả
              <span v-if="sortField === 'description'">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th @click="sortBy('amount')" class="sortable-header text-right">
              Số tiền
              <span v-if="sortField === 'amount'">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            <th v-if="!isCompactView" @click="sortBy('balance')" class="sortable-header text-right">
              Số dư
              <span v-if="sortField === 'balance'">
                {{ sortDirection === 'asc' ? '↑' : '↓' }}
              </span>
            </th>
            
            <!-- Extended columns (advanced mode) -->
            <th v-if="!isCompactView && showAccountColumn">Tài khoản</th>
            <th v-if="!isCompactView">Loại</th>
            <th v-if="!isCompactView">Ghi chú</th>
            
            <th class="w-20">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="transaction in displayTransactions" 
            :key="transaction.id"
            :class="getRowClass(transaction)"
            @click="selectTransaction(transaction)"
            class="cursor-pointer transition-colors"
          >
            <!-- Date Time -->
            <td class="font-medium">
              {{ formatDateTime(transaction.transactionDate) }}
            </td>
            
            <!-- Description -->
            <td>
              <div class="max-w-xs truncate" :title="transaction.description">
                {{ transaction.description }}
              </div>
            </td>
            
            <!-- Amount -->
            <td class="text-right font-semibold" :class="getAmountColorClass(transaction)">
              {{ formatAmount(transaction) }}
            </td>
            
            <!-- Balance (if not compact) -->
            <td v-if="!isCompactView" class="text-right">
              {{ formatCurrency(transaction.balance) }}
            </td>
            
            <!-- Account (if showing multiple accounts) -->
            <td v-if="!isCompactView && showAccountColumn">
              <span class="text-sm bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded">
                {{ transaction.accountName }}
              </span>
            </td>
            
            <!-- Category Type -->
            <td v-if="!isCompactView">
              <span v-if="transaction.categoryType" class="text-sm text-gray-600">
                {{ transaction.categoryType }}
              </span>
              <span v-else class="text-sm text-gray-400">-</span>
            </td>
            
            <!-- Note -->
            <td v-if="!isCompactView">
              <div v-if="transaction.note" class="max-w-xs truncate text-sm text-gray-600" :title="transaction.note">
                {{ transaction.note }}
              </div>
              <span v-else class="text-sm text-gray-400">-</span>
            </td>
            
            <!-- Actions -->
            <td>
              <div class="flex gap-1">
                <button 
                  @click.stop="selectTransaction(transaction)"
                  class="btn btn-xs btn-outline-primary"
                  title="Xem chi tiết"
                >
                  👁️
                </button>
                <button 
                  @click.stop="editTransaction(transaction)"
                  class="btn btn-xs btn-outline-secondary"
                  title="Chỉnh sửa"
                >
                  ✏️
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-12">
      <div class="text-6xl mb-4">📄</div>
      <h3 class="text-lg font-semibold mb-2">{{ emptyStateTitle }}</h3>
      <p class="text-gray-500 mb-6">{{ emptyStateMessage }}</p>
      <slot name="empty-state-actions">
        <button @click="$emit('add-transaction')" class="btn btn-primary">
          <span class="mr-2">+</span>
          Tạo giao dịch đầu tiên
        </button>
      </slot>
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="flex items-center justify-between mt-6">
      <div class="text-sm text-gray-600">
        Hiển thị {{ startIndex + 1 }} - {{ endIndex }} của {{ totalItems }} giao dịch
      </div>
      
      <div class="flex gap-2">
        <button 
          @click="previousPage"
          :disabled="currentPage === 1"
          class="btn btn-sm btn-outline-secondary"
        >
          ← Trước
        </button>
        
        <span class="flex items-center px-3 py-1 text-sm">
          Trang {{ currentPage }} / {{ totalPages }}
        </span>
        
        <button 
          @click="nextPage"
          :disabled="currentPage === totalPages"
          class="btn btn-sm btn-outline-secondary"
        >
          Sau →
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Types
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
  categoryType?: string
  note?: string
}

type SortField = 'transactionDate' | 'description' | 'amount' | 'balance'
type SortDirection = 'asc' | 'desc'

type TransactionListProps = {
  transactions: Transaction[]
  loading?: boolean
  selectedTransactionId?: string | null
  title?: string
  showAccountColumn?: boolean
  pageSize?: number
  emptyStateTitle?: string
  emptyStateMessage?: string
  skeletonRows?: number
}

type TransactionListEmits = {
  'transaction-select': [transaction: Transaction]
  'transaction-edit': [transaction: Transaction]
  'add-transaction': []
  'refresh': []
}

// Props với default values
const props = withDefaults(defineProps<TransactionListProps>(), {
  loading: false,
  selectedTransactionId: null,
  title: 'Danh sách giao dịch',
  showAccountColumn: false,
  pageSize: 20,
  emptyStateTitle: 'Chưa có giao dịch nào',
  emptyStateMessage: 'Không có giao dịch nào trong khoảng thời gian này',
  skeletonRows: 5
})

// Emits
const emit = defineEmits<TransactionListEmits>()

// Reactive data
const isCompactView = ref(true)
const sortField = ref<SortField>('transactionDate')
const sortDirection = ref<SortDirection>('desc')
const currentPage = ref(1)

// Computed properties
const sortedTransactions = computed(() => {
  const sorted = [...props.transactions].sort((a, b) => {
    let aValue: any
    let bValue: any
    
    switch (sortField.value) {
      case 'transactionDate':
        aValue = new Date(a.transactionDate).getTime()
        bValue = new Date(b.transactionDate).getTime()
        break
      case 'description':
        aValue = a.description.toLowerCase()
        bValue = b.description.toLowerCase()
        break
      case 'amount':
        aValue = a.revenueAmount > 0 ? a.revenueAmount : a.spentAmount
        bValue = b.revenueAmount > 0 ? b.revenueAmount : b.spentAmount
        break
      case 'balance':
        aValue = a.balance ?? 0
        bValue = b.balance ?? 0
        break
      default:
        return 0
    }
    
    if (aValue < bValue) return sortDirection.value === 'asc' ? -1 : 1
    if (aValue > bValue) return sortDirection.value === 'asc' ? 1 : -1
    return 0
  })
  
  return sorted
})

const totalItems = computed(() => props.transactions.length)
const totalPages = computed(() => Math.ceil(totalItems.value / props.pageSize))
const startIndex = computed(() => (currentPage.value - 1) * props.pageSize)
const endIndex = computed(() => Math.min(startIndex.value + props.pageSize, totalItems.value))

const displayTransactions = computed(() => {
  return sortedTransactions.value.slice(startIndex.value, endIndex.value)
})

// Methods
function formatDateTime(dateString: string): string {
  const date = new Date(dateString)
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  
  return `${day}/${month} ${hours}:${minutes}`
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
    currency: 'VND',
    minimumFractionDigits: 0
  }).format(amount)
}

function getAmountColorClass(transaction: Transaction): string {
  return transaction.revenueAmount > 0 ? 'text-green-600' : 'text-red-600'
}

function getRowClass(transaction: Transaction): string {
  const baseClass = 'hover:bg-gray-50 dark:hover:bg-gray-800'
  const selectedClass = props.selectedTransactionId === transaction.id 
    ? 'bg-blue-50 dark:bg-blue-900/20 border-l-4 border-blue-500' 
    : ''
  
  return `${baseClass} ${selectedClass}`
}

function selectTransaction(transaction: Transaction) {
  emit('transaction-select', transaction)
}

function editTransaction(transaction: Transaction) {
  emit('transaction-edit', transaction)
}

function toggleViewMode() {
  isCompactView.value = !isCompactView.value
}

function refreshList() {
  emit('refresh')
}

function sortBy(field: SortField) {
  if (sortField.value === field) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortField.value = field
    sortDirection.value = 'desc'
  }
}

function previousPage() {
  if (currentPage.value > 1) {
    currentPage.value--
  }
}

function nextPage() {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
  }
}

// Watch for transaction changes to reset pagination
watch(() => props.transactions.length, () => {
  currentPage.value = 1
})
</script>

<style scoped>
/* Table styles */
.table-responsive {
  overflow-x: auto;
  border-radius: 0.5rem;
  border: 1px solid #e5e7eb;
}

.table-hover {
  border-collapse: collapse;
  width: 100%;
}

.table-hover th,
.table-hover td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.table-hover th {
  font-weight: 600;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #374151;
}

.dark .table-hover th {
  color: #d1d5db;
}

.dark .table-hover th,
.dark .table-hover td {
  border-bottom-color: #374151;
}

.dark .table-responsive {
  border-color: #374151;
}

.sortable-header {
  cursor: pointer;
  user-select: none;
  position: relative;
}

.sortable-header:hover {
  background-color: #f3f4f6;
}

.dark .sortable-header:hover {
  background-color: #4b5563;
}

/* Button styles */
.btn {
  padding: 0.375rem 0.75rem;
  border-radius: 0.375rem;
  font-weight: 500;
  transition: all 0.2s;
  border: 1px solid transparent;
  cursor: pointer;
  font-size: 0.875rem;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-xs {
  padding: 0.125rem 0.375rem;
  font-size: 0.75rem;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.875rem;
}

.btn-primary {
  background-color: #3b82f6;
  color: white;
}

.btn-outline-primary {
  border-color: #3b82f6;
  color: #3b82f6;
  background: transparent;
}

.btn-outline-secondary {
  border-color: #6b7280;
  color: #6b7280;
  background: transparent;
}

/* Animation */
.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
