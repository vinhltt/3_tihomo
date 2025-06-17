import type { 
  Account, 
  AccountType, 
  AccountCreateRequest, 
  AccountUpdateRequest, 
  AccountFilters,
  Pagination,
  FilterBodyRequest,
  ApiResponse 
} from '~/types'
import { useApi } from './useApi'

export const useAccounts = () => {
  const { get, post, postForm, put, putForm, delete: del } = useApi()

  /**
   * Fetch accounts with pagination and filters
   */
  const getAccounts = async (request: FilterBodyRequest): Promise<ApiResponse<Account>> => {
    return post<ApiResponse<Account>>('/api/account/filter', request)
  }

  /**
   * Get account by ID
   */
  const getAccountById = async (id: string): Promise<Account> => {
    return get<Account>(`/api/account/${id}`)
  }

  /**
   * Create new account
   */
  const createAccount = async (request: AccountCreateRequest): Promise<Account> => {
    return postForm<Account>('/api/account', request)
  }

  /**
   * Update existing account
   */
  const updateAccount = async (id: string, request: AccountUpdateRequest): Promise<Account> => {
    return putForm<Account>(`/api/account/${id}`, request)
  }

  /**
   * Delete account (soft delete)
   */
  const deleteAccount = async (id: string): Promise<void> => {
    return del<void>(`/api/account/${id}`)
  }

  /**
   * Format account type for display
   */
  const formatAccountType = (type: AccountType): string => {
    const typeMap: Record<AccountType, string> = {
      Bank: 'Bank Account',
      Wallet: 'Digital Wallet',
      CreditCard: 'Credit Card', 
      DebitCard: 'Debit Card',
      Cash: 'Cash'
    }
    return typeMap[type] || type
  }

  /**
   * Get badge class for account type
   */
  const getAccountTypeBadgeClass = (type: AccountType): string => {
    const classMap: Record<AccountType, string> = {
      Bank: 'badge-outline-primary',
      Wallet: 'badge-outline-success',
      CreditCard: 'badge-outline-warning',
      DebitCard: 'badge-outline-info',
      Cash: 'badge-outline-secondary'
    }
    return classMap[type] || 'badge-outline-primary'
  }

  /**
   * Mask card number for security
   */
  const maskCardNumber = (cardNumber: string): string => {
    if (!cardNumber || cardNumber.length < 4) return cardNumber
    return '**** **** **** ' + cardNumber.slice(-4)
  }

  /**
   * Format currency amount
   */
  const formatCurrency = (amount: number, currency = 'USD'): string => {
    try {
      const formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency,
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
      })
      return formatter.format(amount)
    } catch {
      // Fallback if currency is not supported
      return `${currency} ${amount.toLocaleString()}`
    }
  }

  /**
   * Get balance text color class
   */
  const getBalanceClass = (balance: number): string => {
    if (balance > 0) return 'text-green-600'
    if (balance < 0) return 'text-red-600'
    return 'text-gray-600'
  }

  /**
   * Get currency symbol
   */
  const getCurrencySymbol = (currency: string): string => {
    const symbols: Record<string, string> = {
      VND: '₫',
      USD: '$',
      EUR: '€',
      JPY: '¥',
      GBP: '£'
    }
    return symbols[currency] || currency
  }

  /**
   * Format date for display
   */
  const formatDate = (dateString?: string): string => {
    if (!dateString) return '-'
    try {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      })
    } catch {
      return '-'
    }
  }

  /**
   * Validate account form data
   */
  const validateAccountForm = (data: Partial<AccountCreateRequest | AccountUpdateRequest>): Record<string, string> => {
    const errors: Record<string, string> = {}

    // Name validation
    if (!data.name?.trim()) {
      errors.name = 'Account name is required'
    } else if (data.name.length < 2) {
      errors.name = 'Account name must be at least 2 characters'
    }

    // Type validation  
    if (!data.type) {
      errors.type = 'Account type is required'
    }

    // Currency validation
    if (!data.currency) {
      errors.currency = 'Currency is required'
    }

    // Card number validation for card types
    if (['CreditCard', 'DebitCard'].includes(data.type as string) && data.cardNumber) {
      const cardNumberClean = data.cardNumber.replace(/\s/g, '')
      if (cardNumberClean.length < 13 || cardNumberClean.length > 19) {
        errors.cardNumber = 'Card number must be between 13-19 digits'
      }
    }

    // Initial balance validation (for create requests)
    if ('initialBalance' in data && typeof data.initialBalance === 'number' && data.initialBalance < 0) {
      errors.initialBalance = 'Initial balance cannot be negative'
    }

    // Available limit validation
    if (data.availableLimit !== undefined && data.availableLimit < 0) {
      errors.availableLimit = 'Available limit cannot be negative'
    }

    return errors
  }

  return {
    // API methods
    getAccounts,
    getAccountById,
    createAccount,
    updateAccount,
    deleteAccount,
    
    // Utility methods
    formatAccountType,
    getAccountTypeBadgeClass,
    maskCardNumber,
    formatCurrency,
    getBalanceClass,
    getCurrencySymbol,
    formatDate,
    validateAccountForm
  }
} 