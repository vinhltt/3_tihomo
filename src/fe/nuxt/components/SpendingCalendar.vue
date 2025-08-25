<template>
  <div class="spending-calendar">
    <!-- Header với month navigation -->
    <div class="calendar-header panel p-4 mb-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-3">
          <h3 class="text-lg font-semibold dark:text-white-light">
            Chi tiêu theo ngày
          </h3>
          <div class="text-sm text-white-dark">
            {{ formatMonthYear(currentDate) }}
          </div>
        </div>
        
        <div class="flex items-center gap-2">
          <button 
            @click="previousMonth"
            class="btn btn-outline-primary p-2"
            :disabled="isLoading"
          >
            <Icon name="heroicons:chevron-left" size="16" />
          </button>
          
          <button 
            @click="nextMonth"
            class="btn btn-outline-primary p-2"
            :disabled="isLoading"
          >
            <Icon name="heroicons:chevron-right" size="16" />
          </button>
          
          <button 
            @click="goToToday"
            class="btn btn-primary px-3 py-2 text-sm"
            :disabled="isCurrentMonth"
          >
            Hôm nay
          </button>
        </div>
      </div>
      
      <!-- Summary stats -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mt-4">
        <div class="text-center">
          <div class="text-xs text-white-dark mb-1">Tổng thu</div>
          <div class="text-lg font-semibold text-success">
            {{ formatCurrency(calendarData?.totalIncome || 0) }}
          </div>
        </div>
        <div class="text-center">
          <div class="text-xs text-white-dark mb-1">Tổng chi</div>
          <div class="text-lg font-semibold text-danger">
            {{ formatCurrency(calendarData?.totalExpense || 0) }}
          </div>
        </div>
        <div class="text-center">
          <div class="text-xs text-white-dark mb-1">Lãi/lỗ</div>
          <div class="text-lg font-semibold" :class="(calendarData?.netAmount || 0) >= 0 ? 'text-success' : 'text-danger'">
            {{ formatCurrency(calendarData?.netAmount || 0) }}
          </div>
        </div>
        <div class="text-center">
          <div class="text-xs text-white-dark mb-1">Ngày chi cao nhất</div>
          <div class="text-lg font-semibold text-warning">
            {{ calendarData?.highestExpenseDay ? formatCurrency(calendarData.highestExpenseDay.amount) : '0 VNĐ' }}
          </div>
        </div>
      </div>
    </div>

    <!-- Calendar grid -->
    <div class="calendar-grid panel p-4">
      <!-- Days of week header -->
      <div class="grid grid-cols-7 gap-1 mb-2">
        <div 
          v-for="day in daysOfWeek" 
          :key="day"
          class="text-center text-xs font-medium text-white-dark p-2"
        >
          {{ day }}
        </div>
      </div>
      
      <!-- Calendar days -->
      <div class="grid grid-cols-7 gap-1">
        <div
          v-for="day in calendarDays"
          :key="`${day.date}-${day.isCurrentMonth}`"
          class="calendar-day"
          :class="getDayClasses(day)"
          @click="selectDay(day)"
        >
          <!-- Day number - góc trên bên trái -->
          <div class="day-number">
            {{ getDayNumber(day.date) }}
          </div>
          
          <!-- Spending level indicator - góc trên bên phải -->
          <div v-if="day.hasActivity && day.totalExpense > 0" class="spending-level">
            <div 
              class="spending-dot"
              :class="getSpendingLevelClass(day.level)"
            ></div>
          </div>
          
          <!-- Income and Expense amounts -->
          <div v-if="day.isCurrentMonth && day.hasActivity" class="amounts-container">
            <!-- Income - màu xanh lá -->
            <div v-if="day.totalIncome > 0" class="income-amount">
              +{{ formatCurrencyFull(day.totalIncome) }}
            </div>
            <!-- Expense - màu đỏ, căn lề bên phải -->
            <div v-if="day.totalExpense > 0" class="expense-amount">
              -{{ formatCurrencyFull(day.totalExpense) }}
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Legend -->
    <div class="calendar-legend panel p-4 mt-4">
      <div class="flex items-center justify-between">
        <div class="text-sm font-medium dark:text-white-light">Mức độ chi tiêu:</div>
        <div class="flex items-center gap-4">
          <div class="flex items-center gap-1">
            <div class="w-3 h-3 rounded-full bg-gray-200 dark:bg-gray-600"></div>
            <span class="text-xs text-white-dark">Không</span>
          </div>
          <div class="flex items-center gap-1">
            <div class="w-3 h-3 rounded-full bg-success-light"></div>
            <span class="text-xs text-white-dark">Thấp</span>
          </div>
          <div class="flex items-center gap-1">
            <div class="w-3 h-3 rounded-full bg-warning"></div>
            <span class="text-xs text-white-dark">Trung bình</span>
          </div>
          <div class="flex items-center gap-1">
            <div class="w-3 h-3 rounded-full bg-danger-light"></div>
            <span class="text-xs text-white-dark">Cao</span>
          </div>
          <div class="flex items-center gap-1">
            <div class="w-3 h-3 rounded-full bg-danger"></div>
            <span class="text-xs text-white-dark">Rất cao</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Selected day details modal/popup -->
    <div v-if="selectedDay" class="selected-day-details panel p-4 mt-4">
      <div class="flex items-center justify-between mb-3">
        <h4 class="font-semibold dark:text-white-light">
          Chi tiết ngày {{ formatSelectedDate(selectedDay.date) }}
        </h4>
        <button 
          @click="selectedDay = null"
          class="btn btn-outline-secondary p-1"
        >
          <Icon name="heroicons:x-mark" size="16" />
        </button>
      </div>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <div class="text-sm text-white-dark mb-1">Tổng thu</div>
          <div class="text-xl font-semibold text-success">
            {{ formatCurrency(selectedDay.totalIncome) }}
          </div>
        </div>
        <div>
          <div class="text-sm text-white-dark mb-1">Tổng chi</div>
          <div class="text-xl font-semibold text-danger">
            {{ formatCurrency(selectedDay.totalExpense) }}
          </div>
        </div>
        <div>
          <div class="text-sm text-white-dark mb-1">Lãi/lỗ ròng</div>
          <div class="text-xl font-semibold" :class="selectedDay.netAmount >= 0 ? 'text-success' : 'text-danger'">
            {{ formatCurrency(selectedDay.netAmount) }}
          </div>
        </div>
        <div>
          <div class="text-sm text-white-dark mb-1">Số giao dịch</div>
          <div class="text-xl font-semibold text-primary">
            {{ selectedDay.transactionCount }}
          </div>
        </div>
      </div>
      
      <button 
        @click="viewDayTransactions(selectedDay)"
        class="btn btn-primary w-full mt-4"
      >
        Xem chi tiết giao dịch
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { SpendingCalendarData, SpendingCalendarDay } from '~/types/dashboard'

interface Props {
  data?: SpendingCalendarData
  isLoading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<{
  monthChange: [month: number, year: number]
  daySelect: [day: SpendingCalendarDay]
  viewTransactions: [day: SpendingCalendarDay]
}>()

// Reactive state
const currentDate = ref(new Date())
const selectedDay = ref<SpendingCalendarDay | null>(null)

// Days of week labels
const daysOfWeek = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7']

// Computed properties
const calendarData = computed(() => props.data)

const isCurrentMonth = computed(() => {
  const now = new Date()
  return currentDate.value.getMonth() === now.getMonth() && 
         currentDate.value.getFullYear() === now.getFullYear()
})

const calendarDays = computed(() => {
  if (!calendarData.value) return []
  
  const firstDayOfMonth = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth(), 1)
  const lastDayOfMonth = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 0)
  const firstDayOfWeek = firstDayOfMonth.getDay()
  
  const days: SpendingCalendarDay[] = []
  
  // Add previous month days
  const prevMonth = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 0)
  for (let i = firstDayOfWeek - 1; i >= 0; i--) {
    const date = new Date(prevMonth.getFullYear(), prevMonth.getMonth(), prevMonth.getDate() - i)
    days.push({
      date: date.toISOString().split('T')[0],
      totalIncome: 0,
      totalExpense: 0,
      netAmount: 0,
      transactionCount: 0,
      isCurrentMonth: false,
      isToday: false,
      hasActivity: false,
      level: 'none'
    })
  }
  
  // Add current month days
  for (let day = 1; day <= lastDayOfMonth.getDate(); day++) {
    const date = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth(), day)
    const dateStr = date.toISOString().split('T')[0]
    const dayData = calendarData.value.days.find(d => d.date === dateStr)
    
    days.push({
      date: dateStr,
      totalIncome: dayData?.totalIncome || 0,
      totalExpense: dayData?.totalExpense || 0,
      netAmount: dayData?.netAmount || 0,
      transactionCount: dayData?.transactionCount || 0,
      isCurrentMonth: true,
      isToday: isToday(date),
      hasActivity: (dayData?.totalIncome || 0) > 0 || (dayData?.totalExpense || 0) > 0,
      level: dayData?.level || 'none'
    })
  }
  
  // Add next month days to complete the grid
  const remainingDays = 42 - days.length // 6 rows × 7 days
  for (let day = 1; day <= remainingDays; day++) {
    const date = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, day)
    days.push({
      date: date.toISOString().split('T')[0],
      totalIncome: 0,
      totalExpense: 0,
      netAmount: 0,
      transactionCount: 0,
      isCurrentMonth: false,
      isToday: false,
      hasActivity: false,
      level: 'none'
    })
  }
  
  return days
})

// Methods
const isToday = (date: Date) => {
  const today = new Date()
  return date.toDateString() === today.toDateString()
}

const previousMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 1)
  emit('monthChange', currentDate.value.getMonth() + 1, currentDate.value.getFullYear())
}

const nextMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 1)
  emit('monthChange', currentDate.value.getMonth() + 1, currentDate.value.getFullYear())
}

const goToToday = () => {
  currentDate.value = new Date()
  emit('monthChange', currentDate.value.getMonth() + 1, currentDate.value.getFullYear())
}

const selectDay = (day: SpendingCalendarDay) => {
  if (day.isCurrentMonth) {
    selectedDay.value = day
    emit('daySelect', day)
  }
}

const viewDayTransactions = (day: SpendingCalendarDay) => {
  emit('viewTransactions', day)
}

const getDayNumber = (dateStr: string) => {
  return new Date(dateStr).getDate()
}

const getDayClasses = (day: SpendingCalendarDay) => {
  return {
    'current-month': day.isCurrentMonth,
    'other-month': !day.isCurrentMonth,
    'today': day.isToday,
    'has-activity': day.hasActivity,
    'selected': selectedDay.value?.date === day.date
  }
}

const getSpendingLevelClass = (level: string) => {
  switch (level) {
    case 'low': return 'level-low'
    case 'medium': return 'level-medium'
    case 'high': return 'level-high'
    case 'very-high': return 'level-very-high'
    default: return 'level-none'
  }
}

const formatMonthYear = (date: Date) => {
  return date.toLocaleDateString('vi-VN', { month: 'long', year: 'numeric' })
}

const formatSelectedDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('vi-VN', { 
    day: 'numeric', 
    month: 'long', 
    year: 'numeric' 
  })
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('vi-VN').format(amount) + ' VNĐ'
}

const formatCurrencyFull = (amount: number) => {
  return new Intl.NumberFormat('vi-VN').format(amount)
}

const formatCurrencyShort = (amount: number) => {
  if (amount >= 1000000) {
    return (amount / 1000000).toFixed(1) + 'M'
  } else if (amount >= 1000) {
    return (amount / 1000).toFixed(0) + 'K'
  }
  return amount.toString()
}

// Watch for month changes
watch(() => [currentDate.value.getMonth(), currentDate.value.getFullYear()], () => {
  selectedDay.value = null
})
</script>

<style scoped>
.spending-calendar {
  max-width: 100%;
}

.calendar-grid {
  min-height: 300px;
}

.calendar-day {
  min-height: 100px;
  position: relative;
  padding: 6px;
  border: 1px solid transparent;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
}

.calendar-day:hover {
  background-color: rgb(249 250 251);
  border-color: rgb(99 102 241);
}

.dark .calendar-day:hover {
  background-color: rgb(31 41 55);
}

.calendar-day.current-month {
  color: rgb(17 24 39);
}

.dark .calendar-day.current-month {
  color: rgb(243 244 246);
}

.calendar-day.other-month {
  color: rgb(156 163 175);
  pointer-events: none;
}

.calendar-day.today {
  background-color: rgb(99 102 241);
  color: white;
  font-weight: 600;
}

.calendar-day.today .income-amount,
.calendar-day.today .expense-amount {
  color: rgba(255, 255, 255, 0.9);
}

.calendar-day.selected {
  background-color: rgb(219 234 254);
  border-color: rgb(59 130 246);
}

.dark .calendar-day.selected {
  background-color: rgb(30 58 138);
}

/* Day number - góc trên bên trái */
.day-number {
  position: absolute;
  top: 4px;
  left: 6px;
  font-size: 12px;
  font-weight: 600;
  line-height: 1;
}

/* Spending level - góc trên bên phải */
.spending-level {
  position: absolute;
  top: 4px;
  right: 6px;
}

.spending-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

/* Amounts container - right aligned */
.amounts-container {
  margin-top: auto;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 1px;
  padding-top: 16px;
  padding-right: 2px;
}

.income-amount {
  font-size: 12px;
  color: rgb(34 197 94);
  text-align: right;
  line-height: 1;
  font-weight: 600;
}

.expense-amount {
  font-size: 12px;
  color: rgb(239 68 68);
  text-align: right;
  line-height: 1;
  font-weight: 600;
}

.dark .income-amount {
  color: rgb(74 222 128);
}

.dark .expense-amount {
  color: rgb(248 113 113);
}

.level-none {
  background-color: transparent;
}

.level-low {
  background-color: rgb(34 197 94);
}

.level-medium {
  background-color: rgb(234 179 8);
}

.level-high {
  background-color: rgb(249 115 22);
}

.level-very-high {
  background-color: rgb(239 68 68);
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .calendar-day {
    min-height: 85px;
    padding: 4px;
  }
  
  .day-number {
    font-size: 11px;
    top: 3px;
    left: 4px;
  }
  
  .spending-level {
    top: 3px;
    right: 4px;
  }
  
  .income-amount,
  .expense-amount {
    font-size: 11px;
  }
  
  .spending-dot {
    width: 6px;
    height: 6px;
  }
  
  .amounts-container {
    padding-top: 12px;
  }
}
</style>
