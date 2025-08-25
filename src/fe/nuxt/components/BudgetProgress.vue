<template>
  <div class="budget-progress space-y-4">
    <div v-for="budget in budgets" :key="budget.id" class="budget-item">
      <div class="flex items-center justify-between mb-2">
        <div class="flex-1">
          <h6 class="font-medium text-sm dark:text-white-light">{{ budget.name }}</h6>
          <p class="text-xs text-white-dark">
            {{ formatAmount(budget.spent) }} / {{ formatAmount(budget.limit) }}
          </p>
        </div>
        <div class="text-right">
          <span 
            class="inline-block px-2 py-1 text-xs font-medium rounded"
            :class="getPercentageBadgeClass(budget.percentage)"
          >
            {{ budget.percentage.toFixed(0) }}%
          </span>
        </div>
      </div>
      
      <div class="w-full bg-[#ebedf2] dark:bg-dark/40 rounded-full h-2.5 mb-2">
        <div 
          class="h-2.5 rounded-full transition-all duration-300"
          :class="getProgressBarClass(budget.percentage)"
          :style="{ width: `${Math.min(budget.percentage, 100)}%` }"
        ></div>
      </div>
      
      <div v-if="budget.percentage > 100" class="flex items-center text-xs text-danger">
        <Icon name="heroicons:exclamation-triangle" size="14" class="ltr:mr-1 rtl:ml-1" />
        <span>Vượt ngân sách {{ formatAmount(budget.spent - budget.limit) }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Budget {
  id: string
  name: string
  spent: number
  limit: number
  percentage: number
}

interface Props {
  budgets: Budget[]
}

defineProps<Props>()

const formatAmount = (amount: number): string => {
  const formatter = new Intl.NumberFormat('vi-VN')
  return `${formatter.format(amount)} VNĐ`
}

const getPercentageBadgeClass = (percentage: number) => {
  if (percentage < 80) {
    return 'bg-success-light text-success'
  } else if (percentage < 100) {
    return 'bg-warning-light text-warning'
  } else {
    return 'bg-danger-light text-danger'
  }
}

const getProgressBarClass = (percentage: number) => {
  if (percentage < 80) {
    return 'bg-success'
  } else if (percentage < 100) {
    return 'bg-warning'
  } else {
    return 'bg-danger'
  }
}
</script>
