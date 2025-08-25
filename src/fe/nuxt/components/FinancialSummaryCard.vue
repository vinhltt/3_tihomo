<template>
  <div class="panel h-full" :class="panelVariant">
    <div class="mb-5 flex items-center">
      <div 
        class="w-11 h-11 rounded-lg flex items-center justify-center"
        :class="iconClasses"
      >
        <Icon :name="icon" size="20" />
      </div>
      <div class="flex-1 ltr:ml-3 rtl:mr-3">
        <h5 class="text-sm font-medium text-white-dark dark:text-white-light/90">{{ title }}</h5>
        <p class="text-2xl font-bold" :class="amountClass">
          {{ formattedAmount }}
        </p>
      </div>
    </div>
    
    <div class="flex items-center justify-between">
      <div v-if="change !== undefined" class="flex items-center text-xs" :class="changeClass">
        <Icon :name="changeIcon" size="14" class="ltr:mr-1 rtl:ml-1" />
        <span>{{ Math.abs(change) }}%</span>
      </div>
      <div v-if="subtitle" class="text-xs text-white-dark">{{ subtitle }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  title: string
  amount: number
  icon: string
  color?: 'success' | 'danger' | 'primary' | 'warning' | 'info'
  change?: number
  subtitle?: string
  currency?: string
}

const props = withDefaults(defineProps<Props>(), {
  color: 'primary',
  currency: 'VNĐ'
})

// Format số tiền với dấu phẩy và đơn vị tiền tệ
const formattedAmount = computed(() => {
  const formatter = new Intl.NumberFormat('vi-VN')
  return `${formatter.format(Math.abs(props.amount))} ${props.currency}`
})

// Panel variant class dựa trên color
const panelVariant = computed(() => {
  const variants = {
    success: 'border-l-4 border-success',
    danger: 'border-l-4 border-danger', 
    primary: 'border-l-4 border-primary',
    warning: 'border-l-4 border-warning',
    info: 'border-l-4 border-info'
  }
  return variants[props.color]
})

// Icon classes cho VRISTO pattern
const iconClasses = computed(() => {
  const colors = {
    success: 'bg-success-light text-success dark:bg-success dark:text-success-light',
    danger: 'bg-danger-light text-danger dark:bg-danger dark:text-danger-light',
    primary: 'bg-primary-light text-primary dark:bg-primary dark:text-primary-light',
    warning: 'bg-warning-light text-warning dark:bg-warning dark:text-warning-light',
    info: 'bg-info-light text-info dark:bg-info dark:text-info-light'
  }
  return colors[props.color]
})

// Class cho số tiền (success nếu dương, danger nếu âm)
const amountClass = computed(() => {
  if (props.amount >= 0) {
    return 'text-success dark:text-success-light'
  } else {
    return 'text-danger dark:text-danger-light'
  }
})

// Class và icon cho chỉ số thay đổi
const changeClass = computed(() => {
  if (!props.change) return ''
  if (props.change > 0) {
    return 'text-success'
  } else {
    return 'text-danger'
  }
})

const changeIcon = computed(() => {
  if (!props.change) return ''
  return props.change > 0 ? 'heroicons:arrow-trending-up' : 'heroicons:arrow-trending-down'
})
</script>
