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
        // Use simple get request to get all accounts via Gateway
      const response = await get<AccountViewModel[]>('/api/core-finance/account')
      accounts.value = response || []
      
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