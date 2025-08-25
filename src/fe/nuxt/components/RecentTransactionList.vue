<template>
  <div class="recent-transaction-list space-y-3">
    <div v-for="transaction in transactions" :key="transaction.id" class="transaction-item">
      <div class="flex items-center gap-3">
        <div 
          class="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0"
          :class="getIconClass(transaction.type)"
        >
          <Icon :name="getTransactionIcon(transaction.type)" size="16" />
        </div>
        
        <div class="flex-1 min-w-0">
          <p class="font-medium text-sm dark:text-white-light truncate">{{ transaction.description }}</p>
          <p class="text-xs text-white-dark">{{ transaction.category }}</p>
        </div>
        
        <div class="text-right flex-shrink-0">
          <p class="font-semibold text-sm" :class="getAmountClass(transaction.type, transaction.amount)">
            {{ formatAmount(transaction.amount, transaction.type) }}
          </p>
          <p class="text-xs text-white-dark">{{ formatDate(transaction.date) }}</p>
        </div>
      </div>
    </div>
    
    <div v-if="transactions.length === 0" class="text-center py-8">
      <Icon name="heroicons:document-text" size="32" class="text-white-dark mx-auto mb-2" />
      <p class="text-white-dark">Chưa có giao dịch nào</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { RecentTransaction } from '~/types/dashboard'

interface Props {
  transactions: RecentTransaction[]
}

defineProps<Props>()

const getTransactionIcon = (type: string) => {
  switch (type) {
    case 'income':
      return 'heroicons:arrow-down-circle'
    case 'expense':
      return 'heroicons:arrow-up-circle'
    case 'transfer':
      return 'heroicons:arrow-right-circle'
    default:
      return 'heroicons:document-text'
  }
}

const getIconClass = (type: string) => {
  switch (type) {
    case 'income':
      return 'bg-success-light text-success dark:bg-success dark:text-success-light'
    case 'expense':
      return 'bg-danger-light text-danger dark:bg-danger dark:text-danger-light'
    case 'transfer':
      return 'bg-primary-light text-primary dark:bg-primary dark:text-primary-light'
    default:
      return 'bg-gray-100 text-gray-500'
  }
}

const getAmountClass = (type: string, amount: number) => {
  if (type === 'income' || amount > 0) {
    return 'text-success'
  } else if (type === 'expense' || amount < 0) {
    return 'text-danger'
  } else {
    return 'text-primary'
  }
}

const formatAmount = (amount: number, type: string) => {
  const formatter = new Intl.NumberFormat('vi-VN')
  const prefix = type === 'income' ? '+' : type === 'expense' ? '-' : ''
  return `${prefix}${formatter.format(Math.abs(amount))} VNĐ`
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffTime = Math.abs(now.getTime() - date.getTime())
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))

  if (diffDays === 1) {
    return 'Hôm qua'
  } else if (diffDays === 0) {
    return 'Hôm nay'
  } else if (diffDays <= 7) {
    return `${diffDays} ngày trước`
  } else {
    return date.toLocaleDateString('vi-VN')
  }
}
</script>

<style scoped>
.transaction-item {
  padding: 0.75rem;
  border-radius: 0.5rem;
  border: 1px solid #e0e6ed;
  transition: all 0.2s ease;
}

.transaction-item:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: rgb(99 102 241);
}

.dark .transaction-item {
  background-color: #1b2e4b;
  border-color: #191e3a;
}

.dark .transaction-item:hover {
  border-color: rgb(99 102 241);
}
</style>
