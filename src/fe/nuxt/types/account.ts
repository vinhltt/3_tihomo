export type AccountType = 'Bank' | 'Wallet' | 'CreditCard' | 'DebitCard' | 'Cash'

export interface Account {
  id: string
  userId?: string
  name?: string
  type: AccountType
  cardNumber?: string
  currency?: string
  initialBalance: number
  currentBalance: number
  availableLimit?: number
  createAt?: string
  updateAt?: string
  createBy?: string
  updateBy?: string
}

// Alias for consistency
export type AccountViewModel = Account

export interface AccountCreateRequest {
  userId?: string
  name: string
  type: AccountType
  cardNumber?: string
  currency: string
  initialBalance: number
  availableLimit?: number
}

export interface AccountUpdateRequest {
  id: string
  name?: string
  type?: AccountType
  cardNumber?: string
  currency?: string
  availableLimit?: number
}

export interface AccountFilters {
  search?: string
  type?: AccountType
  currency?: string
} 