<template>
  <div>
    <!-- Breadcrumb -->
    <ul class="flex space-x-2 rtl:space-x-reverse">
      <li>
        <NuxtLink to="/" class="text-primary hover:underline">Dashboard</NuxtLink>
      </li>
      <li class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
        <span>Accounts</span>
      </li>
    </ul>

    <div class="pt-5">
      <!-- Header -->
      <div class="mb-5 flex items-center justify-between">
        <h5 class="text-lg font-semibold dark:text-white-light">Account Management</h5>
        <button type="button" class="btn btn-primary" @click="openCreateModal">
          <icon-plus class="h-5 w-5 ltr:mr-2 rtl:ml-2" />
          Add Account
        </button>
      </div>

      <!-- Filters -->
      <div class="panel mb-5">
        <div class="mb-5">
          <h6 class="text-lg font-semibold">Filters</h6>
        </div>
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-4">
          <div>
            <label class="form-label">Search</label>
            <input v-model="filters.search" type="text" class="form-input" placeholder="Search accounts..."
              @input="debounceSearch" />
          </div>
          <div>
            <label class="form-label">Account Type</label>
            <select v-model="filters.type" class="form-select" @change="applyFilters">
              <option value="">All Types</option>
              <option value="Bank">Bank</option>
              <option value="Wallet">Wallet</option>
              <option value="CreditCard">Credit Card</option>
              <option value="DebitCard">Debit Card</option>
              <option value="Cash">Cash</option>
            </select>
          </div>
          <div>
            <label class="form-label">Currency</label>
            <select v-model="filters.currency" class="form-select" @change="applyFilters">
              <option value="">All Currencies</option>
              <option value="VND">VND</option>
              <option value="USD">USD</option>
              <option value="EUR">EUR</option>
            </select>
          </div>
          <div class="flex items-end">
            <button type="button" class="btn btn-secondary w-full" @click="clearFilters">
              Clear Filters
            </button>
          </div>
        </div>
      </div> <!-- Accounts Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <!-- Loading State -->
        <div v-if="isLoading" class="col-span-full flex justify-center py-20">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
        </div>

        <!-- Account Cards -->
        <div v-for="account in accounts" :key="account.id"
          class="panel cursor-pointer transition-all duration-200 hover:shadow-lg"
          @click="navigateToTransactions(account)">
          <!-- Account Header -->
          <div class="flex items-center justify-between mb-4">
            <div class="flex items-center space-x-3">
              <div :class="[
                'w-10 h-10 rounded-full flex items-center justify-center text-white font-semibold',
                getAccountTypeBadgeClass(account.type)
              ]">
                {{ getAccountInitials(account.name || '') }}
              </div>
              <div>
                <h6 class="font-semibold text-base">{{ account.name }}</h6>
                <p class="text-sm text-gray-500">{{ formatAccountType(account.type) }}</p>
              </div>
            </div>

            <!-- Currency Badge -->
            <span
              class="px-2 py-1 text-xs rounded-full font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300">
              {{ account.currency }}
            </span>
          </div>

          <!-- Account Details -->
          <div class="space-y-3">
            <!-- Current Balance -->
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600">Số dư hiện tại:</span>
              <span :class="[
                'font-semibold',
                getBalanceClass(account.currentBalance)
              ]">
                {{ formatCurrency(account.currentBalance, account.currency) }}
              </span>
            </div>

            <!-- Available Limit (if applicable) -->
            <div v-if="account.availableLimit" class="flex justify-between items-center">
              <span class="text-sm text-gray-600">Hạn mức khả dụng:</span>
              <span class="font-medium text-blue-600">
                {{ formatCurrency(account.availableLimit, account.currency) }}
              </span>
            </div>

            <!-- Card Number (if applicable) -->
            <div v-if="account.cardNumber" class="flex justify-between items-center">
              <span class="text-sm text-gray-600">Số thẻ:</span>
              <span class="font-mono text-sm">
                {{ maskCardNumber(account.cardNumber) }}
              </span>
            </div>

            <!-- Created Date -->
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600">Ngày tạo:</span>
              <span class="text-sm text-gray-500">
                {{ formatDate(account.createAt) }}
              </span>
            </div>
          </div>

          <!-- Quick Actions -->
          <div class="flex gap-2 mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
            <button @click.stop="navigateToTransactions(account)" class="flex-1 btn btn-sm btn-outline-primary"
              title="Xem giao dịch">
              <icon-eye class="h-4 w-4 mr-1" />
              Giao dịch
            </button>
            <button @click.stop="openEditModal(account)" class="btn btn-sm btn-outline-secondary" title="Chỉnh sửa">
              <icon-edit class="h-4 w-4" />
            </button>
            <button @click.stop="confirmDelete(account)" class="btn btn-sm btn-outline-danger" title="Xóa tài khoản">
              <icon-trash class="h-4 w-4" />
            </button>
          </div>
        </div>

        <!-- Add Account Card -->
        <div
          class="panel border-2 border-dashed border-gray-300 dark:border-gray-600 flex items-center justify-center min-h-64 cursor-pointer hover:border-primary transition-colors">
          <div class="text-center">
            <div
              class="w-12 h-12 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-3">
              <icon-plus class="h-6 w-6 text-gray-400" />
            </div>
            <p class="text-gray-500 mb-2">Thêm tài khoản mới</p>
            <button @click="openCreateModal" class="btn btn-sm btn-primary">
              Tạo tài khoản
            </button>
          </div>
        </div>

        <!-- Empty State -->
        <div v-if="!accounts.length && !isLoading" class="col-span-full text-center py-20">
          <div
            class="w-20 h-20 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-4">
            <span class="text-3xl">🏦</span>
          </div>
          <h3 class="text-lg font-semibold mb-2">Chưa có tài khoản nào</h3>
          <p class="text-gray-500 mb-6">Tạo tài khoản đầu tiên để bắt đầu quản lý tài chính</p>
          <button @click="openCreateModal" class="btn btn-primary"> <icon-plus class="h-5 w-5 mr-2" />
            Tạo tài khoản đầu tiên
          </button>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="pagination.pageCount > 1" class="mt-5 flex items-center justify-between">
        <div class="text-sm text-gray-500">
          Showing {{ (pagination.pageIndex - 1) * pagination.pageSize + 1 }} to
          {{ Math.min(pagination.pageIndex * pagination.pageSize, pagination.totalRow) }}
          of {{ pagination.totalRow }} results
        </div>
        <div class="flex items-center gap-2">
          <button type="button" class="btn btn-sm btn-outline-primary" :disabled="pagination.pageIndex <= 1"
            @click="changePage(pagination.pageIndex - 1)">
            Previous
          </button>
          <template v-for="page in visiblePages" :key="page">
            <button v-if="page !== '...'" type="button" class="btn btn-sm"
              :class="page === pagination.pageIndex ? 'btn-primary' : 'btn-outline-primary'"
              @click="changePage(Number(page))">
              {{ page }}
            </button>
            <span v-else class="px-2">...</span>
          </template>
          <button type="button" class="btn btn-sm btn-outline-primary"
            :disabled="pagination.pageIndex >= pagination.pageCount" @click="changePage(pagination.pageIndex + 1)">
            Next
          </button>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <AccountModal v-model="isModalOpen" :account="selectedAccount" :is-edit="isEditMode" @saved="handleAccountSaved" />
  </div>
</template>

<script setup lang="ts">
import { useAppStore } from '@/stores/index'
import { useAccounts } from '@/composables/useAccounts'
import type { Account, AccountFilters, Pagination, FilterBodyRequest, ApiResponse, FilterRequest, FilterDetailsRequest } from '~/types'
import { FilterLogicalOperator, FilterType } from '~/types'
import Swal from 'sweetalert2'

const store = useAppStore()
const {
  getAccounts,
  deleteAccount,
  formatAccountType,
  getAccountTypeBadgeClass,
  maskCardNumber,
  formatCurrency,
  getBalanceClass,
  formatDate
} = useAccounts()

// Reactive data
const accounts = ref<Account[]>([])
const isLoading = ref(false)
const isModalOpen = ref(false)
const isEditMode = ref(false)
const selectedAccount = ref<Account | null>(null)

const filters = ref<{ search: string, type: string, currency: string }>({
  search: '',
  type: '',
  currency: ''
})

const pagination = ref<Pagination>({
  pageIndex: 1,
  pageSize: 10,
  pageCount: 0,
  totalRow: 0
})

// Computed
const visiblePages = computed(() => {
  const total = pagination.value.pageCount
  const current = pagination.value.pageIndex
  const delta = 2
  const range = []
  const rangeWithDots = []

  for (let i = Math.max(2, current - delta); i <= Math.min(total - 1, current + delta); i++) {
    range.push(i)
  }

  if (current - delta > 2) {
    rangeWithDots.push(1, '...')
  } else {
    rangeWithDots.push(1)
  }

  rangeWithDots.push(...range)

  if (current + delta < total - 1) {
    rangeWithDots.push('...', total)
  } else if (total > 1) {
    rangeWithDots.push(total)
  }

  return rangeWithDots
})

// Methods
const fetchAccounts = async () => {
  try {
    isLoading.value = true

    // Build filter details array
    const filterDetails: FilterDetailsRequest[] = []

    if (filters.value.type) {
      filterDetails.push({
        attributeName: 'type',
        filterType: FilterType.Equal,
        value: filters.value.type
      })
    }

    if (filters.value.currency) {
      filterDetails.push({
        attributeName: 'currency',
        filterType: FilterType.Equal,
        value: filters.value.currency
      })
    }

    const filterBody: FilterBodyRequest = {
      langId: "",
      searchValue: filters.value.search || "",
      filter: filterDetails.length > 0 ? {
        logicalOperator: FilterLogicalOperator.And,
        details: filterDetails
      } : {},
      orders: [],
      pagination: {
        pageIndex: pagination.value.pageIndex,
        pageSize: pagination.value.pageSize,
        pageCount: pagination.value.pageCount,
        totalRow: pagination.value.totalRow
      }
    }

    console.log('FilterBodyRequest being sent:', JSON.stringify(filterBody, null, 2))
    const response = await getAccounts(filterBody)
    accounts.value = response?.data || []
    if (response?.pagination) {
      pagination.value = response.pagination
    }
  } catch (error) {
    console.error('Error fetching accounts:', error)
    showErrorMessage('Failed to fetch accounts')
  } finally {
    isLoading.value = false
  }
}

const debounceTimer = ref<NodeJS.Timeout>()
const debounceSearch = () => {
  clearTimeout(debounceTimer.value)
  debounceTimer.value = setTimeout(() => {
    applyFilters()
  }, 500)
}

const applyFilters = () => {
  pagination.value.pageIndex = 1
  fetchAccounts()
}

const clearFilters = () => {
  filters.value = {
    search: '',
    type: '',
    currency: ''
  }
  applyFilters()
}

const changePage = (page: number) => {
  pagination.value.pageIndex = page
  fetchAccounts()
}

const navigateToTransactions = (account: Account) => {
  navigateTo({
    path: '/apps/transactions',
    query: {
      accountId: account.id,
      accountName: account.name
    }
  })
}

const openCreateModal = () => {
  selectedAccount.value = null
  isEditMode.value = false
  isModalOpen.value = true
}

const openEditModal = (account: Account) => {
  selectedAccount.value = { ...account }
  isEditMode.value = true
  isModalOpen.value = true
}

const handleAccountSaved = () => {
  isModalOpen.value = false
  fetchAccounts()
  showSuccessMessage(isEditMode.value ? 'Account updated successfully' : 'Account created successfully')
}

const confirmDelete = async (account: Account) => {
  const result = await Swal.fire({
    title: 'Are you sure?',
    text: `Delete account "${account.name}"?`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#d33',
    cancelButtonColor: '#3085d6',
    confirmButtonText: 'Yes, delete it!'
  })

  if (result.isConfirmed) {
    try {
      await deleteAccount(account.id)
      fetchAccounts()
      showSuccessMessage('Account deleted successfully')
    } catch (error) {
      console.error('Error deleting account:', error)
      showErrorMessage('Failed to delete account')
    }
  }
}

// Utility functions are now imported from useAccounts composable

// Additional utility functions for card layout
const getAccountInitials = (name: string): string => {
  return name.split(' ')
    .map(word => word.charAt(0))
    .join('')
    .toUpperCase()
    .slice(0, 2)
}

const showSuccessMessage = (message: string) => {
  Swal.fire({
    icon: 'success',
    title: 'Success',
    text: message,
    timer: 2000,
    showConfirmButton: false
  })
}

const showErrorMessage = (message: string) => {
  Swal.fire({
    icon: 'error',
    title: 'Error',
    text: message
  })
}

// Lifecycle
onMounted(() => {
  fetchAccounts()
})

// SEO
useHead({
  title: 'Account Management - CoreFinance'
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
  display: inline-flex;
  align-items: center;
  justify-content: center;
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

.btn-outline-secondary:hover {
  background-color: #6b7280;
  color: white;
}

.btn-outline-info {
  border-color: #06b6d4;
  color: #06b6d4;
  background: transparent;
}

.btn-outline-info:hover {
  background-color: #06b6d4;
  color: white;
}

.btn-outline-danger {
  border-color: #ef4444;
  color: #ef4444;
  background: transparent;
}

.btn-outline-danger:hover {
  background-color: #ef4444;
  color: white;
}

.btn-sm {
  padding: 0.25rem 0.75rem;
  font-size: 0.875rem;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn:disabled:hover {
  background-color: inherit;
  color: inherit;
  border-color: inherit;
}
</style>