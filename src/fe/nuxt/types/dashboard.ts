export interface FinancialSummary {
  totalCash: number
  totalDebt: number
  totalLent: number
  totalBorrowed: number
  cashChange?: number
  debtChange?: number
  lentChange?: number
  borrowedChange?: number
}

export interface Budget {
  id: string
  name: string
  spent: number
  limit: number
  percentage: number
  category?: string
  description?: string
}

export interface RecentTransaction {
  id: string
  date: string
  description: string
  amount: number
  type: 'income' | 'expense' | 'transfer'
  category: string
  accountId?: string
  accountName?: string
}

export interface DashboardStats {
  totalIncome: number
  totalExpense: number
  netIncome: number
  transactionCount: number
  averageTransaction: number
  topCategory?: {
    name: string
    amount: number
    percentage: number
  }
}

export interface QuickAction {
  id: string
  label: string
  icon: string
  action: string
  description?: string
}

export interface NetWorthData {
  current: number
  previous?: number
  change?: number
  changePercentage?: number
  trend: 'up' | 'down' | 'stable'
}

export interface SpendingCalendarDay {
  date: string
  totalIncome: number
  totalExpense: number
  netAmount: number
  transactionCount: number
  isCurrentMonth: boolean
  isToday: boolean
  hasActivity: boolean
  level: 'none' | 'low' | 'medium' | 'high' | 'very-high'
}

export interface SpendingCalendarData {
  month: number
  year: number
  days: SpendingCalendarDay[]
  totalIncome: number
  totalExpense: number
  netAmount: number
  averageDailyIncome: number
  averageDailyExpense: number
  highestIncomeDay: {
    date: string
    amount: number
  } | null
  highestExpenseDay: {
    date: string
    amount: number
  } | null
}

export interface DashboardData {
  financialSummary: FinancialSummary
  budgets: Budget[]
  recentTransactions: RecentTransaction[]
  stats: DashboardStats
  netWorth: NetWorthData
  spendingCalendar?: SpendingCalendarData
}
