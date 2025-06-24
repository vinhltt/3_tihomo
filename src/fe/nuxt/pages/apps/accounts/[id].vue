<template>
  <div>
    <!-- Breadcrumb -->
    <ul class="flex space-x-2 rtl:space-x-reverse">
      <li>
        <NuxtLink to="/" class="text-primary hover:underline">Dashboard</NuxtLink>
      </li>
      <li class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
        <NuxtLink to="/apps/accounts" class="text-primary hover:underline">Accounts</NuxtLink>
      </li>
      <li class="before:content-['/'] ltr:before:mr-2 rtl:before:ml-2">
        <span>{{ account?.name || 'Account Details' }}</span>
      </li>
    </ul>

    <div class="pt-5">
      <!-- Loading State -->
      <div v-if="isLoading" class="flex justify-center py-20">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>

      <!-- Account Not Found -->
      <div v-else-if="!account" class="panel">
        <div class="text-center py-20">
          <h3 class="text-2xl font-semibold text-gray-500 mb-4">Account Not Found</h3>
          <p class="text-gray-400 mb-6">The account you're looking for doesn't exist or has been deleted.</p>
          <NuxtLink to="/apps/accounts" class="btn btn-primary">
            Back to Accounts
          </NuxtLink>
        </div>
      </div>

      <!-- Account Details -->
      <div v-else>
        <!-- Header with Account Info -->
        <div class="mb-6">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h2 class="text-2xl font-bold text-gray-900 dark:text-white">{{ account.name }}</h2>
              <div class="flex items-center gap-3 mt-2">
                <span class="badge" :class="getAccountTypeBadgeClass(account.type)">
                  {{ formatAccountType(account.type) }}
                </span>
                <span class="text-sm text-gray-500">
                  Created {{ formatDate(account.createAt) }}
                </span>
              </div>
            </div>
            <div class="flex gap-3">
              <button type="button" class="btn btn-outline-primary" @click="openEditModal">
                <icon-edit class="h-4 w-4 ltr:mr-2 rtl:ml-2" />
                Edit Account
              </button>
              <button type="button" class="btn btn-outline-danger" @click="confirmDelete">
                <icon-trash class="h-4 w-4 ltr:mr-2 rtl:ml-2" />
                Delete
              </button>
            </div>
          </div>
        </div>

        <!-- Balance Overview Cards -->
        <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-6">
          <!-- Current Balance -->
          <div class="panel">
            <div class="flex items-center justify-between">
              <div>
                <div class="text-sm text-gray-500 mb-1">Current Balance</div>
                <div class="text-2xl font-bold" :class="getBalanceClass(account.currentBalance)">
                  {{ formatCurrency(account.currentBalance, account.currency) }}
                </div>
              </div>
              <div class="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center">
                <icon-credit-card class="h-6 w-6 text-primary" />
              </div>
            </div>
          </div>

          <!-- Initial Balance -->
          <div class="panel">
            <div class="flex items-center justify-between">
              <div>
                <div class="text-sm text-gray-500 mb-1">Initial Balance</div>
                <div class="text-2xl font-bold text-gray-700 dark:text-gray-300">
                  {{ formatCurrency(account.initialBalance, account.currency) }}
                </div>
              </div>
              <div class="w-12 h-12 bg-success/10 rounded-full flex items-center justify-center">
                <icon-plus class="h-6 w-6 text-success" />
              </div>
            </div>
          </div>

          <!-- Available Limit -->
          <div v-if="account.availableLimit" class="panel">
            <div class="flex items-center justify-between">
              <div>
                <div class="text-sm text-gray-500 mb-1">Available Limit</div>
                <div class="text-2xl font-bold text-blue-600">
                  {{ formatCurrency(account.availableLimit, account.currency) }}
                </div>
              </div>
              <div class="w-12 h-12 bg-info/10 rounded-full flex items-center justify-center">
                <icon-credit-card class="h-6 w-6 text-info" />
              </div>
            </div>
          </div>

          <!-- Card Number -->
          <div v-if="account.cardNumber" class="panel">
            <div class="flex items-center justify-between">
              <div>
                <div class="text-sm text-gray-500 mb-1">Card Number</div>
                <div class="text-lg font-mono text-gray-700 dark:text-gray-300">
                  {{ maskCardNumber(account.cardNumber) }}
                </div>
              </div>
              <div class="w-12 h-12 bg-warning/10 rounded-full flex items-center justify-center">
                <icon-credit-card class="h-6 w-6 text-warning" />
              </div>
            </div>
          </div>
        </div>

        <!-- Account Details Panel -->
        <div class="panel mb-6">
          <div class="mb-5">
            <h6 class="text-lg font-semibold">Account Information</h6>
          </div>
          <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            <div>
              <label class="text-sm text-gray-500">Account Name</label>
              <div class="text-base font-medium">{{ account.name }}</div>
            </div>
            <div>
              <label class="text-sm text-gray-500">Account Type</label>
              <div class="text-base font-medium">{{ formatAccountType(account.type) }}</div>
            </div>
            <div>
              <label class="text-sm text-gray-500">Currency</label>
              <div class="text-base font-medium">{{ account.currency }}</div>
            </div>
            <div>
              <label class="text-sm text-gray-500">Created Date</label>
              <div class="text-base font-medium">{{ formatDate(account.createAt) }}</div>
            </div>
            <div>
              <label class="text-sm text-gray-500">Last Updated</label>
              <div class="text-base font-medium">{{ formatDate(account.updateAt) }}</div>
            </div>
            <div v-if="account.createBy">
              <label class="text-sm text-gray-500">Created By</label>
              <div class="text-base font-medium">{{ account.createBy }}</div>
            </div>
          </div>
        </div>

        <!-- Balance Chart -->
        <div class="panel mb-6">
          <div class="mb-5 flex items-center justify-between">
            <h6 class="text-lg font-semibold">Balance History</h6>
            <div class="dropdown">
              <client-only>
                <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'">
                  <button type="button" class="btn btn-outline-primary btn-sm">
                    <icon-horizontal-dots class="h-4 w-4" />
                  </button>
                  <template #content="{ close }">
                    <ul @click="close()">
                      <li><a href="#" @click="changeChartPeriod('7d')">Last 7 Days</a></li>
                      <li><a href="#" @click="changeChartPeriod('30d')">Last 30 Days</a></li>
                      <li><a href="#" @click="changeChartPeriod('90d')">Last 90 Days</a></li>
                      <li><a href="#" @click="changeChartPeriod('1y')">Last Year</a></li>
                    </ul>
                  </template>
                </Popper>
              </client-only>
            </div>
          </div>

          <client-only>
            <apexchart v-if="chartOptions && chartSeries" height="300" :options="chartOptions" :series="chartSeries" />
            <div v-else class="flex justify-center py-20">
              <div class="text-gray-500">No chart data available</div>
            </div>
          </client-only>
        </div>

        <!-- Recent Transactions -->
        <div class="panel">
          <div class="mb-5 flex items-center justify-between">
            <h6 class="text-lg font-semibold">Recent Transactions</h6>
            <NuxtLink to="/apps/transactions" class="btn btn-outline-primary btn-sm">
              View All Transactions
            </NuxtLink>
          </div>

          <div class="table-responsive">
            <table class="table-auto">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Amount</th>
                  <th>Balance</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="transaction in recentTransactions" :key="transaction.id">
                  <td>{{ formatDate(transaction.date) }}</td>
                  <td>{{ transaction.description }}</td>
                  <td>
                    <span class="font-semibold" :class="transaction.amount >= 0 ? 'text-green-600' : 'text-red-600'">
                      {{ formatCurrency(transaction.amount, account.currency) }}
                    </span>
                  </td>
                  <td>
                    <span class="font-semibold">
                      {{ formatCurrency(transaction.balance, account.currency) }}
                    </span>
                  </td>
                </tr>
                <tr v-if="!recentTransactions.length">
                  <td colspan="4" class="text-center py-10 text-gray-500">
                    No transactions found
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <AccountModal v-model="isEditModalOpen" :account="account" :is-edit="true" @saved="handleAccountUpdated" />
  </div>
</template>

<script setup lang="ts">
import { useAppStore } from '@/stores/index'
import { useAccounts } from '@/composables/useAccounts'
import type { Account } from '~/types'
import Swal from 'sweetalert2'

const route = useRoute()
const router = useRouter()
const store = useAppStore()

const {
  getAccountById,
  deleteAccount,
  formatAccountType,
  getAccountTypeBadgeClass,
  maskCardNumber,
  formatCurrency,
  getBalanceClass,
  formatDate
} = useAccounts()

// Reactive data
const account = ref<Account | null>(null)
const isLoading = ref(true)
const isEditModalOpen = ref(false)
const chartPeriod = ref('30d')

// Mock data for demonstration
const recentTransactions = ref([
  {
    id: '1',
    date: '2024-01-15',
    description: 'Online Purchase',
    amount: -125.50,
    balance: 2874.50
  },
  {
    id: '2',
    date: '2024-01-14',
    description: 'Salary Deposit',
    amount: 3000.00,
    balance: 3000.00
  },
  {
    id: '3',
    date: '2024-01-13',
    description: 'ATM Withdrawal',
    amount: -200.00,
    balance: 0.00
  }
])

// Chart configuration
const chartOptions = computed(() => {
  if (!account.value) return null

  return {
    chart: {
      type: 'area',
      height: 300,
      toolbar: {
        show: false
      },
      zoom: {
        enabled: false
      }
    },
    colors: ['#1f2937'],
    dataLabels: {
      enabled: false
    },
    stroke: {
      curve: 'smooth',
      width: 2
    },
    fill: {
      type: 'gradient',
      gradient: {
        shadeIntensity: 1,
        opacityFrom: 0.4,
        opacityTo: 0.1,
        stops: [0, 90, 100]
      }
    },
    grid: {
      show: true,
      borderColor: '#e0e6ed',
      strokeDashArray: 5
    },
    xaxis: {
      categories: ['Jan 1', 'Jan 5', 'Jan 10', 'Jan 15', 'Jan 20', 'Jan 25', 'Jan 30'],
      axisBorder: {
        show: false
      },
      axisTicks: {
        show: false
      }
    },
    yaxis: {
      labels: {
        formatter: (value: number) => formatCurrency(value, account.value?.currency)
      }
    },
    tooltip: {
      y: {
        formatter: (value: number) => formatCurrency(value, account.value?.currency)
      }
    }
  }
})

const chartSeries = computed(() => {
  if (!account.value) return null

  // Mock balance history data
  return [{
    name: 'Balance',
    data: [2000, 2500, 2200, 2800, 2600, 2900, account.value.currentBalance]
  }]
})

// Methods
const fetchAccount = async () => {
  try {
    isLoading.value = true
    const accountId = route.params.id as string
    account.value = await getAccountById(accountId)
  } catch (error) {
    console.error('Error fetching account:', error)
    account.value = null
  } finally {
    isLoading.value = false
  }
}

const openEditModal = () => {
  isEditModalOpen.value = true
}

const handleAccountUpdated = () => {
  isEditModalOpen.value = false
  fetchAccount()
  showSuccessMessage('Account updated successfully')
}

const confirmDelete = async () => {
  if (!account.value) return

  const result = await Swal.fire({
    title: 'Are you sure?',
    text: `Delete account "${account.value.name}"? This action cannot be undone.`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#d33',
    cancelButtonColor: '#3085d6',
    confirmButtonText: 'Yes, delete it!'
  })

  if (result.isConfirmed) {
    try {
      await deleteAccount(account.value.id)
      showSuccessMessage('Account deleted successfully')
      router.push('/apps/accounts')
    } catch (error) {
      console.error('Error deleting account:', error)
      showErrorMessage('Failed to delete account')
    }
  }
}

const changeChartPeriod = (period: string) => {
  chartPeriod.value = period
  // In a real app, you would fetch new chart data here
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
  fetchAccount()
})

// SEO
useHead({
  title: computed(() => `${account.value?.name || 'Account'} - CoreFinance`)
})
</script>