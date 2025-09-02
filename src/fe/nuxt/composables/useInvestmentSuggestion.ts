/**
 * Composable for handling investment suggestions from transactions
 * Tích hợp suggestion để tạo Investment từ giao dịch có CategoryType.Investment
 */

import type { TransactionViewModel } from '~/types/transaction'

export interface InvestmentSuggestionRequest {
  symbol: string
  name: string
  initialInvestment: number
  quantity: number
  pricePerUnit: number
  transactionId?: string
  description?: string
}

export const useInvestmentSuggestion = () => {
  const isCreating = ref(false)
  const error = ref<string | null>(null)

  /**
   * Tạo Investment record từ transaction suggestion
   */
  const createInvestmentFromTransaction = async (
    transaction: TransactionViewModel,
    suggestionData: Omit<InvestmentSuggestionRequest, 'transactionId' | 'initialInvestment'>
  ) => {
    try {
      isCreating.value = true
      error.value = null

      // Calculate initial investment from transaction amounts
      const initialInvestment = transaction.spentAmount > 0 
        ? transaction.spentAmount 
        : transaction.revenueAmount

      const request: InvestmentSuggestionRequest = {
        ...suggestionData,
        initialInvestment,
        transactionId: transaction.id,
        description: suggestionData.description || transaction.description || `Investment từ giao dịch ${transaction.transactionCode || transaction.id}`
      }

      const data = await $fetch('/api/planninginvestment', {
        method: 'POST',
        body: request
      }) as any

      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không thể tạo investment record'
      throw err
    } finally {
      isCreating.value = false
    }
  }

  /**
   * Parse transaction description để suggest investment details
   */
  const parseTransactionForInvestment = (transaction: TransactionViewModel) => {
    const description = transaction.description?.toLowerCase() || ''
    
    // Common patterns for stock transactions
    const stockPatterns = [
      /mua\s+(\d+)\s+cp\s+([a-z0-9]+)/i, // "mua 100 cp VIC"
      /buy\s+(\d+)\s+([a-z0-9]+)/i,      // "buy 100 VIC"
      /(\d+)\s+([a-z0-9]+)\s+stock/i,    // "100 VIC stock"
    ]

    for (const pattern of stockPatterns) {
      const match = description.match(pattern)
      if (match) {
        const quantity = parseInt(match[1])
        const symbol = match[2].toUpperCase()
        const amount = transaction.spentAmount > 0 ? transaction.spentAmount : transaction.revenueAmount
        const pricePerUnit = amount / quantity

        return {
          symbol,
          name: `Cổ phiếu ${symbol}`,
          quantity,
          pricePerUnit,
          suggested: true
        }
      }
    }

    // Default suggestion for manual entry
    const amount = transaction.spentAmount > 0 ? transaction.spentAmount : transaction.revenueAmount
    return {
      symbol: '',
      name: '',
      quantity: 1,
      pricePerUnit: amount,
      suggested: false
    }
  }

  /**
   * Check if transaction is eligible for investment suggestion
   */
  const isEligibleForInvestment = (transaction: TransactionViewModel) => {
    return transaction.categoryType === 4 && // CategoryType.Investment
           (transaction.spentAmount > 0 || transaction.revenueAmount > 0)
  }

  return {
    isCreating: readonly(isCreating),
    error: readonly(error),
    createInvestmentFromTransaction,
    parseTransactionForInvestment,
    isEligibleForInvestment
  }
}
