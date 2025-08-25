<template>
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
            <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                    {{ isEditMode ? 'Chỉnh sửa mẫu giao dịch định kỳ' : 'Tạo mẫu giao dịch định kỳ mới' }}
                </h3>
                <button 
                    type="button" 
                    class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                    @click="$emit('close')"
                >
                    <icon-x class="w-6 h-6" />
                </button>
            </div>

            <form @submit.prevent="handleSubmit" class="p-6 space-y-6">
                <!-- Basic Information -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label class="form-label">Tên mẫu *</label>
                        <input 
                            v-model="form.name" 
                            type="text" 
                            class="form-input" 
                            placeholder="Ví dụ: Tiền điện hàng tháng"
                            required
                        />
                    </div>
                    <div>
                        <label class="form-label">Tài khoản *</label>
                        <select v-model="form.accountId" class="form-select" required>
                            <option value="">Chọn tài khoản</option>
                            <option v-for="account in accountsList" :key="account.id" :value="account.id">
                                {{ account.name }}
                            </option>
                        </select>
                    </div>
                </div>

                <div>
                    <label class="form-label">Mô tả</label>
                    <textarea 
                        v-model="form.description" 
                        class="form-textarea" 
                        rows="3"
                        placeholder="Mô tả chi tiết về giao dịch định kỳ này"
                    ></textarea>
                </div>

                <!-- Transaction Details -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label class="form-label">Loại giao dịch *</label>
                        <select v-model="form.transactionType" class="form-select" required>
                            <option :value="0">Thu nhập</option>
                            <option :value="1">Chi phí</option>
                        </select>
                    </div>
                    <div>
                        <label class="form-label">Số tiền *</label>
                        <input 
                            v-model.number="form.amount" 
                            type="number" 
                            step="0.01"
                            min="0"
                            class="form-input" 
                            placeholder="0.00"
                            required
                        />
                    </div>
                </div>

                <div>
                    <label class="form-label">Danh mục</label>
                    <input 
                        v-model="form.category" 
                        type="text" 
                        class="form-input" 
                        placeholder="Ví dụ: Hóa đơn, Giải trí, Lương"
                    />
                </div>

                <!-- Frequency Settings -->
                <div class="border-t border-gray-200 dark:border-gray-700 pt-6">
                    <h4 class="text-md font-medium text-gray-900 dark:text-white mb-4">Cài đặt tần suất</h4>
                    
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="form-label">Tần suất lặp lại *</label>
                            <select v-model="form.frequency" class="form-select" required @change="handleFrequencyChange">
                                <option :value="0">Hàng ngày</option>
                                <option :value="1">Hàng tuần</option>
                                <option :value="2">Hai tuần một lần</option>
                                <option :value="3">Hàng tháng</option>
                                <option :value="4">Hàng quý</option>
                                <option :value="5">Nửa năm</option>
                                <option :value="6">Hàng năm</option>
                                <option :value="7">Tùy chỉnh</option>
                            </select>
                        </div>
                        <div v-if="form.frequency === 7">
                            <label class="form-label">Số ngày tùy chỉnh *</label>
                            <input 
                                v-model.number="form.customIntervalDays" 
                                type="number" 
                                min="1"
                                class="form-input" 
                                placeholder="Số ngày"
                                required
                            />
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                        <div>
                            <label class="form-label">Ngày bắt đầu *</label>
                            <input 
                                v-model="form.startDate" 
                                type="date" 
                                class="form-input" 
                                required
                            />
                        </div>
                        <div>
                            <label class="form-label">Ngày kết thúc</label>
                            <input 
                                v-model="form.endDate" 
                                type="date" 
                                class="form-input"
                            />
                        </div>
                    </div>
                </div>

                <!-- Advanced Settings -->
                <div class="border-t border-gray-200 dark:border-gray-700 pt-6">
                    <h4 class="text-md font-medium text-gray-900 dark:text-white mb-4">Cài đặt nâng cao</h4>
                    
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="form-label">Số ngày tạo trước</label>
                            <input 
                                v-model.number="form.daysInAdvance" 
                                type="number" 
                                min="1"
                                max="365"
                                class="form-input" 
                                placeholder="30"
                            />
                            <p class="text-xs text-gray-500 mt-1">Số ngày trước để tạo giao dịch dự kiến</p>
                        </div>
                        <div class="space-y-3">
                            <label class="flex items-center">
                                <input 
                                    v-model="form.isActive" 
                                    type="checkbox" 
                                    class="form-checkbox"
                                />
                                <span class="ml-2 text-sm">Kích hoạt mẫu</span>
                            </label>
                            <label class="flex items-center">
                                <input 
                                    v-model="form.autoGenerate" 
                                    type="checkbox" 
                                    class="form-checkbox"
                                />
                                <span class="ml-2 text-sm">Tự động tạo giao dịch dự kiến</span>
                            </label>
                        </div>
                    </div>

                    <div class="mt-4">
                        <label class="form-label">Ghi chú</label>
                        <textarea 
                            v-model="form.notes" 
                            class="form-textarea" 
                            rows="3"
                            placeholder="Ghi chú bổ sung về mẫu giao dịch này"
                        ></textarea>
                    </div>
                </div>

                <!-- Action Buttons -->
                <div class="flex items-center justify-end space-x-3 pt-6 border-t border-gray-200 dark:border-gray-700">
                    <button 
                        type="button" 
                        class="btn btn-outline-secondary"
                        @click="$emit('close')"
                    >
                        Hủy
                    </button>
                    <button 
                        type="submit" 
                        class="btn btn-primary"
                        :disabled="loading"
                    >
                        <div v-if="loading" class="inline-block animate-spin border-2 border-transparent border-l-white rounded-full w-4 h-4 mr-2"></div>
                        {{ isEditMode ? 'Cập nhật' : 'Tạo mẫu' }}
                    </button>
                </div>
                <div v-if="submitError" class="mt-3 text-sm text-red-600 dark:text-red-400">
                    {{ submitError }}
                </div>
            </form>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRecurringTransactions } from '~/composables/useRecurringTransactions'
import { useAccountsSimple } from '~/composables/useAccountsSimple'
import type { RecurringTransactionTemplateViewModel, RecurringTransactionTemplateCreateRequest, RecurringTransactionTemplateUpdateRequest } from '~/types'
import IconX from '~/components/icon/icon-x.vue'

// Props
interface Props {
    template?: RecurringTransactionTemplateViewModel | null
    // allow readonly arrays coming from composables (.value is readonly)
    accounts: ReadonlyArray<any>
}

const props = withDefaults(defineProps<Props>(), {
    template: null
})

// Emits
const emit = defineEmits<{
    close: []
    saved: []
}>()

// Composables
const { createTemplate, updateTemplate } = useRecurringTransactions()
const { accounts: accountsFromStore, getAccounts: fetchAccounts } = useAccountsSimple()

// Use accounts prop if provided, otherwise fallback to composable's accounts
const accountsList = computed(() => (props.accounts && props.accounts.length ? props.accounts : accountsFromStore as any))

// Reactive data
const loading = ref(false)
const submitError = ref<string | null>(null)

const extractErrorMessage = (err: any): string => {
    if (!err) return 'Lỗi không xác định'
    // Axios-style error with response payload
    const resp = err?.response?.data
    if (resp) {
        if (typeof resp === 'string') return resp
        if (resp.message) return resp.message
        if (resp.error) return resp.error
        if (resp.errors) return JSON.stringify(resp.errors)
    }
    if (err.message) return err.message
    return String(err)
}
const form = ref({
    userId: '00000000-0000-0000-0000-000000000000', // Temporary user ID
    accountId: '',
    name: '',
    description: '',
    amount: 0,
    transactionType: 1, // Default to Expense
    category: '',
    frequency: 3, // Default to Monthly
    customIntervalDays: null as number | null,
    startDate: '',
    endDate: '',
    cronExpression: '',
    isActive: true,
    autoGenerate: true,
    daysInAdvance: 30,
    notes: ''
})

// Computed
const isEditMode = computed(() => !!props.template)

// Methods
const handleFrequencyChange = () => {
    if (form.value.frequency !== 7) { // Not Custom
        form.value.customIntervalDays = null
    }
}

const resetForm = () => {
    if (props.template) {
        // Edit mode - populate form with template data
            form.value = {
            userId: props.template.userId,
            accountId: props.template.accountId,
            name: props.template.name,
            description: props.template.description || '',
            amount: props.template.amount ?? 0,
            transactionType: props.template.transactionType ?? 1,
            category: props.template.category || '',
            frequency: props.template.frequency ?? 3,
            customIntervalDays: props.template.customIntervalDays ?? null,
            startDate: (props.template.startDate ?? new Date().toISOString()).split('T')[0], // Convert to date format
            endDate: props.template.endDate ? props.template.endDate.split('T')[0] : '',
            cronExpression: props.template.cronExpression || '',
            isActive: props.template.isActive ?? true,
            autoGenerate: props.template.autoGenerate ?? true,
            daysInAdvance: props.template.daysInAdvance ?? 30,
            notes: props.template.notes || ''
        }
    } else {
        // Create mode - set default values
        const today = new Date().toISOString().split('T')[0]
        form.value = {
            userId: '00000000-0000-0000-0000-000000000000',
            accountId: '',
            name: '',
            description: '',
            amount: 0,
            transactionType: 1,
            category: '',
            frequency: 3,
            customIntervalDays: null,
            startDate: today,
            endDate: '',
            cronExpression: '',
            isActive: true,
            autoGenerate: true,
            daysInAdvance: 30,
            notes: ''
        }
    }
}

const handleSubmit = async () => {
    submitError.value = null
    try {
        loading.value = true
        // Basic client-side validation to avoid server-side null constraint errors
        if (!form.value.name || form.value.name.trim() === '') {
            submitError.value = 'Tên mẫu là bắt buộc.'
            loading.value = false
            return
        }
        if (!form.value.accountId || form.value.accountId.trim() === '') {
            submitError.value = 'Vui lòng chọn tài khoản.'
            loading.value = false
            return
        }

        const requestData = {
            ...form.value,
            endDate: form.value.endDate || null,
            description: form.value.description || null,
            category: form.value.category || null,
            cronExpression: form.value.cronExpression || null,
            notes: form.value.notes || null
        }

        // Debug: log request to help troubleshooting in dev
        // (No sensitive values are logged)
        // eslint-disable-next-line no-console
        console.debug('RecurringTemplate submit', { isEdit: isEditMode.value, requestData })

        if (isEditMode.value && props.template) {
            // Update existing template
            const updateRequest: RecurringTransactionTemplateUpdateRequest = {
                ...requestData,
                id: props.template.id
            }
            await updateTemplate(props.template.id, updateRequest)
        } else {
            // Create new template
            const createRequest: RecurringTransactionTemplateCreateRequest = requestData
            await createTemplate(createRequest)
        }

        emit('saved')
    } catch (error: any) {
        // eslint-disable-next-line no-console
        console.error('Error saving template:', error)
        submitError.value = extractErrorMessage(error) || 'Lỗi khi lưu mẫu. Vui lòng thử lại.'
    } finally {
        loading.value = false
    }
}

// Watchers
watch(() => props.template, () => {
    resetForm()
}, { immediate: true })

// Lifecycle
onMounted(async () => {
    resetForm()
    // If parent didn't pass accounts, fetch them here as a fallback
    if ((!props.accounts || props.accounts.length === 0) && fetchAccounts) {
        await fetchAccounts()
    }
})
</script>

<style scoped>
.form-label {
    @apply block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1;
}

.form-input {
    @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-primary focus:border-primary dark:bg-gray-700 dark:text-white;
}

.form-select {
    @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-primary focus:border-primary dark:bg-gray-700 dark:text-white;
}

.form-textarea {
    @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-primary focus:border-primary dark:bg-gray-700 dark:text-white resize-none;
}

.form-checkbox {
    @apply h-4 w-4 text-primary focus:ring-primary border-gray-300 dark:border-gray-600 rounded;
}
</style> 