import type { 
  FinancialSummary, 
  Budget, 
  RecentTransaction, 
  SpendingCalendarData,
  SpendingCalendarDay 
} from '~/types/dashboard'

export const mockFinancialSummary: FinancialSummary = {
  totalCash: 45750000, // 45.75 triệu
  totalDebt: 8500000,  // 8.5 triệu
  totalLent: 3200000,  // 3.2 triệu  
  totalBorrowed: 1800000, // 1.8 triệu
  cashChange: 8.2,     // +8.2%
  debtChange: -12.5,   // -12.5%
  lentChange: 15.3,    // +15.3%
  borrowedChange: -25.7 // -25.7%
}

export const mockBudgets: Budget[] = [
  {
    id: '1',
    name: 'Ăn uống',
    spent: 2850000,
    limit: 3000000,
    percentage: 95.0,
    category: 'food',
    description: 'Ăn sáng, trưa, tối và đồ uống'
  },
  {
    id: '2', 
    name: 'Giao thông',
    spent: 750000,
    limit: 1000000,
    percentage: 75.0,
    category: 'transportation',
    description: 'Xăng xe, grab, xe bus'
  },
  {
    id: '3',
    name: 'Giải trí',
    spent: 1350000,
    limit: 1000000,
    percentage: 135.0,
    category: 'entertainment',
    description: 'Phim, game, karaoke, bar'
  },
  {
    id: '4',
    name: 'Mua sắm',
    spent: 450000,
    limit: 1500000,
    percentage: 30.0,
    category: 'shopping',
    description: 'Quần áo, mỹ phẩm, đồ dùng'
  },
  {
    id: '5',
    name: 'Sức khỏe',
    spent: 680000,
    limit: 800000,
    percentage: 85.0,
    category: 'healthcare',
    description: 'Khám bác sĩ, thuốc men, gym'
  }
]

export const mockRecentTransactions: RecentTransaction[] = [
  {
    id: '1',
    date: new Date().toISOString(),
    description: 'Lương tháng 8/2025',
    amount: 18000000,
    type: 'income',
    category: 'Lương',
    accountId: 'acc1',
    accountName: 'Techcombank'
  },
  {
    id: '2',
    date: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(), // 2 hours ago
    description: 'Mua groceries tại CoopMart',
    amount: 320000,
    type: 'expense',
    category: 'Ăn uống',
    accountId: 'acc1',
    accountName: 'Techcombank'
  },
  {
    id: '3',
    date: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(), // yesterday
    description: 'Chuyển tiền cho mẹ',
    amount: 3000000,
    type: 'transfer',
    category: 'Gia đình',
    accountId: 'acc1',
    accountName: 'Techcombank'
  },
  {
    id: '4',
    date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(), // 2 days ago
    description: 'Xăng Shell V-Power',
    amount: 450000,
    type: 'expense',
    category: 'Giao thông',
    accountId: 'acc2',
    accountName: 'VietcomBank'
  },
  {
    id: '5',
    date: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(), // 3 days ago
    description: 'Tiền lãi tiết kiệm',
    amount: 125000,
    type: 'income',
    category: 'Lãi suất',
    accountId: 'acc1',
    accountName: 'Techcombank'
  },
  {
    id: '6',
    date: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000).toISOString(), // 4 days ago
    description: 'Ăn tối tại Sushi Hokkaido',
    amount: 890000,
    type: 'expense',
    category: 'Ăn uống',
    accountId: 'acc1',
    accountName: 'Techcombank'
  },
  {
    id: '7',
    date: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(), // 5 days ago
    description: 'Freelance project payment',
    amount: 2500000,
    type: 'income',
    category: 'Freelance',
    accountId: 'acc2',
    accountName: 'VietcomBank'
  },
  {
    id: '8',
    date: new Date(Date.now() - 6 * 24 * 60 * 60 * 1000).toISOString(), // 6 days ago
    description: 'Mua áo sơ mi Uniqlo',
    amount: 599000,
    type: 'expense',
    category: 'Mua sắm',
    accountId: 'acc1',
    accountName: 'Techcombank'
  }
]

// Mock data cho các variation khác nhau
export const mockFinancialSummaryVariations = {
  // Tình trạng tài chính tốt
  good: {
    totalCash: 75000000,
    totalDebt: 5000000,
    totalLent: 8000000,
    totalBorrowed: 0,
    cashChange: 15.2,
    debtChange: -35.7,
    lentChange: 22.1,
    borrowedChange: -100.0
  } as FinancialSummary,
  
  // Tình trạng tài chính khó khăn
  struggling: {
    totalCash: 2500000,
    totalDebt: 25000000,
    totalLent: 0,
    totalBorrowed: 5000000,
    cashChange: -25.8,
    debtChange: 18.3,
    lentChange: -100.0,
    borrowedChange: 45.2
  } as FinancialSummary,
  
  // Mới bắt đầu
  starter: {
    totalCash: 12000000,
    totalDebt: 0,
    totalLent: 500000,
    totalBorrowed: 0,
    cashChange: 5.0,
    debtChange: 0,
    lentChange: 100.0,
    borrowedChange: 0
  } as FinancialSummary
}

// Mock spending calendar data generator
export const generateMockSpendingCalendar = (month: number, year: number): SpendingCalendarData => {
  const daysInMonth = new Date(year, month, 0).getDate()
  const days: SpendingCalendarDay[] = []
  
  let totalIncome = 0
  let totalExpense = 0
  let highestIncome = 0
  let highestExpense = 0
  let highestIncomeDate = ''
  let highestExpenseDate = ''
  
  for (let day = 1; day <= daysInMonth; day++) {
    const date = new Date(year, month - 1, day)
    const dateStr = date.toISOString().split('T')[0]
    
    // Random activity data với realistic patterns
    const hasActivity = Math.random() > 0.2 // 80% chance of having activity
    let income = 0
    let expense = 0
    let transactionCount = 0
    let level: SpendingCalendarDay['level'] = 'none'
    
    if (hasActivity) {
      // Weekend có xu hướng chi nhiều hơn, thu ít hơn
      const isWeekend = date.getDay() === 0 || date.getDay() === 6
      const dayOfWeek = date.getDay()
      
      // Income patterns - thường có vào đầu tháng, giữa tháng, cuối tháng
      const isPayday = day <= 5 || (day >= 15 && day <= 20) || day >= 25
      if (isPayday && Math.random() > 0.7) {
        // Salary or freelance income
        income = Math.round((Math.random() * 15000000 + 5000000)) // 5M-20M
      } else if (Math.random() > 0.8) {
        // Small income (sell items, etc)
        income = Math.round((Math.random() * 2000000 + 100000)) // 100K-2.1M
      }
      
      // Expense patterns
      if (Math.random() > 0.3) { // 70% chance of expense
        const baseExpense = isWeekend ? 300000 : 150000
        
        // Random multiplier với weighted distribution
        const multipliers = [0.3, 0.7, 1, 1.5, 2.5, 4, 6] // Low to high expense
        const weights = [10, 25, 30, 20, 10, 4, 1] // % distribution
        
        let randomValue = Math.random() * 100
        let selectedMultiplier = 1
        
        for (let i = 0; i < weights.length; i++) {
          if (randomValue <= weights.slice(0, i + 1).reduce((a, b) => a + b, 0)) {
            selectedMultiplier = multipliers[i]
            break
          }
        }
        
        expense = Math.round(baseExpense * selectedMultiplier)
      }
      
      // Transaction count
      transactionCount = Math.floor(Math.random() * 6) + (income > 0 ? 1 : 0) + (expense > 0 ? 1 : 0)
      
      // Determine spending level based on expense amount
      if (expense < 200000) level = 'low'
      else if (expense < 400000) level = 'medium'
      else if (expense < 800000) level = 'high'
      else level = 'very-high'
      
      totalIncome += income
      totalExpense += expense
      
      if (income > highestIncome) {
        highestIncome = income
        highestIncomeDate = dateStr
      }
      
      if (expense > highestExpense) {
        highestExpense = expense
        highestExpenseDate = dateStr
      }
    }
    
    const netAmount = income - expense
    
    days.push({
      date: dateStr,
      totalIncome: income,
      totalExpense: expense,
      netAmount,
      transactionCount,
      isCurrentMonth: true,
      isToday: date.toDateString() === new Date().toDateString(),
      hasActivity,
      level
    })
  }
  
  return {
    month,
    year,
    days,
    totalIncome,
    totalExpense,
    netAmount: totalIncome - totalExpense,
    averageDailyIncome: Math.round(totalIncome / daysInMonth),
    averageDailyExpense: Math.round(totalExpense / daysInMonth),
    highestIncomeDay: highestIncome > 0 ? {
      date: highestIncomeDate,
      amount: highestIncome
    } : null,
    highestExpenseDay: highestExpense > 0 ? {
      date: highestExpenseDate,
      amount: highestExpense
    } : null
  }
}

// Default current month calendar
export const mockSpendingCalendar: SpendingCalendarData = generateMockSpendingCalendar(
  new Date().getMonth() + 1,
  new Date().getFullYear()
)
