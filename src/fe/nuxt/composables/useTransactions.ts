import type { 
  TransactionViewModel, 
  TransactionCreateRequest, 
  TransactionUpdateRequest,
  TransactionFilter,
  TransactionDirectionType,
  CategoryTypeType 
} from '~/types/transaction'
import type { FilterBodyRequest, ApiResponse } from '~/types/api'
import { FilterLogicalOperator, FilterType, SortDirection } from '~/types/api'
import { 
  TransactionDirection, 
  CategoryType, 
  convertToBackendRequest, 
  convertToBackendUpdateRequest,
  formatDisplayAmount,
  getAmountClass,
  getTransactionDirection,
  getAmount
} from '~/types/transaction'

export const useTransactions = () => {
  const { postForm, putForm, delete: deleteRequest, post } = useApi()
  
  // Reactive state
  const transactions = ref<TransactionViewModel[]>([])
  const selectedTransaction = ref<TransactionViewModel | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const pagination = ref({
    pageIndex: 0,
    pageSize: 20,
    totalRow: 0,
    pageCount: 0
  })

  // Current filters
  const currentFilter = ref<TransactionFilter>({
    startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 30 days ago
    endDate: new Date().toISOString().split('T')[0] // today
  })

  // Get paginated transactions with filtering
  const getTransactions = async (filter?: Partial<TransactionFilter>, page: number = 0, pageSize: number = 20) => {
    try {
      isLoading.value = true
      error.value = null

      // Merge current filter with new filter
      const mergedFilter = { ...currentFilter.value, ...filter }
      
      // Handle clearing filters: if a filter value is empty string, remove it completely
      if (filter) {
        Object.keys(filter).forEach(key => {
          const value = filter[key as keyof TransactionFilter]
          if (value === '' || value === null || value === undefined) {
            delete mergedFilter[key as keyof TransactionFilter]
          }
        })
      }
      
      currentFilter.value = mergedFilter      // Build filter details array
      const filterDetails: any[] = []

      // Add account filter - only when a specific account is selected
      if (mergedFilter.accountId && mergedFilter.accountId.trim() !== '') {
        filterDetails.push({
          attributeName: 'accountId',
          filterType: FilterType.Equal,
          value: mergedFilter.accountId
        })
        console.log('Adding account filter:', mergedFilter.accountId)
      } else {
        console.log('No account filter - loading all accounts transactions')
      }

      if (mergedFilter.startDate && mergedFilter.startDate.trim() !== '') {
        filterDetails.push({
          attributeName: 'transactionDate',
          filterType: FilterType.GreaterThanOrEqual,
          value: mergedFilter.startDate
        })
      }

      if (mergedFilter.endDate && mergedFilter.endDate.trim() !== '') {
        filterDetails.push({
          attributeName: 'transactionDate',
          filterType: FilterType.LessThan,
          value: mergedFilter.endDate + 'T23:59:59' // Ensure we include the whole day
        })
      }

      if (mergedFilter.categoryType !== undefined && mergedFilter.categoryType !== null) {
        filterDetails.push({
          attributeName: 'categoryType',
          filterType: FilterType.Equal,
          value: mergedFilter.categoryType.toString()
        })
      }      // Build FilterBodyRequest
      const filterRequest: FilterBodyRequest = {
        langId: '',
        searchValue: '',
        filter: filterDetails.length > 0 ? {
          logicalOperator: FilterLogicalOperator.And,
          details: filterDetails
        } : {},
        orders: [
          {
            field: 'transactionDate',
            direction: SortDirection.Descending
          }
        ],
        pagination: {
          pageIndex: page,
          pageSize,
          totalRow: 0,
          pageCount: 0
        }
      }

      console.log('Transaction filter request:', {
        accountFilter: mergedFilter.accountId || 'ALL_ACCOUNTS',
        filterDetailsCount: filterDetails.length,
        filterRequest: JSON.stringify(filterRequest, null, 2)
      })

      const response = await post<ApiResponse<TransactionViewModel>>('/api/core-finance/transaction/filter', filterRequest)
      
      if (response?.data) {
        transactions.value = response.data
        pagination.value = response.pagination || pagination.value
      }

      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch transactions'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // Get transaction by ID
  const getTransactionById = async (id: string) => {
    try {
      isLoading.value = true
      error.value = null
        const response = await $fetch<TransactionViewModel>(`/api/core-finance/transaction/${id}`, {
        headers: {
          'Authorization': `Bearer ${useAuthStore().token}`
        }
      })
      
      selectedTransaction.value = response
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch transaction'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // Create transaction
  const createTransaction = async (request: TransactionCreateRequest) => {
    try {
      isLoading.value = true
      error.value = null

      // Convert frontend request to backend format
      const backendRequest = convertToBackendRequest(request)
      
      const response = await postForm<TransactionViewModel>('/api/core-finance/transaction', backendRequest)
      
      if (response) {
        // Refresh the list
        await getTransactions()
      }
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create transaction'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // Update transaction
  const updateTransaction = async (request: TransactionUpdateRequest) => {
    try {
      isLoading.value = true
      error.value = null

      // Convert frontend request to backend format
      const backendRequest = convertToBackendUpdateRequest(request)
      
      const response = await putForm<TransactionViewModel>(`/api/core-finance/transaction/${request.id}`, backendRequest)
      
      if (response) {
        // Update local state
        const index = transactions.value.findIndex((t: TransactionViewModel) => t.id === request.id)
        if (index !== -1) {
          transactions.value[index] = response
        }
        if (selectedTransaction.value?.id === request.id) {
          selectedTransaction.value = response
        }
      }
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update transaction'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // Delete transaction
  const deleteTransaction = async (id: string) => {
    try {
      isLoading.value = true
      error.value = null
      
      await deleteRequest(`/api/core-finance/transaction/${id}`)
      
      // Remove from local state
      transactions.value = transactions.value.filter((t: TransactionViewModel) => t.id !== id)
      if (selectedTransaction.value?.id === id) {
        selectedTransaction.value = null
      }
      
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete transaction'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // Helper functions
  const getDisplayAmount = (transaction: TransactionViewModel) => formatDisplayAmount(transaction)
  const getDisplayClass = (transaction: TransactionViewModel) => getAmountClass(transaction)
  const getDirection = (transaction: TransactionViewModel) => getTransactionDirection(transaction)
  const getAmountValue = (transaction: TransactionViewModel) => getAmount(transaction)

  // Filter helpers
  const filterByAccount = (accountId: string) => {
    getTransactions({ accountId })
  }

  const filterByDirection = (direction: TransactionDirectionType) => {
    getTransactions({ transactionDirection: direction })
  }

  const filterByDateRange = (startDate: string, endDate: string) => {
    getTransactions({ startDate, endDate })
  }

  const clearFilters = () => {
    currentFilter.value = {
      startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      endDate: new Date().toISOString().split('T')[0]
    }
    getTransactions()
  }

  // Form helpers
  const createFormDefaults = (direction?: TransactionDirectionType, accountId?: string): Partial<TransactionCreateRequest> => {
    return {
      transactionDate: new Date().toISOString().slice(0, 16), // Format for datetime-local input
      transactionDirection: direction || TransactionDirection.Spent,
      accountId: accountId || '',
      categoryType: direction === TransactionDirection.Revenue ? CategoryType.Income : CategoryType.Expense,
      syncMisa: false,
      syncSms: false,
      vn: true,
      amount: 0,
      balance: 0
    }
  }

  const resetForm = (form: any) => {
    if (form.amount !== undefined) form.amount = 0
    if (form.description !== undefined) form.description = ''
  }

  // State management helpers
  const setSelectedTransaction = (transaction: TransactionViewModel | null) => {
    selectedTransaction.value = transaction
  }

  return {
    // State
    transactions: readonly(transactions),
    selectedTransaction: readonly(selectedTransaction),
    isLoading: readonly(isLoading),
    error: readonly(error),
    pagination: readonly(pagination),
    currentFilter: readonly(currentFilter),
    
    // Actions
    getTransactions,
    getTransactionById,
    createTransaction,
    updateTransaction,
    deleteTransaction,
    
    // Helper functions
    getDisplayAmount,
    getDisplayClass,
    getDirection,
    getAmountValue,
    
    // Filter helpers
    filterByAccount,
    filterByDirection,
    filterByDateRange,
    clearFilters,
    
    // Form helpers
    createFormDefaults,
    resetForm,
    
    // State management
    setSelectedTransaction,
    
    // Constants
    TransactionDirection,
    CategoryType
  }
} 