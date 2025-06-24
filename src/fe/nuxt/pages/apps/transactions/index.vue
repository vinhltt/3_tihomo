<template>
  <div class="space-y-6"> <!-- Header với breadcrumbs -->
    <div>
      <ul class="flex space-x-2 rtl:space-x-reverse">
        <li>
          <NuxtLink to="/apps" class="text-primary hover:underline">Apps</NuxtLink>
        </li>
        <li v-if="route.query.accountId && route.query.accountName"
          class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
          <NuxtLink to="/apps/accounts" class="text-primary hover:underline">Accounts</NuxtLink>
        </li>
        <li v-if="route.query.accountId && route.query.accountName"
          class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
          <NuxtLink :to="`/apps/accounts?highlight=${route.query.accountId}`" class="text-primary hover:underline">
            {{ route.query.accountName }}
          </NuxtLink>
        </li>
        <li class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
          <span>Transactions</span>
        </li>
      </ul>
      <div class="pt-5">
        <h1 class="text-2xl font-semibold">
          {{ route.query.accountName ? `Giao dịch - ${route.query.accountName}` : 'Quản lý Giao dịch' }}
        </h1>
        <p class="text-gray-600 dark:text-white-light">
          {{
            route.query.accountName
              ? `Theo dõi giao dịch của tài khoản ${route.query.accountName}`
              : 'Theo dõi và quản lý các giao dịch thu chi'
          }}
        </p>
      </div>
    </div>

    <!-- Filter Section -->
    <div class="panel">
      <div class="mb-5 flex flex-col gap-4 sm:flex-row sm:items-end sm:gap-6"> <!-- Account Filter -->
        <div class="flex-1">
          <label class="text-sm font-medium">Tài khoản</label>
          <div class="relative">
            <select v-model="selectedAccountId" @change="handleAccountChange" class="form-select" :disabled="isLoading">
              <option value="">Tất cả tài khoản</option>
              <option v-for="account in accounts" :key="account.id" :value="account.id">
                {{ account.name }}
              </option>
            </select>
            <div v-if="isLoading" class="absolute right-8 top-1/2 transform -translate-y-1/2">
              <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-primary"></div>
            </div>
          </div>
        </div>

        <!-- Date Range -->
        <div class="flex-1">
          <label class="text-sm font-medium">Từ ngày</label>
          <input v-model="dateRange.startDate" @change="handleDateRangeChange" type="date" class="form-input" />
        </div>
        <div class="flex-1">
          <label class="text-sm font-medium">Đến ngày</label>
          <input v-model="dateRange.endDate" @change="handleDateRangeChange" type="date" class="form-input" />
        </div>

        <!-- Quick Actions -->
        <div class="flex gap-2">
          <button @click="() => openCreateModal(TransactionDirection.Revenue)" class="btn btn-outline-success">
            <icon-plus class="w-4 h-4" />
            Thu
          </button>
          <button @click="() => openCreateModal(TransactionDirection.Spent)" class="btn btn-outline-danger">
            <icon-plus class="w-4 h-4" />
            Chi
          </button>
        </div>
      </div>
    </div>

    <!-- Main Layout: Transaction List + Detail Panel -->
    <div class="relative">
      <!-- Transaction List Container -->
      <div class="transition-all duration-300" :class="showDetailPanel ? 'lg:mr-[50%]' : ''">
        <div class="panel">
          <!-- Table Header với Column Selector -->
          <div class="mb-5 flex items-center justify-between">
            <h5 class="text-lg font-semibold dark:text-white-light">
              Danh sách giao dịch
            </h5>
            <div class="flex items-center gap-4">
              <div class="text-sm text-gray-500">
                Tổng: {{ pagination.totalRow }} giao dịch
              </div>

              <!-- Columns Selector -->
              <div class="relative">
                <Popper placement="bottom-end">
                  <button class="btn btn-outline-primary btn-sm">
                    <icon-settings class="w-4 h-4" />
                    Cột
                  </button>
                  <template #content="{ close }">
                    <div
                      class="bg-white dark:bg-[#0e1726] border border-gray-200 dark:border-[#1b2e4b] rounded-lg shadow-lg p-4 min-w-[200px]">
                      <h6 class="font-semibold mb-3">Chọn cột hiển thị</h6>
                      <div class="space-y-2">
                        <label class="flex items-center">
                          <input type="checkbox" v-model="visibleColumns.date" class="form-checkbox" />
                          <span class="ml-2">Ngày</span>
                        </label>
                        <label class="flex items-center">
                          <input type="checkbox" v-model="visibleColumns.description" class="form-checkbox" />
                          <span class="ml-2">Mô tả</span>
                        </label>
                        <label class="flex items-center">
                          <input type="checkbox" v-model="visibleColumns.amount" class="form-checkbox" />
                          <span class="ml-2">Số tiền</span>
                        </label>
                        <hr class="border-gray-200 dark:border-[#1b2e4b]" />
                        <label class="flex items-center">
                          <input type="checkbox" v-model="visibleColumns.account" class="form-checkbox" />
                          <span class="ml-2">Tài khoản</span>
                        </label>
                        <label class="flex items-center">
                          <input type="checkbox" v-model="visibleColumns.category" class="form-checkbox" />
                          <span class="ml-2">Danh mục</span>
                        </label>
                        <label class="flex items-center">
                          <input type="checkbox" v-model="visibleColumns.balance" class="form-checkbox" />
                          <span class="ml-2">Số dư</span>
                        </label>
                        <label class="flex items-center">
                          <input type="checkbox" v-model="visibleColumns.actions" class="form-checkbox" />
                          <span class="ml-2">Thao tác</span>
                        </label>
                      </div>
                      <div class="flex gap-2 mt-4">
                        <button @click="setSimpleMode()" class="btn btn-outline-primary btn-sm flex-1">
                          Đơn giản
                        </button>
                        <button @click="setAdvancedMode()" class="btn btn-outline-primary btn-sm flex-1">
                          Nâng cao
                        </button>
                      </div>
                    </div>
                  </template>
                </Popper>
              </div>
            </div>
          </div>

          <!-- Loading State -->
          <div v-if="isLoading" class="text-center py-8">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            <p class="mt-2 text-gray-500">Đang tải...</p>
          </div>

          <!-- Error State -->
          <div v-else-if="error" class="text-center py-8 text-danger">
            <p>{{ error }}</p>
            <button @click="getTransactions()" class="btn btn-primary mt-2">
              Thử lại
            </button>
          </div>

          <!-- Empty State -->
          <div v-else-if="!transactions.length" class="text-center py-8">
            <icon-folder class="w-16 h-16 mx-auto text-gray-400 mb-4" />
            <p class="text-gray-500">Chưa có giao dịch nào</p>
            <div class="flex gap-2 justify-center mt-4">
              <button @click="() => openCreateModal(TransactionDirection.Revenue)" class="btn btn-success btn-sm">
                Thêm giao dịch Thu
              </button>
              <button @click="() => openCreateModal(TransactionDirection.Spent)" class="btn btn-danger btn-sm">
                Thêm giao dịch Chi
              </button>
            </div>
          </div>

          <!-- Transaction Table -->
          <div v-else class="table-responsive">
            <table class="table-hover">
              <thead>
                <tr>
                  <th v-if="visibleColumns.date">Ngày</th>
                  <th v-if="visibleColumns.account">Tài khoản</th>
                  <th v-if="visibleColumns.description">Mô tả</th>
                  <th v-if="visibleColumns.category">Danh mục</th>
                  <th v-if="visibleColumns.amount" class="text-right">Số tiền</th>
                  <th v-if="visibleColumns.balance" class="text-right">Số dư</th>
                  <th v-if="visibleColumns.actions" class="text-center">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="transaction in transactions" :key="transaction.id"
                  @click="() => selectTransaction(transaction)" class="cursor-pointer transition-colors" :class="[
                    'hover:bg-gray-50 dark:hover:bg-gray-800',
                    selectedTransaction?.id === transaction.id ? 'bg-primary/10 border-l-4 border-primary' : ''
                  ]">
                  <td v-if="visibleColumns.date">
                    <div class="text-sm font-medium">
                      {{ formatDate(transaction.transactionDate) }}
                    </div>
                  </td>
                  <td v-if="visibleColumns.account">
                    <div class="font-medium">
                      {{ getAccountNameFromComposable(transaction.accountId) }}
                    </div>
                  </td>
                  <td v-if="visibleColumns.description">
                    <div class="max-w-xs truncate" :title="transaction.description">
                      {{ transaction.description || '-' }}
                    </div>
                  </td>
                  <td v-if="visibleColumns.category">
                    <span class="badge badge-outline-info">
                      {{ getCategoryTypeName(transaction.categoryType) }}
                    </span>
                  </td>
                  <td v-if="visibleColumns.amount" class="text-right">
                    <span :class="getDisplayClass(transaction)" class="font-semibold">
                      {{ getDisplayAmount(transaction) }}
                    </span>
                  </td>
                  <td v-if="visibleColumns.balance" class="text-right">
                    <span class="font-medium">
                      {{ formatCurrency(transaction.balance) }}
                    </span>
                  </td>
                  <td v-if="visibleColumns.actions" class="text-center">
                    <div class="flex items-center justify-center gap-2">
                      <button @click.stop="() => selectTransaction(transaction)" class="btn btn-sm btn-outline-primary"
                        title="Xem chi tiết">
                        <icon-eye class="w-4 h-4" />
                      </button>
                      <button @click.stop="() => editTransaction(transaction)" class="btn btn-sm btn-outline-warning"
                        title="Chỉnh sửa">
                        <icon-edit class="w-4 h-4" />
                      </button>
                      <button @click.stop="() => confirmDelete(transaction)" class="btn btn-sm btn-outline-danger"
                        title="Xóa">
                        <icon-trash class="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="pagination.pageCount > 1" class="mt-5 flex items-center justify-between">
            <div class="text-sm text-gray-500">
              Hiển thị {{ (pagination.pageIndex * pagination.pageSize) + 1 }}-{{ Math.min((pagination.pageIndex + 1) *
                pagination.pageSize, pagination.totalRow) }}
              trong tổng số {{ pagination.totalRow }} giao dịch
            </div>
            <div class="flex gap-1">
              <button @click="changePage(pagination.pageIndex - 1)" :disabled="pagination.pageIndex === 0"
                class="btn btn-sm btn-outline-primary">
                Trước
              </button>
              <button @click="changePage(pagination.pageIndex + 1)"
                :disabled="pagination.pageIndex >= pagination.pageCount - 1" class="btn btn-sm btn-outline-primary">
                Sau
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Transaction Detail Panel (Desktop) -->
      <div v-if="showDetailPanel"
        class="fixed right-0 top-0 h-full w-full lg:w-1/2 z-40 bg-white dark:bg-[#0e1726] border-l border-gray-200 dark:border-[#1b2e4b] transition-transform duration-300"
        :class="showDetailPanel ? 'translate-x-0' : 'translate-x-full'">
        <div class="h-full overflow-y-auto">
          <TransactionDetailPanel :visible="showDetailPanel" :transaction="selectedTransaction"
            :accounts="[...accounts]" :mode="detailMode" :default-direction="defaultDirection"
            :default-account-id="selectedAccountId" @update:visible="closeDetailPanel"
            @created="handleTransactionCreated" @updated="handleTransactionUpdated" @deleted="handleTransactionDeleted"
            @edit="handleEdit" />
        </div>
      </div>

      <!-- Mobile Overlay -->
      <div v-if="showDetailPanel" class="lg:hidden fixed inset-0 bg-black/50 z-30" @click="closeDetailPanel"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TransactionViewModel, TransactionFilter, TransactionDirectionType } from '~/types/transaction'
import type { AccountViewModel } from '~/types/account'
import { TransactionDirection, CategoryType, formatDisplayAmount, getAmountClass } from '~/types/transaction'
import TransactionDetailPanel from '~/components/apps/transactions/TransactionDetailPanel.vue'

// Composables
const route = useRoute()
const {
  transactions,
  selectedTransaction,
  isLoading,
  error,
  pagination,
  getTransactions,
  deleteTransaction,
  filterByAccount,
  filterByDateRange,
  setSelectedTransaction
} = useTransactions()

const { accounts, getAccounts, getAccountName: getAccountNameFromComposable } = useAccountsSimple()

// Page State
const selectedAccountId = ref('')
const dateRange = ref({
  startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
  endDate: new Date().toISOString().split('T')[0]
})

const showDetailPanel = ref(false)
const detailMode = ref<'create' | 'edit' | 'view'>('view')
const defaultDirection = ref<TransactionDirectionType>(TransactionDirection.Spent)

// Column visibility state
const visibleColumns = ref({
  date: true,
  description: true,
  amount: true,
  account: false,
  category: false,
  balance: false,
  actions: false
})

// SEO
useHead({
  title: 'Quản lý Giao dịch'
})

// Keyboard event handler
const handleKeydown = (event: KeyboardEvent) => {
  if (event.key === 'Escape' && showDetailPanel.value) {
    closeDetailPanel()
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    getAccounts(),
    handleNavigationContext()
  ])

  // Add ESC key listener
  window.addEventListener('keydown', handleKeydown)
})

// Handle navigation context from query parameters
const handleNavigationContext = async () => {
  const { accountId, accountName } = route.query

  if (accountId && accountName) {
    // Set the selected account based on navigation context
    selectedAccountId.value = accountId as string

    // Update page title to include account name
    useHead({
      title: `Giao dịch - ${accountName} | Quản lý Giao dịch`
    })

    // Filter transactions by the selected account
    await getTransactions({ accountId: accountId as string })
  } else {
    // Default behavior - load all transactions
    await getTransactions()
  }
}

onUnmounted(() => {
  // Remove ESC key listener
  window.removeEventListener('keydown', handleKeydown)
})

// Methods
const handleAccountChange = () => {
  // If "Tất cả tài khoản" is selected (empty string), pass undefined to clear the filter
  // Otherwise pass the selected account ID
  const accountIdFilter = selectedAccountId.value === '' ? undefined : selectedAccountId.value

  console.log('Account filter changed:', {
    selectedValue: selectedAccountId.value,
    filterValue: accountIdFilter,
    willFilterByAccount: accountIdFilter !== undefined
  })

  // Always call getTransactions with accountId filter to ensure proper clearing
  getTransactions({ accountId: accountIdFilter })
}

const handleDateRangeChange = () => {
  filterByDateRange(dateRange.value.startDate, dateRange.value.endDate)
}

const openCreateModal = (direction: TransactionDirectionType) => {
  detailMode.value = 'create'
  setSelectedTransaction(null)
  defaultDirection.value = direction
  showDetailPanel.value = true
}

const selectTransaction = (transaction: TransactionViewModel) => {
  setSelectedTransaction(transaction)
  detailMode.value = 'view'
  showDetailPanel.value = true
}

const editTransaction = (transaction: TransactionViewModel) => {
  setSelectedTransaction(transaction)
  detailMode.value = 'edit'
  showDetailPanel.value = true
}

const closeDetailPanel = () => {
  showDetailPanel.value = false
  setSelectedTransaction(null)
}

const confirmDelete = async (transaction: TransactionViewModel) => {
  if (confirm(`Bạn có chắc muốn xóa giao dịch "${transaction.description || 'Không có mô tả'}"?`)) {
    try {
      await deleteTransaction(transaction.id)
      await getTransactions()
    } catch (err) {
      console.error('Failed to delete transaction:', err)
    }
  }
}

const changePage = async (page: number) => {
  if (page >= 0 && page < pagination.value.pageCount) {
    await getTransactions({}, page)
  }
}

// Column visibility methods
const setSimpleMode = () => {
  visibleColumns.value = {
    date: true,
    description: true,
    amount: true,
    account: false,
    category: false,
    balance: false,
    actions: false
  }
}

const setAdvancedMode = () => {
  visibleColumns.value = {
    date: true,
    description: true,
    amount: true,
    account: true,
    category: true,
    balance: true,
    actions: true
  }
}

// Event handlers
const handleTransactionCreated = () => {
  closeDetailPanel()
  getTransactions()
}

const handleTransactionUpdated = () => {
  closeDetailPanel()
  getTransactions()
}

const handleTransactionDeleted = () => {
  closeDetailPanel()
  getTransactions()
}

const handleEdit = () => {
  detailMode.value = 'edit'
}

// Helper functions
const getCategoryTypeName = (categoryType: number) => {
  const names: Record<number, string> = {
    [CategoryType.Income]: 'Thu nhập',
    [CategoryType.Expense]: 'Chi tiêu',
    [CategoryType.Transfer]: 'Chuyển khoản',
    [CategoryType.Fee]: 'Phí',
    [CategoryType.Other]: 'Khác'
  }
  return names[categoryType] || 'Khác'
}

const getDisplayAmount = (transaction: TransactionViewModel) => {
  return formatDisplayAmount(transaction)
}

const getDisplayClass = (transaction: TransactionViewModel) => {
  return getAmountClass(transaction)
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  })
}

const formatCurrency = (amount: number) => {
  return amount.toLocaleString('vi-VN') + ' ₫'
}
</script>