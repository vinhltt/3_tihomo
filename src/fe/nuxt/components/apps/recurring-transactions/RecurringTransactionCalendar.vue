<template>
    <div class="panel">
        <div class="mb-5 flex items-center justify-between">
            <h5 class="text-lg font-semibold dark:text-white-light">Lịch Giao dịch Định kỳ</h5>
            <div class="flex items-center gap-3">
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    @click="goToPreviousMonth"
                >
                    <icon-chevron-left class="w-4 h-4" />
                    Tháng trước
                </button>
                <span class="text-lg font-medium">{{ currentMonthYear }}</span>
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    @click="goToNextMonth"
                >
                    Tháng sau
                    <icon-chevron-right class="w-4 h-4" />
                </button>
            </div>
        </div>

        <!-- Calendar View -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow">
            <!-- Calendar Header -->
            <div class="grid grid-cols-7 gap-px bg-gray-200 dark:bg-gray-700 p-px rounded-t-lg">
                <div
                    v-for="day in weekdays"
                    :key="day"
                    class="bg-gray-50 dark:bg-gray-600 py-2 text-center text-sm font-medium text-gray-700 dark:text-gray-300"
                >
                    {{ day }}
                </div>
            </div>

            <!-- Calendar Body -->
            <div class="grid grid-cols-7 gap-px bg-gray-200 dark:bg-gray-700 p-px">
                <div
                    v-for="day in calendarDays"
                    :key="`${day.date}`"
                    :class="[
                        'bg-white dark:bg-gray-800 min-h-[120px] p-2',
                        {
                            'bg-gray-50 dark:bg-gray-900': !day.isCurrentMonth,
                            'ring-2 ring-primary': day.isToday
                        }
                    ]"
                >
                    <!-- Day Number -->
                    <div class="flex justify-between items-start mb-1">
                        <span
                            :class="[
                                'text-sm font-medium',
                                {
                                    'text-gray-400 dark:text-gray-600': !day.isCurrentMonth,
                                    'text-primary font-bold': day.isToday,
                                    'text-gray-900 dark:text-gray-100': day.isCurrentMonth && !day.isToday
                                }
                            ]"
                        >
                            {{ day.dayNumber }}
                        </span>
                        <div
                            v-if="day.events.length > 0"
                            class="text-xs text-gray-500 bg-gray-100 dark:bg-gray-700 px-1 rounded"
                        >
                            {{ day.events.length }}
                        </div>
                    </div>

                    <!-- Events -->
                    <div class="space-y-1">
                        <div
                            v-for="event in day.events.slice(0, 3)"
                            :key="event.id"
                            :class="[
                                'text-xs p-1 rounded cursor-pointer truncate',
                                event.transactionType === 0 
                                    ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                                    : 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300'
                            ]"
                            @click="showEventDetails(event)"
                            :title="`${event.name}: ${formatCurrency(event.amount)}`"
                        >
                            {{ event.name }}
                        </div>
                        <div
                            v-if="day.events.length > 3"
                            class="text-xs text-gray-500 cursor-pointer hover:text-gray-700"
                            @click="showAllEventsForDay(day)"
                        >
                            +{{ day.events.length - 3 }} thêm
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Legend -->
        <div class="mt-4 flex items-center justify-center gap-6">
            <div class="flex items-center gap-2">
                <div class="w-3 h-3 bg-green-100 rounded border border-green-300"></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Thu nhập</span>
            </div>
            <div class="flex items-center gap-2">
                <div class="w-3 h-3 bg-red-100 rounded border border-red-300"></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Chi phí</span>
            </div>
            <div class="flex items-center gap-2">
                <div class="w-3 h-3 bg-blue-100 rounded border border-blue-300"></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Hôm nay</span>
            </div>
        </div>
    </div>

    <!-- Event Details Modal -->
    <div
        v-if="selectedEvent"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
        @click="closeEventDetails"
    >
        <div
            class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full mx-4"
            @click.stop
        >
            <div class="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700">
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Chi tiết Giao dịch</h3>
                <button
                    type="button"
                    class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                    @click="closeEventDetails"
                >
                    <icon-x class="w-6 h-6" />
                </button>
            </div>
            <div class="p-4 space-y-3">
                <div>
                    <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Tên giao dịch</label>
                    <p class="text-gray-900 dark:text-gray-100">{{ selectedEvent.name }}</p>
                </div>
                <div v-if="selectedEvent.description">
                    <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Mô tả</label>
                    <p class="text-gray-900 dark:text-gray-100">{{ selectedEvent.description }}</p>
                </div>
                <div>
                    <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Số tiền</label>
                    <p
                        :class="[
                            'font-semibold',
                            selectedEvent.transactionType === 0 ? 'text-green-600' : 'text-red-600'
                        ]"
                    >
                        {{ selectedEvent.transactionType === 0 ? '+' : '-' }}{{ formatCurrency(selectedEvent.amount) }}
                    </p>
                </div>
                <div>
                    <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Ngày thực hiện</label>
                    <p class="text-gray-900 dark:text-gray-100">{{ formatDate(selectedEvent.nextExecutionDate) }}</p>
                </div>
                <div v-if="selectedEvent.category">
                    <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Danh mục</label>
                    <p class="text-gray-900 dark:text-gray-100">{{ selectedEvent.category }}</p>
                </div>
            </div>
            <div class="flex justify-end p-4 border-t border-gray-200 dark:border-gray-700">
                <button type="button" class="btn btn-primary" @click="closeEventDetails">
                    Đóng
                </button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRecurringTransactions } from '~/composables/useRecurringTransactions'
import type { RecurringTransactionCalendarEvent } from '~/types'

// Icons
import IconChevronLeft from '~/components/icon/icon-chevron-left.vue'
import IconChevronRight from '~/components/icon/icon-chevron-right.vue'
import IconX from '~/components/icon/icon-x.vue'

interface CalendarDay {
    date: string
    dayNumber: number
    isCurrentMonth: boolean
    isToday: boolean
    events: RecurringTransactionCalendarEvent[]
}

// Composables
const { getCalendarEvents } = useRecurringTransactions()

// Reactive data
const currentDate = ref(new Date())
const calendarEvents = ref<RecurringTransactionCalendarEvent[]>([])
const loading = ref(false)
const selectedEvent = ref<RecurringTransactionCalendarEvent | null>(null)

// Computed
const currentMonthYear = computed(() => {
    return currentDate.value.toLocaleDateString('vi-VN', {
        month: 'long',
        year: 'numeric'
    })
})

const weekdays = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7']

const calendarDays = computed((): CalendarDay[] => {
    const year = currentDate.value.getFullYear()
    const month = currentDate.value.getMonth()
    
    // First day of the month
    const firstDay = new Date(year, month, 1)
    // Last day of the month
    const lastDay = new Date(year, month + 1, 0)
    
    // First day to show (might be from previous month)
    const startDate = new Date(firstDay)
    startDate.setDate(startDate.getDate() - firstDay.getDay())
    
    // Last day to show (might be from next month)
    const endDate = new Date(lastDay)
    const daysToShow = 42 // 6 weeks
    endDate.setDate(lastDay.getDate() + (6 - lastDay.getDay()))
    
    const days: CalendarDay[] = []
    const currentDateObj = new Date(startDate)
    
    for (let i = 0; i < daysToShow; i++) {
        const dayEvents = calendarEvents.value.filter(event => {
            const eventDate = new Date(event.nextExecutionDate)
            return (
                eventDate.getDate() === currentDateObj.getDate() &&
                eventDate.getMonth() === currentDateObj.getMonth() &&
                eventDate.getFullYear() === currentDateObj.getFullYear()
            )
        })

        const today = new Date()
        const isToday = (
            currentDateObj.getDate() === today.getDate() &&
            currentDateObj.getMonth() === today.getMonth() &&
            currentDateObj.getFullYear() === today.getFullYear()
        )

        days.push({
            date: currentDateObj.toISOString().split('T')[0],
            dayNumber: currentDateObj.getDate(),
            isCurrentMonth: currentDateObj.getMonth() === month,
            isToday,
            events: dayEvents
        })
        
        currentDateObj.setDate(currentDateObj.getDate() + 1)
    }
    
    return days
})

// Methods
const loadCalendarEvents = async () => {
    try {
        loading.value = true
        const year = currentDate.value.getFullYear()
        const month = currentDate.value.getMonth() + 1
        const monthString = `${year}-${month.toString().padStart(2, '0')}`
        
        const events = await getCalendarEvents(monthString)
        calendarEvents.value = events || []
    } catch (error) {
        console.error('Error loading calendar events:', error)
        calendarEvents.value = []
    } finally {
        loading.value = false
    }
}

const goToPreviousMonth = () => {
    const newDate = new Date(currentDate.value)
    newDate.setMonth(newDate.getMonth() - 1)
    currentDate.value = newDate
}

const goToNextMonth = () => {
    const newDate = new Date(currentDate.value)
    newDate.setMonth(newDate.getMonth() + 1)
    currentDate.value = newDate
}

const showEventDetails = (event: RecurringTransactionCalendarEvent) => {
    selectedEvent.value = event
}

const closeEventDetails = () => {
    selectedEvent.value = null
}

const showAllEventsForDay = (day: CalendarDay) => {
    // Could implement a modal showing all events for the day
    console.log('Show all events for day:', day.date, day.events)
}

// Utility functions
const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount)
}

const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    })
}

// Watchers
watch(currentDate, () => {
    loadCalendarEvents()
}, { immediate: false })

// Lifecycle
onMounted(() => {
    loadCalendarEvents()
})
</script>

<style scoped>
.panel {
    @apply bg-white dark:bg-gray-800 rounded-lg shadow p-6;
}
</style>