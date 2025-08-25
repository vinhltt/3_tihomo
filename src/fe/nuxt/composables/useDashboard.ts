import { useApi } from './useApi'
import type { 
  FinancialSummary, 
  Budget, 
  RecentTransaction, 
  DashboardStats,
  SpendingCalendarData
} from '~/types/dashboard'
import { generateMockSpendingCalendar } from '~/utils/mockData'

export const useDashboard = () => {
  const { get } = useApi()

  /**
   * Lấy tổng quan tài chính
   */
  const getFinancialSummary = async (userId: string): Promise<FinancialSummary> => {
    return get<FinancialSummary>(`/api/core-finance/Dashboard/financial-summary/${userId}`)
  }

  /**
   * Lấy ngân sách tháng hiện tại
   */
  const getMonthlyBudgets = async (userId: string): Promise<Budget[]> => {
    return get<Budget[]>(`/api/core-finance/Dashboard/monthly-budgets/${userId}`)
  }

  /**
   * Lấy giao dịch gần đây
   */
  const getRecentTransactions = async (userId: string, limit = 5): Promise<RecentTransaction[]> => {
    return get<RecentTransaction[]>(`/api/core-finance/Dashboard/recent-transactions/${userId}?limit=${limit}`)
  }

  /**
   * Tính tài sản ròng
   */
  const calculateNetWorth = (summary: FinancialSummary): number => {
    return summary.totalCash + summary.totalLent - summary.totalDebt - summary.totalBorrowed
  }

  /**
   * Lấy thống kê tổng quan cho một khoảng thời gian
   */
  const getDashboardStats = async (userId: string, period: 'thisMonth' | 'lastMonth' | 'thisYear' = 'thisMonth'): Promise<DashboardStats> => {
    return get<DashboardStats>(`/api/core-finance/Dashboard/stats/${userId}?period=${period}`)
  }

  /**
   * Lấy dữ liệu spending calendar theo tháng
   */
  const getSpendingCalendar = async (userId: string, month: number, year: number): Promise<SpendingCalendarData> => {
    try {
      return await get<SpendingCalendarData>(`/api/core-finance/Dashboard/spending-calendar/${userId}?month=${month}&year=${year}`)
    } catch (error) {
      // Fallback to mock data khi API chưa ready
      console.warn('API not available, using mock data for spending calendar')
      return generateMockSpendingCalendar(month, year)
    }
  }

  return {
    getFinancialSummary,
    getMonthlyBudgets,
    getRecentTransactions,
    calculateNetWorth,
    getDashboardStats,
    getSpendingCalendar
  }
}
