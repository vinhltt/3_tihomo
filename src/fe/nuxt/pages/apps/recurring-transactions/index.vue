<template>
    <div>
        <div class="panel">
            <div class="mb-5 flex items-center justify-between">
                <div class="flex items-center gap-4">
                    <h5 class="text-lg font-semibold dark:text-white-light">Quản lý Giao dịch Định kỳ</h5>
                    <!-- View Toggle -->
                    <div class="flex bg-gray-100 dark:bg-gray-700 rounded-lg p-1">
                        <button
                            type="button"
                            :class="[
                                'px-3 py-1 text-sm font-medium rounded-md transition-colors',
                                currentView === 'list' 
                                    ? 'bg-white dark:bg-gray-600 text-gray-900 dark:text-white shadow-sm' 
                                    : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
                            ]"
                            @click="currentView = 'list'"
                        >
                            <icon-list class="w-4 h-4 mr-1" />
                            Danh sách
                        </button>
                        <button
                            type="button"
                            :class="[
                                'px-3 py-1 text-sm font-medium rounded-md transition-colors',
                                currentView === 'calendar' 
                                    ? 'bg-white dark:bg-gray-600 text-gray-900 dark:text-white shadow-sm' 
                                    : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
                            ]"
                            @click="currentView = 'calendar'"
                        >
                            <icon-calendar class="w-4 h-4 mr-1" />
                            Lịch
                        </button>
                    </div>
                </div>
                <button type="button" class="btn btn-primary" @click="openCreateModal">
                    <icon-plus class="w-5 h-5 ltr:mr-2 rtl:ml-2" />
                    Thêm mẫu mới
                </button>
            </div>

            <!-- List View -->
            <div v-show="currentView === 'list'">
            <!-- Filters -->
            <div class="mb-5 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <div>
                    <label class="form-label">Tài khoản</label>
                    <select v-model="selectedAccountId" class="form-select" @change="handleAccountChange">
                        <option value="">Tất cả tài khoản</option>
                        <option v-for="account in accounts" :key="account.id" :value="account.id">
                            {{ account.name }}
                        </option>
                    </select>
                </div>
                <div>
                    <label class="form-label">Trạng thái</label>
                    <select v-model="selectedStatus" class="form-select" @change="handleStatusChange">
                        <option value="">Tất cả</option>
                        <option value="active">Đang hoạt động</option>
                        <option value="inactive">Không hoạt động</option>
                    </select>
                </div>
                <div>
                    <label class="form-label">Tần suất</label>
                    <select v-model="selectedFrequency" class="form-select" @change="handleFrequencyChange">
                        <option value="">Tất cả tần suất</option>
                        <option value="0">Hàng ngày</option>
                        <option value="1">Hàng tuần</option>
                        <option value="2">Hai tuần một lần</option>
                        <option value="3">Hàng tháng</option>
                        <option value="4">Hàng quý</option>
                        <option value="5">Nửa năm</option>
                        <option value="6">Hàng năm</option>
                        <option value="7">Tùy chỉnh</option>
                    </select>
                </div>
                <div class="flex items-end">
                    <button type="button" class="btn btn-secondary w-full" @click="resetFilters">
                        <icon-refresh class="w-4 h-4 ltr:mr-2 rtl:ml-2" />
                        Đặt lại
                    </button>
                </div>
            </div>

            <!-- Templates Table -->
            <div class="table-responsive">
                <table class="table-hover">
                    <thead>
                        <tr>
                            <th>Tên mẫu</th>
                            <th>Tài khoản</th>
                            <th>Số tiền</th>
                            <th>Tần suất</th>
                            <th>Ngày tiếp theo</th>
                            <th>Trạng thái</th>
                            <th class="text-center">Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="isLoading">
                            <td colspan="7" class="text-center py-4">
                                <div class="inline-block animate-spin border-4 border-transparent border-l-primary rounded-full w-10 h-10"></div>
                            </td>
                        </tr>
                        <tr v-else-if="templates.length === 0">
                            <td colspan="7" class="text-center py-4 text-gray-500">
                                Không có mẫu giao dịch định kỳ nào
                            </td>
                        </tr>
                        <tr v-else v-for="template in templates" :key="template.id">
                            <td>
                                <div>
                                    <div class="font-semibold">{{ template.name }}</div>
                                    <div class="text-xs text-gray-500">{{ template.description }}</div>
                                </div>
                            </td>
                            <td>{{ getAccountName(template.accountId) }}</td>
                            <td>
                                <span :class="template.transactionType === 0 ? 'text-success' : 'text-danger'">
                                    {{ template.transactionType === 0 ? '+' : '-' }}{{ formatCurrency(template.amount) }}
                                </span>
                            </td>
                            <td>{{ getFrequencyText(template.frequency) }}</td>
                            <td>{{ formatDate(template.nextExecutionDate) }}</td>
                            <td>
                                <span :class="template.isActive ? 'badge badge-outline-success' : 'badge badge-outline-secondary'">
                                    {{ template.isActive ? 'Hoạt động' : 'Không hoạt động' }}
                                </span>
                            </td>
                            <td class="text-center">
                                <div class="flex items-center justify-center gap-2">
                                    <button 
                                        type="button" 
                                        class="btn btn-sm btn-outline-primary"
                                        @click="viewTemplate(template)"
                                        title="Xem chi tiết"
                                    >
                                        <icon-eye class="w-4 h-4" />
                                    </button>
                                    <button 
                                        type="button" 
                                        class="btn btn-sm btn-outline-info"
                                        @click="editTemplate(template)"
                                        title="Chỉnh sửa"
                                    >
                                        <icon-edit class="w-4 h-4" />
                                    </button>
                                    <button 
                                        type="button" 
                                        :class="template.isActive ? 'btn btn-sm btn-outline-warning' : 'btn btn-sm btn-outline-success'"
                                        @click="toggleTemplateStatus(template)"
                                        :title="template.isActive ? 'Tạm dừng' : 'Kích hoạt'"
                                    >
                                        <icon-pause v-if="template.isActive" class="w-4 h-4" />
                                        <icon-play v-else class="w-4 h-4" />
                                    </button>
                                    <button 
                                        type="button" 
                                        class="btn btn-sm btn-outline-danger"
                                        @click="deleteTemplate(template)"
                                        title="Xóa"
                                    >
                                        <icon-trash class="w-4 h-4" />
                                    </button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Pagination -->
            <div v-if="pagination.totalRow > 0" class="mt-5 flex items-center justify-between">
                <div class="text-sm text-gray-500">
                    Hiển thị {{ (pagination.pageIndex - 1) * pagination.pageSize + 1 }} - 
                    {{ Math.min(pagination.pageIndex * pagination.pageSize, pagination.totalRow) }} 
                    trong tổng số {{ pagination.totalRow }} mẫu
                </div>
                <div class="flex items-center gap-2">
                    <button 
                        type="button" 
                        class="btn btn-sm btn-outline-primary"
                        :disabled="pagination.pageIndex <= 1"
                        @click="changePage(pagination.pageIndex - 1)"
                    >
                        Trước
                    </button>
                    <span class="px-3 py-1 text-sm">
                        Trang {{ pagination.pageIndex }} / {{ pagination.pageCount }}
                    </span>
                    <button 
                        type="button" 
                        class="btn btn-sm btn-outline-primary"
                        :disabled="pagination.pageIndex >= pagination.pageCount"
                        @click="changePage(pagination.pageIndex + 1)"
                    >
                        Sau
                    </button>
                </div>
            </div>
            </div>

            <!-- Calendar View -->
            <div v-show="currentView === 'calendar'">
                <RecurringTransactionCalendar />
            </div>
        </div>

        <!-- Create/Edit Modal -->
        <RecurringTransactionModal
            v-if="showModal"
            :template="selectedTemplate"
            :accounts="accounts"
            @close="closeModal"
            @saved="handleTemplateSaved"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRecurringTransactions } from '~/composables/useRecurringTransactions'
import { useAccountsSimple } from '~/composables/useAccountsSimple'
import type { RecurringTransactionTemplateViewModel } from '~/types'
import type { FilterBodyRequest } from '~/types/api'
import { FilterLogicalOperator, FilterType, SortDirection } from '~/types/api'
import RecurringTransactionModal from '~/components/apps/recurring-transactions/RecurringTransactionModal.vue'
import RecurringTransactionCalendar from '~/components/apps/recurring-transactions/RecurringTransactionCalendar.vue'

// Icons
import IconPlus from '~/components/icon/icon-plus.vue'
import IconList from '~/components/icon/icon-list.vue'
import IconCalendar from '~/components/icon/icon-calendar.vue'
import IconRefresh from '~/components/icon/icon-refresh.vue'
import IconEye from '~/components/icon/icon-eye.vue'
import IconEdit from '~/components/icon/icon-edit.vue'
import IconPause from '~/components/icon/icon-pause.vue'
import IconPlay from '~/components/icon/icon-play.vue'
import IconTrash from '~/components/icon/icon-trash.vue'

// Composables (stateless API methods)
const { getTemplates: apiGetTemplates, toggleActiveStatus, deleteTemplate: deleteTemplateApi } = useRecurringTransactions()
const { accounts, getAccounts, getAccountName } = useAccountsSimple()

// Local reactive state (previously provided by the composable)
const templates = ref<RecurringTransactionTemplateViewModel[]>([])
const isLoading = ref(false)
const pagination = ref({
    pageIndex: 1,
    pageSize: 10,
    totalRow: 0,
    pageCount: 0
})

// Reactive data
const currentView = ref<'list' | 'calendar'>('list')
const selectedAccountId = ref('')
const selectedStatus = ref('')
const selectedFrequency = ref('')
const showModal = ref(false)
const selectedTemplate = ref<RecurringTransactionTemplateViewModel | null>(null)

// Build a FilterBodyRequest compatible with the backend filter endpoint
const buildFilterRequest = (filters: any = {}): FilterBodyRequest => {
    const details: any[] = []
    if (filters.accountId) {
        details.push({ attributeName: 'AccountId', value: filters.accountId, filterType: FilterType.Equal })
    }
    if (filters.isActive !== undefined) {
        details.push({ attributeName: 'IsActive', value: String(filters.isActive), filterType: FilterType.Equal })
    }
    if (filters.frequency !== undefined) {
        details.push({ attributeName: 'Frequency', value: String(filters.frequency), filterType: FilterType.Equal })
    }

    return {
        langId: '',
        searchValue: '',
        filter: {
            logicalOperator: FilterLogicalOperator.And,
            details
        },
        orders: [{ field: 'CreatedAt', direction: SortDirection.Descending }],
        pagination: {
            pageIndex: filters.pageIndex || pagination.value.pageIndex,
            pageSize: filters.pageSize || pagination.value.pageSize,
            totalRow: 0,
            pageCount: 0
        }
    }
}

// Wrapper that calls the stateless composable and updates local state
const fetchTemplates = async (filters: any = {}) => {
    isLoading.value = true
    try {
        const req = buildFilterRequest(filters)
        const resp = await apiGetTemplates(req)
        // resp expected to be IBasePaging<T> with .data and pagination fields
        templates.value = resp?.data || []
        pagination.value.pageIndex = resp?.pageIndex ?? pagination.value.pageIndex
        pagination.value.pageSize = resp?.pageSize ?? pagination.value.pageSize
        pagination.value.totalRow = resp?.totalRow ?? pagination.value.totalRow
        pagination.value.pageCount = resp?.pageCount ?? pagination.value.pageCount
    } catch (err) {
        // on error, reset list
        // eslint-disable-next-line no-console
        console.error('Error loading recurring templates', err)
        templates.value = []
    } finally {
        isLoading.value = false
    }
}

// Methods
const handleAccountChange = () => {
    fetchTemplates({ accountId: selectedAccountId.value })
}

const handleStatusChange = () => {
    const isActive = selectedStatus.value === 'active' ? true : 
                    selectedStatus.value === 'inactive' ? false : undefined
    fetchTemplates({ isActive })
}

const handleFrequencyChange = () => {
    const frequency = selectedFrequency.value ? parseInt(selectedFrequency.value) : undefined
    fetchTemplates({ frequency })
}

const resetFilters = () => {
    selectedAccountId.value = ''
    selectedStatus.value = ''
    selectedFrequency.value = ''
    fetchTemplates()
}

const changePage = (page: number) => {
    fetchTemplates({ pageIndex: page })
}

const openCreateModal = () => {
    selectedTemplate.value = null
    showModal.value = true
}

const viewTemplate = (template: RecurringTransactionTemplateViewModel) => {
    selectedTemplate.value = template
    showModal.value = true
}

const editTemplate = (template: RecurringTransactionTemplateViewModel) => {
    selectedTemplate.value = template
    showModal.value = true
}

const toggleTemplateStatus = async (template: RecurringTransactionTemplateViewModel) => {
    try {
        await toggleActiveStatus(template.id, !template.isActive)
        await fetchTemplates()
    } catch (err) {
        // eslint-disable-next-line no-console
        console.error('Error toggling template status', err)
    }
}

const deleteTemplate = async (template: RecurringTransactionTemplateViewModel) => {
    if (confirm(`Bạn có chắc chắn muốn xóa mẫu "${template.name}"?`)) {
        try {
            await deleteTemplateApi(template.id)
            await fetchTemplates()
        } catch (err) {
            // eslint-disable-next-line no-console
            console.error('Error deleting template', err)
        }
    }
}

const closeModal = () => {
    showModal.value = false
    selectedTemplate.value = null
}

const handleTemplateSaved = async () => {
    closeModal()
    await fetchTemplates()
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

const getFrequencyText = (frequency: number) => {
    const frequencies = [
        'Hàng ngày',
        'Hàng tuần', 
        'Hai tuần một lần',
        'Hàng tháng',
        'Hàng quý',
        'Nửa năm',
        'Hàng năm',
        'Tùy chỉnh'
    ]
    return frequencies[frequency] || 'Không xác định'
}

// Lifecycle
onMounted(async () => {
    // Load templates and accounts in parallel
    await Promise.all([
        fetchTemplates(),
        getAccounts()
    ])
})
</script>

<style scoped>
.table-responsive {
    @apply overflow-x-auto;
}

.table-hover tbody tr:hover {
    @apply bg-gray-50 dark:bg-gray-800;
}
</style> 