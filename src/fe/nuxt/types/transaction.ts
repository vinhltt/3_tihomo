// Transaction Direction enum theo design document mới
export const TransactionDirection = {
  Revenue: 1,
  Spent: 2
} as const

export type TransactionDirectionType = typeof TransactionDirection[keyof typeof TransactionDirection]

// Category Type enum khớp với backend
export const CategoryType = {
  Income: 0,
  Expense: 1,
  Transfer: 2,
  Fee: 3,
  Other: 4
} as const

export type CategoryTypeType = typeof CategoryType[keyof typeof CategoryType]

// Transaction ViewModel - sử dụng cấu trúc backend hiện tại
export interface TransactionViewModel {
  id: string
  accountId: string
  userId?: string
  transactionDate: string
  revenueAmount: number
  spentAmount: number
  description?: string
  balance: number
  balanceCompare?: number
  availableLimit?: number
  availableLimitCompare?: number
  transactionCode?: string
  syncMisa: boolean
  syncSms: boolean
  vn: boolean
  categorySummary?: string
  note?: string
  importFrom?: string
  increaseCreditLimit?: number
  usedPercent?: number
  categoryType: CategoryTypeType
  group?: string
  createAt?: string
  updateAt?: string
  createBy?: string
  updateBy?: string
}

// Frontend Request với TransactionDirection và Amount đơn
export interface TransactionCreateRequest {
  accountId: string
  userId?: string
  transactionDate: string
  transactionDirection: TransactionDirectionType
  amount: number
  description?: string
  balance?: number
  balanceCompare?: number
  availableLimit?: number
  availableLimitCompare?: number
  transactionCode?: string
  syncMisa?: boolean
  syncSms?: boolean
  vn?: boolean
  categorySummary?: string
  note?: string
  importFrom?: string
  increaseCreditLimit?: number
  usedPercent?: number
  categoryType: CategoryTypeType
  group?: string
}

export interface TransactionUpdateRequest {
  id: string
  accountId: string
  userId?: string
  transactionDate: string
  transactionDirection: TransactionDirectionType
  amount: number
  description?: string
  balance?: number
  balanceCompare?: number
  availableLimit?: number
  availableLimitCompare?: number
  transactionCode?: string
  syncMisa?: boolean
  syncSms?: boolean
  vn?: boolean
  categorySummary?: string
  note?: string
  importFrom?: string
  increaseCreditLimit?: number
  usedPercent?: number
  categoryType: CategoryTypeType
  group?: string
}

// Computed property để hiển thị số tiền với dấu +/-
export type TransactionDisplayViewModel = TransactionViewModel & {
  displayAmount: string
  isRevenue: boolean
  amountClass: string
}

// Filter cho Transaction
export interface TransactionFilter {
  accountId?: string
  transactionDirection?: TransactionDirectionType
  startDate?: string
  endDate?: string
  categoryType?: CategoryTypeType
}

// Helper functions
export const formatDisplayAmount = (transaction: TransactionViewModel): string => {
  if (transaction.revenueAmount > 0) {
    return `+${transaction.revenueAmount.toLocaleString('vi-VN')} ₫`
  }
  if (transaction.spentAmount > 0) {
    return `-${transaction.spentAmount.toLocaleString('vi-VN')} ₫`
  }
  return '0 ₫'
}

export const getAmountClass = (transaction: TransactionViewModel): string => {
  if (transaction.revenueAmount > 0) return 'text-success'
  if (transaction.spentAmount > 0) return 'text-danger'
  return 'text-dark'
}

export const getTransactionDirection = (transaction: TransactionViewModel): TransactionDirectionType => {
  return transaction.revenueAmount > 0 ? TransactionDirection.Revenue : TransactionDirection.Spent
}

export const getAmount = (transaction: TransactionViewModel): number => {
  return transaction.revenueAmount > 0 ? transaction.revenueAmount : transaction.spentAmount
}

// Convert frontend request to backend format
export const convertToBackendRequest = (frontendRequest: TransactionCreateRequest): any => {
  const backendRequest = {
    ...frontendRequest,
    // Convert datetime-local format to ISO string for backend
    transactionDate: new Date(frontendRequest.transactionDate).toISOString(),
    revenueAmount: frontendRequest.transactionDirection === TransactionDirection.Revenue ? frontendRequest.amount : 0,
    spentAmount: frontendRequest.transactionDirection === TransactionDirection.Spent ? frontendRequest.amount : 0
  }
  delete (backendRequest as any).transactionDirection
  delete (backendRequest as any).amount
  return backendRequest
}

export const convertToBackendUpdateRequest = (frontendRequest: TransactionUpdateRequest): any => {
  const backendRequest = {
    ...frontendRequest,
    // Convert datetime-local format to ISO string for backend
    transactionDate: new Date(frontendRequest.transactionDate).toISOString(),
    revenueAmount: frontendRequest.transactionDirection === TransactionDirection.Revenue ? frontendRequest.amount : 0,
    spentAmount: frontendRequest.transactionDirection === TransactionDirection.Spent ? frontendRequest.amount : 0
  }
  delete (backendRequest as any).transactionDirection
  delete (backendRequest as any).amount
  return backendRequest
} 