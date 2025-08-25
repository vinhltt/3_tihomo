<template>
  <div class="dashboard-container">
    <!-- Header -->
    <div class="dashboard-header">
      <h1 class="dashboard-title">Tổng quan tài chính</h1>
      <div class="header-controls">
        <!-- Mock Data Scenario Selector (for testing UI) -->
        <div class="scenario-selector">
          <label for="scenario" class="scenario-label">Demo scenario:</label>
          <select v-model="selectedScenario" @change="switchScenario" class="scenario-select">
            <option value="default">Bình thường</option>
            <option value="good">Tài chính tốt</option>
            <option value="struggling">Khó khăn</option>
            <option value="starter">Mới bắt đầu</option>
          </select>
        </div>
        
        <div class="date-selector">
          <button @click="loadDashboardData" :disabled="loading" class="refresh-btn">
            <Icon name="heroicons:arrow-path" size="16" :class="{ 'animate-spin': loading }" />
          </button>
          <select v-model="selectedPeriod" class="period-select" @change="loadDashboardData">
            <option value="thisMonth">Tháng này</option>
            <option value="lastMonth">Tháng trước</option>
            <option value="thisYear">Năm này</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Đang tải dữ liệu...</p>
    </div>

    <!-- Dashboard Content -->
    <div v-else>
      <!-- Financial Summary Cards -->
      <div class="summary-grid">
        <FinancialSummaryCard
          title="Tổng tiền thực có"
          :amount="financialData?.totalCash || 0"
          icon="heroicons:banknotes"
          color="success"
          :change="financialData?.cashChange"
          subtitle="Tất cả tài khoản"
        />
        
        <FinancialSummaryCard
          title="Tổng nợ phải trả"
          :amount="-(financialData?.totalDebt || 0)"
          icon="heroicons:credit-card"
          color="danger"
          :change="financialData?.debtChange"
          subtitle="Thẻ tín dụng & Khoản vay"
        />
        
        <FinancialSummaryCard
          title="Tiền đang cho vay"
          :amount="financialData?.totalLent || 0"
          icon="heroicons:arrow-up-circle"
          color="primary"
          :change="financialData?.lentChange"
          subtitle="Khoản cho bạn bè vay"
        />
        
        <FinancialSummaryCard
          title="Tiền đang đi vay"
          :amount="-(financialData?.totalBorrowed || 0)"
          icon="heroicons:arrow-down-circle"
          color="warning"
          :change="financialData?.borrowedChange"
          subtitle="Khoản vay từ bạn bè"
        />
      </div>

      <!-- Tài sản ròng -->
      <div class="net-worth-section">
        <div class="panel">
          <h2 class="mb-5 text-lg font-semibold dark:text-white-light">Tài sản ròng</h2>
          <div class="text-center">
            <p class="text-3xl font-bold mb-4" :class="netWorthClass">
              {{ formattedNetWorth }}
            </p>
            <div class="flex justify-center gap-8">
              <div class="text-center">
                <span class="block text-sm text-white-dark mb-1">Tài sản:</span>
                <span class="text-lg font-semibold text-success">{{ formattedAssets }}</span>
              </div>
              <div class="text-center">
                <span class="block text-sm text-white-dark mb-1">Nợ:</span>
                <span class="text-lg font-semibold text-danger">{{ formattedLiabilities }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions">
        <div class="panel">
          <h3 class="mb-5 text-lg font-semibold dark:text-white-light">Thao tác nhanh</h3>
          <div class="actions-grid">
            <ActionButton
              icon="heroicons:plus"
              label="Thêm giao dịch"
              @click="navigateToTransaction"
            />
            <ActionButton
              icon="heroicons:arrow-path"
              label="Giao dịch định kỳ"
              @click="navigateToRecurring"
            />
            <ActionButton
              icon="heroicons:chart-bar"
              label="Báo cáo"
              @click="navigateToReports"
            />
            <ActionButton
              icon="heroicons:cog-6-tooth"
              label="Cài đặt"
              @click="navigateToSettings"
            />
          </div>
        </div>
      </div>

      <!-- Recent Transactions -->
      <div class="recent-transactions">
        <div class="panel">
          <div class="mb-5 flex items-center justify-between">
            <h3 class="text-lg font-semibold dark:text-white-light">Giao dịch gần đây</h3>
            <NuxtLink to="/transactions" class="text-primary hover:text-primary/80 font-medium text-sm">
              Xem tất cả
            </NuxtLink>
          </div>
          <RecentTransactionList v-if="recentTransactions.length > 0" :transactions="recentTransactions" />
          <div v-else class="text-center py-8">
            <Icon name="heroicons:document-text" size="48" class="text-white-dark mx-auto mb-3" />
            <p class="text-white-dark">Chưa có giao dịch nào</p>
          </div>
        </div>
      </div>

      <!-- Budget Overview -->
      <div class="budget-overview">
        <div class="panel">
          <h3 class="mb-5 text-lg font-semibold dark:text-white-light">Ngân sách tháng này</h3>
          <BudgetProgress v-if="monthlyBudgets.length > 0" :budgets="monthlyBudgets" />
          <div v-else class="text-center py-8">
            <Icon name="heroicons:chart-pie" size="48" class="text-white-dark mx-auto mb-3" />
            <p class="text-white-dark mb-4">Chưa có ngân sách nào được thiết lập</p>
            <button class="btn btn-primary" @click="navigateToBudgetSetup">
              Thiết lập ngân sách
            </button>
          </div>
        </div>
      </div>

      <!-- Spending Calendar -->
      <div class="spending-calendar-section">
        <SpendingCalendar 
          :data="spendingCalendarData || undefined"
          :is-loading="calendarLoading"
          @month-change="handleMonthChange"
          @day-select="handleDaySelect"
          @view-transactions="handleViewDayTransactions"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useDashboard } from '~/composables/useDashboard'
import { useAuthStore } from '~/stores/auth'
import type { 
  FinancialSummary, 
  Budget, 
  RecentTransaction, 
  SpendingCalendarData,
  SpendingCalendarDay
} from '~/types/dashboard'

// Page meta
definePageMeta({
  title: 'Dashboard Tài chính'
  // Bỏ middleware auth tạm thời để test UI
  // middleware: 'auth'
})

// Composables
const { getFinancialSummary, getMonthlyBudgets, getRecentTransactions, calculateNetWorth, getSpendingCalendar } = useDashboard()
const authStore = useAuthStore()
const router = useRouter()

// Reactive state
const loading = ref(true)
const calendarLoading = ref(false)
const selectedPeriod = ref<'thisMonth' | 'lastMonth' | 'thisYear'>('thisMonth')
const selectedScenario = ref<'default' | 'good' | 'struggling' | 'starter'>('default')
const financialData = ref<FinancialSummary | null>(null)
const monthlyBudgets = ref<Budget[]>([])
const recentTransactions = ref<RecentTransaction[]>([])
const spendingCalendarData = ref<SpendingCalendarData | null>(null)

// Computed properties
const netWorth = computed(() => {
  if (!financialData.value) return 0
  return calculateNetWorth(financialData.value)
})

const formattedNetWorth = computed(() => {
  const formatter = new Intl.NumberFormat('vi-VN')
  return `${formatter.format(Math.abs(netWorth.value))} VNĐ`
})

const formattedAssets = computed(() => {
  if (!financialData.value) return '0 VNĐ'
  const assets = financialData.value.totalCash + financialData.value.totalLent
  const formatter = new Intl.NumberFormat('vi-VN')
  return `${formatter.format(assets)} VNĐ`
})

const formattedLiabilities = computed(() => {
  if (!financialData.value) return '0 VNĐ'
  const liabilities = financialData.value.totalDebt + financialData.value.totalBorrowed
  const formatter = new Intl.NumberFormat('vi-VN')
  return `${formatter.format(liabilities)} VNĐ`
})

const netWorthClass = computed(() => {
  if (netWorth.value >= 0) {
    return 'text-success dark:text-success-light'
  } else {
    return 'text-danger dark:text-danger-light'
  }
})

// Methods
const loadDashboardData = async () => {
  try {
    loading.value = true
    
    // Sử dụng mock data thay vì call API để test UI
    const { mockFinancialSummary, mockBudgets, mockRecentTransactions, mockFinancialSummaryVariations } = await import('~/utils/mockData')
    
    // Simulate API delay để test loading state
    await new Promise(resolve => setTimeout(resolve, 1500))
    
    // Chọn financial data dựa trên scenario
    let selectedFinancialData = mockFinancialSummary
    if (selectedScenario.value !== 'default' && mockFinancialSummaryVariations[selectedScenario.value]) {
      selectedFinancialData = mockFinancialSummaryVariations[selectedScenario.value]
    }
    
    financialData.value = selectedFinancialData
    monthlyBudgets.value = mockBudgets
    recentTransactions.value = mockRecentTransactions

    // Load spending calendar cho tháng hiện tại
    await loadSpendingCalendar()

  } catch (error) {
    console.error('Error loading mock data:', error)
  } finally {
    loading.value = false
  }
}

const loadSpendingCalendar = async (month?: number, year?: number) => {
  try {
    calendarLoading.value = true
    const now = new Date()
    const targetMonth = month || (now.getMonth() + 1)
    const targetYear = year || now.getFullYear()
    
    // Mock userId cho development
    const userId = 'mock-user-id'
    
    spendingCalendarData.value = await getSpendingCalendar(userId, targetMonth, targetYear)
  } catch (error) {
    console.error('Error loading spending calendar:', error)
  } finally {
    calendarLoading.value = false
  }
}

const handleMonthChange = async (month: number, year: number) => {
  await loadSpendingCalendar(month, year)
}

const handleDaySelect = (day: SpendingCalendarDay) => {
  console.log('Selected day:', day)
  // Có thể show popup hoặc navigate tới transaction detail
}

const handleViewDayTransactions = (day: SpendingCalendarDay) => {
  // Navigate tới transaction list với filter cho ngày cụ thể
  router.push({
    path: '/transactions',
    query: {
      date: day.date,
      type: 'expense'
    }
  })
}

const switchScenario = () => {
  loadDashboardData()
}

// Navigation methods
const navigateToTransaction = () => {
  router.push('/transactions/new')
}

const navigateToRecurring = () => {
  router.push('/transactions/recurring')
}

const navigateToReports = () => {
  router.push('/reports')
}

const navigateToSettings = () => {
  router.push('/settings')
}

const navigateToBudgetSetup = () => {
  router.push('/budgets/setup')
}

// Lifecycle
onMounted(() => {
  loadDashboardData()
})
</script>

<style scoped>
.dashboard-container {
  padding: 1.5rem;
  max-width: 1200px;
  margin: 0 auto;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.header-controls {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.scenario-selector {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.scenario-label {
  font-size: 0.875rem;
  color: rgb(107 114 128);
  font-weight: 500;
}

.dark .scenario-label {
  color: rgb(156 163 175);
}

.scenario-select {
  padding: 0.5rem 0.75rem;
  border: 1px solid rgb(209 213 219);
  border-radius: 0.5rem;
  background-color: white;
  color: rgb(17 24 39);
  cursor: pointer;
  font-size: 0.875rem;
}

.dark .scenario-select {
  background-color: rgb(31 41 55);
  border-color: rgb(55 65 81);
  color: rgb(243 244 246);
}

.dashboard-title {
  font-size: 2rem;
  font-weight: 700;
  color: rgb(17 24 39);
}

.dark .dashboard-title {
  color: rgb(243 244 246);
}

.date-selector {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.refresh-btn {
  padding: 0.5rem;
  border: 1px solid rgb(209 213 219);
  border-radius: 0.5rem;
  background-color: white;
  color: rgb(59 130 246);
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.dark .refresh-btn {
  background-color: rgb(31 41 55);
  border-color: rgb(55 65 81);
  color: rgb(147 197 253);
}

.refresh-btn:hover:not(:disabled) {
  background-color: rgb(59 130 246);
  color: white;
}

.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.animate-spin {
  animation: spin 1s linear infinite;
}

.period-select {
  padding: 0.5rem 1rem;
  border: 1px solid rgb(209 213 219);
  border-radius: 0.5rem;
  background-color: white;
  color: rgb(17 24 39);
  cursor: pointer;
}

.dark .period-select {
  background-color: rgb(31 41 55);
  border-color: rgb(55 65 81);
  color: rgb(243 244 246);
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem;
  gap: 1rem;
}

.loading-spinner {
  width: 2rem;
  height: 2rem;
  border: 3px solid rgb(229 231 235);
  border-top: 3px solid rgb(59 130 246);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.net-worth-section {
  margin-bottom: 2rem;
}

.net-worth-card {
  background-color: white;
  border: 1px solid rgb(229 231 235);
  border-radius: 0.75rem;
  padding: 2rem;
  text-align: center;
}

.dark .net-worth-card {
  background-color: rgb(31 41 55);
  border-color: rgb(55 65 81);
}

.net-worth-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: rgb(75 85 99);
  margin-bottom: 1rem;
}

.dark .net-worth-title {
  color: rgb(209 213 219);
}

.net-worth-amount {
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
}

.net-worth-positive {
  color: rgb(5 150 105);
}

.net-worth-negative {
  color: rgb(220 38 38);
}

.net-worth-breakdown {
  display: flex;
  justify-content: center;
  gap: 2rem;
}

.breakdown-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.breakdown-label {
  font-size: 0.875rem;
  color: rgb(107 114 128);
}

.dark .breakdown-label {
  color: rgb(156 163 175);
}

.breakdown-value {
  font-size: 1.125rem;
  font-weight: 600;
}

.breakdown-value.positive {
  color: rgb(5 150 105);
}

.breakdown-value.negative {
  color: rgb(220 38 38);
}

.quick-actions,
.recent-transactions,
.budget-overview {
  margin-bottom: 2rem;
}

.section-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: rgb(17 24 39);
  margin-bottom: 1rem;
}

.dark .section-title {
  color: rgb(243 244 246);
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.view-all-link {
  color: rgb(59 130 246);
  text-decoration: none;
  font-weight: 500;
}

.view-all-link:hover {
  text-decoration: underline;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  text-align: center;
  background-color: white;
  border: 1px dashed rgb(209 213 219);
  border-radius: 0.75rem;
}

.dark .empty-state {
  background-color: rgb(31 41 55);
  border-color: rgb(55 65 81);
}

.empty-icon {
  color: rgb(156 163 175);
  margin-bottom: 1rem;
}

.empty-text {
  color: rgb(107 114 128);
  margin-bottom: 1rem;
}

.dark .empty-text {
  color: rgb(156 163 175);
}

.setup-budget-btn {
  padding: 0.5rem 1rem;
  background-color: rgb(59 130 246);
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
}

.setup-budget-btn:hover {
  background-color: rgb(37 99 235);
}

.budget-overview {
  margin-bottom: 2rem;
}

.spending-calendar-section {
  margin-bottom: 2rem;
}

@media (max-width: 768px) {
  .dashboard-container {
    padding: 1rem;
  }
  
  .dashboard-header {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .header-controls {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
    width: 100%;
  }
  
  .scenario-selector {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .summary-grid {
    grid-template-columns: 1fr;
  }
  
  .net-worth-breakdown {
    flex-direction: column;
    gap: 1rem;
  }
  
  .actions-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
