import { defineStore } from 'pinia'

// Date utility function để tránh external dependency
function subDays(date: Date, days: number): Date {
  const result = new Date(date)
  result.setDate(result.getDate() - days)
  return result
}

// Types cho Transaction Filter Store
export type TransactionFilterState = {
  selectedAccountId: string | null
  selectedAccountName: string
  dateFrom: Date
  dateTo: Date
  transactionType: 'All' | 'Revenue' | 'Spent'
}

type LoadFromUrlParamsOptions = {
  accountId?: string
  accountName?: string
  dateFrom?: string
  dateTo?: string
  transactionType?: string
}

/**
 * Pinia store để quản lý state filtering cho Transaction Management
 * Supports navigation context từ Account page và direct menu access
 */
export const useTransactionFilterStore = defineStore('transactionFilter', {
  state: (): TransactionFilterState => ({
    selectedAccountId: null,
    selectedAccountName: 'Tất cả tài khoản',
    dateFrom: subDays(new Date(), 30), // Default: 30 ngày gần nhất
    dateTo: new Date(),
    transactionType: 'All'
  }),

  getters: {
    /**
     * Check if có account cụ thể được selected
     */
    hasAccountSelected: (state): boolean => {
      return state.selectedAccountId !== null && state.selectedAccountId !== ''
    },    /**
     * Get current filter summary for display
     */
    filterSummary: (state): string => {
      const accountPart = state.selectedAccountId !== null && state.selectedAccountId !== ''
        ? state.selectedAccountName 
        : 'Tất cả tài khoản'
      
      const datePart = `${state.dateFrom.toLocaleDateString('vi-VN')} - ${state.dateTo.toLocaleDateString('vi-VN')}`
      
      return `${accountPart} | ${datePart}`
    },

    /**
     * Check if filters are in default state
     */
    isDefaultState: (state): boolean => {
      const thirtyDaysAgo = subDays(new Date(), 30)
      const today = new Date()
      const hasAccountSelected = state.selectedAccountId !== null && state.selectedAccountId !== ''
      
      return !hasAccountSelected &&
             state.transactionType === 'All' &&
             Math.abs(state.dateFrom.getTime() - thirtyDaysAgo.getTime()) < 1000 * 60 * 60 * 24 && // Within 1 day
             Math.abs(state.dateTo.getTime() - today.getTime()) < 1000 * 60 * 60 * 24
    }
  },

  actions: {
    /**
     * Set account filter với accountId và accountName
     * Used khi navigate từ Account page hoặc change dropdown
     */
    setAccountFilter(accountId: string | null, accountName: string) {
      this.selectedAccountId = accountId
      this.selectedAccountName = accountName || 'Tất cả tài khoản'
      
      // Reset về "Tất cả tài khoản" nếu accountId null/empty
      if (!accountId) {
        this.selectedAccountName = 'Tất cả tài khoản'
      }
    },

    /**
     * Set date range filter
     * Validates that dateFrom <= dateTo
     */
    setDateRange(dateFrom: Date, dateTo: Date) {
      // Validate date range
      if (dateFrom > dateTo) {
        console.warn('dateFrom cannot be greater than dateTo')
        return
      }

      this.dateFrom = new Date(dateFrom)
      this.dateTo = new Date(dateTo)
    },

    /**
     * Set transaction type filter
     */
    setTransactionType(type: 'All' | 'Revenue' | 'Spent') {
      this.transactionType = type
    },

    /**
     * Reset all filters về default state
     * Used khi user wants to clear filters
     */
    resetToDefault() {
      this.selectedAccountId = null
      this.selectedAccountName = 'Tất cả tài khoản'
      this.dateFrom = subDays(new Date(), 30)
      this.dateTo = new Date()
      this.transactionType = 'All'
    },

    /**
     * Load filter state từ URL parameters
     * Used trong onMounted của transactions.vue
     */
    loadFromUrlParams(params: LoadFromUrlParamsOptions) {
      // Account filter từ URL
      if (params.accountId && params.accountName) {
        this.setAccountFilter(params.accountId, params.accountName)
      } else {
        this.setAccountFilter(null, 'Tất cả tài khoản')
      }

      // Date range từ URL (optional)
      if (params.dateFrom && params.dateTo) {
        try {
          const fromDate = new Date(params.dateFrom)
          const toDate = new Date(params.dateTo)
          
          if (!isNaN(fromDate.getTime()) && !isNaN(toDate.getTime())) {
            this.setDateRange(fromDate, toDate)
          }
        } catch (error) {
          console.warn('Invalid date parameters in URL, using defaults')
          // Keep default dates if URL params are invalid
        }
      }

      // Transaction type từ URL (optional)
      if (params.transactionType && ['All', 'Revenue', 'Spent'].includes(params.transactionType)) {
        this.setTransactionType(params.transactionType as 'All' | 'Revenue' | 'Spent')
      }
    },

    /**
     * Get current filter state as URL query parameters
     * Used để sync URL với store state
     */
    toUrlParams(): Record<string, string> {
      const params: Record<string, string> = {}

      // Add account params if selected
      if (this.hasAccountSelected && this.selectedAccountId) {
        params.accountId = this.selectedAccountId
        params.accountName = this.selectedAccountName
      }

      // Add date params if not default
      if (!this.isDefaultState) {
        params.dateFrom = this.dateFrom.toISOString().split('T')[0] // YYYY-MM-DD format
        params.dateTo = this.dateTo.toISOString().split('T')[0]
      }

      // Add transaction type if not 'All'
      if (this.transactionType !== 'All') {
        params.transactionType = this.transactionType
      }

      return params
    },

    /**
     * Quick preset: Last 7 days
     */
    setLast7Days() {
      this.setDateRange(subDays(new Date(), 7), new Date())
    },

    /**
     * Quick preset: Last 30 days (default)
     */
    setLast30Days() {
      this.setDateRange(subDays(new Date(), 30), new Date())
    },

    /**
     * Quick preset: Current month
     */
    setCurrentMonth() {
      const now = new Date()
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1)
      const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0)
      
      this.setDateRange(firstDay, lastDay)
    },

    /**
     * Quick preset: Last month
     */
    setLastMonth() {
      const now = new Date()
      const firstDay = new Date(now.getFullYear(), now.getMonth() - 1, 1)
      const lastDay = new Date(now.getFullYear(), now.getMonth(), 0)
      
      this.setDateRange(firstDay, lastDay)
    }
  }
})
