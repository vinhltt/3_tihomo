<template>
    <div>
        <div class="panel">
            <div class="mb-5 flex items-center justify-between">
                <h5 class="text-lg font-semibold dark:text-white-light">Quản lý Giao dịch Định kỳ</h5>
                <button type="button" class="btn btn-primary" @click="openCreateModal">
                    <icon-plus class="w-5 h-5 ltr:mr-2 rtl:ml-2" />
                    Thêm mẫu mới
                </button>
            </div>

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
                        <tr v-if="loading">
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
import RecurringTransactionModal from '~/components/apps/recurring-transactions/RecurringTransactionModal.vue'

// Icons
import IconPlus from '~/components/icon/icon-plus.vue'
import IconRefresh from '~/components/icon/icon-refresh.vue'
import IconEye from '~/components/icon/icon-eye.vue'
import IconEdit from '~/components/icon/icon-edit.vue'
import IconPause from '~/components/icon/icon-pause.vue'
import IconPlay from '~/components/icon/icon-play.vue'
import IconTrash from '~/components/icon/icon-trash.vue'

// Composables
const { 
    templates, 
    loading, 
    pagination, 
    getTemplates, 
    toggleActiveStatus, 
    deleteTemplate: deleteTemplateApi 
} = useRecurringTransactions()
const { accounts, getAccountName } = useAccountsSimple()

// Reactive data
const selectedAccountId = ref('')
const selectedStatus = ref('')
const selectedFrequency = ref('')
const showModal = ref(false)
const selectedTemplate = ref<RecurringTransactionTemplateViewModel | null>(null)

// Methods
const handleAccountChange = () => {
    getTemplates({ accountId: selectedAccountId.value })
}

const handleStatusChange = () => {
    const isActive = selectedStatus.value === 'active' ? true : 
                    selectedStatus.value === 'inactive' ? false : undefined
    getTemplates({ isActive })
}

const handleFrequencyChange = () => {
    const frequency = selectedFrequency.value ? parseInt(selectedFrequency.value) : undefined
    getTemplates({ frequency })
}

const resetFilters = () => {
    selectedAccountId.value = ''
    selectedStatus.value = ''
    selectedFrequency.value = ''
    getTemplates()
}

const changePage = (page: number) => {
    getTemplates({ pageIndex: page })
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
    const success = await toggleActiveStatus(template.id, !template.isActive)
    if (success) {
        await getTemplates()
    }
}

const deleteTemplate = async (template: RecurringTransactionTemplateViewModel) => {
    if (confirm(`Bạn có chắc chắn muốn xóa mẫu "${template.name}"?`)) {
        const success = await deleteTemplateApi(template.id)
        if (success) {
            await getTemplates()
        }
    }
}

const closeModal = () => {
    showModal.value = false
    selectedTemplate.value = null
}

const handleTemplateSaved = () => {
    closeModal()
    getTemplates()
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
    await getTemplates()
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