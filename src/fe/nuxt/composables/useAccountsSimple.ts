import type { AccountViewModel } from '~/types/account'

export const useAccountsSimple = () => {
  const { get } = useApi()
  
  // Reactive state
  const accounts = ref<AccountViewModel[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  /**
   * Fetch all accounts
   */
  const getAccounts = async () => {
    try {
      isLoading.value = true
      error.value = null

      // Backend exposes selection/paged endpoints for accounts.
      // Call the 'selections' GET endpoint when available. The response
      // may be either an array or a paging object { items: AccountViewModel[] }.
      const response = await get<any>(`/api/core-finance/account/selections`)

      if (!response) {
        accounts.value = []
      } else if (Array.isArray(response)) {
        accounts.value = response as AccountViewModel[]
      } else if (response.items && Array.isArray(response.items)) {
        accounts.value = response.items as AccountViewModel[]
      } else {
        // Fallback: try to coerce possible shapes
        accounts.value = (response as AccountViewModel[]) || []
      }

    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Lỗi khi tải danh sách tài khoản'
      accounts.value = []
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Get account by ID
   */
  const getAccountById = (id: string): AccountViewModel | undefined => {
    return accounts.value.find(account => account.id === id)
  }

  /**
   * Get account name by ID
   */
  const getAccountName = (id: string): string => {
    const account = getAccountById(id)
    return account?.name || 'Không xác định'
  }

  return {
    // State
    accounts: readonly(accounts),
    isLoading: readonly(isLoading),
    error: readonly(error),
    
    // Methods
    getAccounts,
    getAccountById,
    getAccountName
  }
} 