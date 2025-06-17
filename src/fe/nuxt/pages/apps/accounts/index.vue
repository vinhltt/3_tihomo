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
        <button
          type="button"
          class="btn btn-primary"
          @click="openCreateModal"
        >
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
            <input
              v-model="filters.search"
              type="text"
              class="form-input"
              placeholder="Search accounts..."
              @input="debounceSearch"
            />
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
            <button
              type="button"
              class="btn btn-secondary w-full"
              @click="clearFilters"
            >
              Clear Filters
            </button>
          </div>
        </div>
      </div>

      <!-- Accounts Table -->
      <div class="panel">
        <div class="mb-5 flex items-center justify-between">
          <h6 class="text-lg font-semibold">Accounts List</h6>
          <div class="text-sm text-gray-500">
            Total: {{ pagination.totalRow }} accounts
          </div>
        </div>

        <!-- Loading State -->
        <div v-if="isLoading" class="flex justify-center py-10">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
        </div>

        <!-- Table -->
        <div v-else class="table-responsive">
          <table class="table-auto">
            <thead>
              <tr>
                <th>Account Name</th>
                <th>Type</th>
                <th>Card Number</th>
                <th>Currency</th>
                <th>Current Balance</th>
                <th>Available Limit</th>
                <th>Created Date</th>
                <th class="text-center">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="account in accounts" :key="account.id" class="hover:bg-gray-50 dark:hover:bg-gray-800">
                <td>
                  <div class="font-semibold">{{ account.name }}</div>
                </td>
                <td>
                  <span class="badge" :class="getAccountTypeBadgeClass(account.type)">
                    {{ formatAccountType(account.type) }}
                  </span>
                </td>
                <td>
                  <span v-if="account.cardNumber" class="font-mono">
                    {{ maskCardNumber(account.cardNumber) }}
                  </span>
                  <span v-else class="text-gray-400">-</span>
                </td>
                <td>
                  <span class="font-semibold">{{ account.currency }}</span>
                </td>
                <td>
                  <span class="font-semibold" :class="getBalanceClass(account.currentBalance)">
                    {{ formatCurrency(account.currentBalance, account.currency) }}
                  </span>
                </td>
                <td>
                  <span v-if="account.availableLimit" class="font-semibold text-blue-600">
                    {{ formatCurrency(account.availableLimit, account.currency) }}
                  </span>
                  <span v-else class="text-gray-400">-</span>
                </td>
                <td>{{ formatDate(account.createAt) }}</td>
                <td>
                  <div class="flex items-center justify-center gap-2">
                    <button
                      type="button"
                      class="btn btn-sm btn-outline-primary"
                      @click="openEditModal(account)"
                    >
                      <icon-edit class="h-4 w-4" />
                    </button>
                    <button
                      type="button"
                      class="btn btn-sm btn-outline-danger"
                      @click="confirmDelete(account)"
                    >
                      <icon-trash class="h-4 w-4" />
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!accounts.length && !isLoading">
                <td colspan="8" class="text-center py-10 text-gray-500">
                  No accounts found
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="pagination.pageCount > 1" class="mt-5 flex items-center justify-between">
          <div class="text-sm text-gray-500">
            Showing {{ (pagination.pageIndex - 1) * pagination.pageSize + 1 }} to 
            {{ Math.min(pagination.pageIndex * pagination.pageSize, pagination.totalRow) }} 
            of {{ pagination.totalRow }} results
          </div>
          <div class="flex items-center gap-2">
            <button
              type="button"
              class="btn btn-sm btn-outline-primary"
              :disabled="pagination.pageIndex <= 1"
              @click="changePage(pagination.pageIndex - 1)"
            >
              Previous
            </button>
            <template v-for="page in visiblePages" :key="page">
              <button
                v-if="page !== '...'"
                type="button"
                class="btn btn-sm"
                :class="page === pagination.pageIndex ? 'btn-primary' : 'btn-outline-primary'"
                @click="changePage(Number(page))"
              >
                {{ page }}
              </button>
              <span v-else class="px-2">...</span>
            </template>
            <button
              type="button"
              class="btn btn-sm btn-outline-primary"
              :disabled="pagination.pageIndex >= pagination.pageCount"
              @click="changePage(pagination.pageIndex + 1)"
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <AccountModal
      v-model="isModalOpen"
      :account="selectedAccount"
      :is-edit="isEditMode"
      @saved="handleAccountSaved"
    />
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

const filters = ref<{search: string, type: string, currency: string}>({
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