<template>
  <!-- Mobile Modal Overlay -->
  <div 
    v-if="visible" 
    class="fixed inset-0 z-[9999] lg:hidden"
    @click.self="$emit('update:visible', false)"
  >
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-black/50"></div>
    
    <!-- Modal Content -->
    <div class="relative flex h-full flex-col bg-white dark:bg-[#0e1726]">
      <!-- Header -->
      <div class="flex items-center justify-between border-b p-4 dark:border-[#1b2e4b]">
        <h5 class="text-lg font-semibold dark:text-white-light">
          <span v-if="mode === 'create'">Thêm giao dịch</span>
          <span v-else-if="mode === 'edit'">Chỉnh sửa giao dịch</span>
          <span v-else>Chi tiết giao dịch</span>
        </h5>
        <button 
          @click="$emit('update:visible', false)"
          class="btn btn-sm btn-outline-danger"
        >
          <icon-x class="w-4 h-4" />
        </button>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-4">
        <!-- Loading State -->
        <div v-if="isLoading" class="text-center py-8">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
          <p class="mt-2 text-gray-500">Đang xử lý...</p>
        </div>

        <!-- Form -->
        <form v-else @submit.prevent="handleSubmit" class="space-y-5">
          <!-- Simple Mode Fields -->
          <div class="space-y-4">
            <!-- Transaction Direction -->
            <div>
              <label class="text-sm font-medium text-white-dark">
                Loại giao dịch *
              </label>
              <select 
                v-model="form.transactionDirection" 
                :disabled="mode === 'view'"
                class="form-select"
                required
              >
                <option :value="TransactionDirection.Revenue">Thu</option>
                <option :value="TransactionDirection.Spent">Chi</option>
              </select>
            </div>

            <!-- Amount -->
            <div>
              <label class="text-sm font-medium text-white-dark">
                Số tiền *
              </label>
              <input 
                v-model.number="form.amount" 
                :disabled="mode === 'view'"
                type="number" 
                step="0.01"
                class="form-input"
                placeholder="Nhập số tiền"
                required
              />
            </div>

            <!-- Transaction Date -->
            <div>
              <label class="text-sm font-medium text-white-dark">
                Ngày giao dịch *
              </label>
              <input 
                v-model="form.transactionDate" 
                :disabled="mode === 'view'"
                type="date" 
                class="form-input"
                required
              />
            </div>

            <!-- Account -->
            <div>
              <label class="text-sm font-medium text-white-dark">
                Tài khoản *
              </label>
              <select 
                v-model="form.accountId" 
                :disabled="mode === 'view'"
                class="form-select"
                required
              >
                <option value="">Chọn tài khoản</option>
                <option 
                  v-for="account in accounts" 
                  :key="account.id" 
                  :value="account.id"
                >
                  {{ account.name }}
                </option>
              </select>
            </div>

            <!-- Category Type -->
            <div>
              <label class="text-sm font-medium text-white-dark">
                Danh mục *
              </label>
              <select 
                v-model="form.categoryType" 
                :disabled="mode === 'view'"
                class="form-select"
                required
              >
                <option :value="CategoryType.Income">Thu nhập</option>
                <option :value="CategoryType.Expense">Chi tiêu</option>
                <option :value="CategoryType.Transfer">Chuyển khoản</option>
                <option :value="CategoryType.Fee">Phí</option>
                <option :value="CategoryType.Other">Khác</option>
              </select>
            </div>

            <!-- Description -->
            <div>
              <label class="text-sm font-medium text-white-dark">
                Mô tả
              </label>
              <input 
                v-model="form.description" 
                :disabled="mode === 'view'"
                type="text" 
                class="form-input"
                placeholder="Nhập mô tả giao dịch"
              />
            </div>

            <!-- Balance -->
            <div>
              <label class="text-sm font-medium text-white-dark">
                Số dư sau giao dịch *
              </label>
              <input 
                v-model.number="form.balance" 
                :disabled="mode === 'view'"
                type="number" 
                step="0.01"
                class="form-input"
                placeholder="Nhập số dư"
                required
              />
            </div>
          </div>

          <!-- Toggle Advanced Mode -->
          <div class="border-t pt-5 dark:border-[#1b2e4b]">
            <button 
              type="button"
              @click="showAdvanced = !showAdvanced"
              class="btn btn-outline-info btn-sm"
              :disabled="mode === 'view'"
            >
              <icon-chevron-down 
                :class="{ 'rotate-180': showAdvanced }"
                class="w-4 h-4 transition-transform"
              />
              {{ showAdvanced ? 'Ẩn' : 'Hiện' }} thông tin nâng cao
            </button>
          </div>

          <!-- Advanced Mode Fields -->
          <div v-if="showAdvanced" class="space-y-4 border-t pt-5 dark:border-[#1b2e4b]">
            <!-- Financial Info Group -->
            <div class="space-y-4">
              <h6 class="text-md font-semibold">Thông tin tài chính mở rộng</h6>
              
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    So sánh số dư
                  </label>
                  <input 
                    v-model.number="form.balanceCompare" 
                    :disabled="mode === 'view'"
                    type="number" 
                    step="0.01"
                    class="form-input"
                    placeholder="Số dư trước đó"
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    Hạn mức khả dụng
                  </label>
                  <input 
                    v-model.number="form.availableLimit" 
                    :disabled="mode === 'view'"
                    type="number" 
                    step="0.01"
                    class="form-input"
                    placeholder="Hạn mức"
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    So sánh hạn mức
                  </label>
                  <input 
                    v-model.number="form.availableLimitCompare" 
                    :disabled="mode === 'view'"
                    type="number" 
                    step="0.01"
                    class="form-input"
                    placeholder="Hạn mức trước đó"
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    Tăng hạn mức
                  </label>
                  <input 
                    v-model.number="form.increaseCreditLimit" 
                    :disabled="mode === 'view'"
                    type="number" 
                    step="0.01"
                    class="form-input"
                    placeholder="Số tiền tăng"
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    % Đã sử dụng
                  </label>
                  <input 
                    v-model.number="form.usedPercent" 
                    :disabled="mode === 'view'"
                    type="number" 
                    step="0.01"
                    max="100"
                    class="form-input"
                    placeholder="0-100%"
                  />
                </div>
              </div>
            </div>

            <!-- Category & Notes Group -->
            <div class="space-y-4">
              <h6 class="text-md font-semibold">Phân loại & ghi chú</h6>
              
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    Tóm tắt danh mục
                  </label>
                  <input 
                    v-model="form.categorySummary" 
                    :disabled="mode === 'view'"
                    type="text" 
                    class="form-input"
                    placeholder="Tóm tắt"
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    Nhóm
                  </label>
                  <input 
                    v-model="form.group" 
                    :disabled="mode === 'view'"
                    type="text" 
                    class="form-input"
                    placeholder="Nhóm giao dịch"
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    Ghi chú
                  </label>
                  <textarea 
                    v-model="form.note" 
                    :disabled="mode === 'view'"
                    rows="3"
                    class="form-textarea"
                    placeholder="Ghi chú chi tiết"
                  ></textarea>
                </div>
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    Import từ
                  </label>
                  <input 
                    v-model="form.importFrom" 
                    :disabled="mode === 'view'"
                    type="text" 
                    class="form-input"
                    placeholder="Nguồn import"
                  />
                </div>
              </div>
            </div>

            <!-- Sync & Metadata Group -->
            <div class="space-y-4">
              <h6 class="text-md font-semibold">Đồng bộ & metadata</h6>
              
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-white-dark">
                    Mã giao dịch
                  </label>
                  <input 
                    v-model="form.transactionCode" 
                    :disabled="mode === 'view'"
                    type="text" 
                    class="form-input"
                    placeholder="Mã giao dịch"
                  />
                </div>
                
                <div class="space-y-3">
                  <div class="flex items-center">
                    <input 
                      v-model="form.syncMisa" 
                      :disabled="mode === 'view'"
                      type="checkbox" 
                      id="mobile-syncMisa"
                      class="form-checkbox"
                    />
                    <label for="mobile-syncMisa" class="ml-2 text-sm font-medium text-white-dark">
                      Đồng bộ Misa
                    </label>
                  </div>
                  
                  <div class="flex items-center">
                    <input 
                      v-model="form.syncSms" 
                      :disabled="mode === 'view'"
                      type="checkbox" 
                      id="mobile-syncSms"
                      class="form-checkbox"
                    />
                    <label for="mobile-syncSms" class="ml-2 text-sm font-medium text-white-dark">
                      Đồng bộ SMS
                    </label>
                  </div>
                  
                  <div class="flex items-center">
                    <input 
                      v-model="form.vn" 
                      :disabled="mode === 'view'"
                      type="checkbox" 
                      id="mobile-vn"
                      class="form-checkbox"
                    />
                    <label for="mobile-vn" class="ml-2 text-sm font-medium text-white-dark">
                      Tiền tệ VN
                    </label>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Error Message -->
          <div v-if="error" class="alert alert-danger">
            {{ error }}
          </div>
        </form>
      </div>

      <!-- Footer Buttons -->
      <div class="border-t p-4 dark:border-[#1b2e4b]">
        <div v-if="mode !== 'view'" class="flex gap-2">
          <button 
            @click="handleSubmit"
            :disabled="isLoading"
            class="btn btn-primary flex-1"
          >
            <icon-save class="w-4 h-4" v-if="!isLoading" />
            <div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white" v-else></div>
            {{ mode === 'create' ? 'Tạo giao dịch' : 'Cập nhật' }}
          </button>
          <button 
            @click="$emit('update:visible', false)"
            class="btn btn-outline-danger"
          >
            Hủy
          </button>
        </div>

        <!-- View Mode Actions -->
        <div v-else class="flex gap-2">
          <button 
            @click="$emit('edit')"
            class="btn btn-warning flex-1"
          >
            <icon-edit class="w-4 h-4" />
            Chỉnh sửa
          </button>
          <button 
            @click="confirmDelete"
            class="btn btn-danger"
          >
            <icon-trash class="w-4 h-4" />
            Xóa
          </button>
          <button 
            @click="$emit('update:visible', false)"
            class="btn btn-outline-secondary"
          >
            Đóng
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TransactionViewModel, TransactionCreateRequest, TransactionUpdateRequest } from '~/types/transaction'
import type { AccountViewModel } from '~/types/account'
import { TransactionDirection, CategoryType } from '~/types/transaction'

// Props
interface Props {
  visible: boolean
  transaction?: TransactionViewModel | null
  accounts: AccountViewModel[]
  mode: 'create' | 'edit' | 'view'
  defaultDirection?: number
  defaultAccountId?: string
}

const props = withDefaults(defineProps<Props>(), {
  transaction: null,
  defaultDirection: TransactionDirection.Spent,
  defaultAccountId: ''
})

// Emits
const emit = defineEmits<{
  'update:visible': [value: boolean]
  'created': []
  'updated': []
  'deleted': []
  'edit': []
}>()

// Composables
const { createTransaction, updateTransaction, deleteTransaction, createFormDefaults, resetForm } = useTransactions()

// State
const isLoading = ref(false)
const error = ref<string | null>(null)
const showAdvanced = ref(false)

// Form state
const form = ref<TransactionCreateRequest>({
  accountId: '',
  transactionDate: new Date().toISOString().split('T')[0],
  transactionDirection: TransactionDirection.Spent,
  amount: 0,
  description: '',
  balance: 0,
  categoryType: CategoryType.Expense,
  syncMisa: false,
  syncSms: false,
  vn: true,
  balanceCompare: undefined,
  availableLimit: undefined,
  availableLimitCompare: undefined,
  transactionCode: undefined,
  categorySummary: undefined,
  note: undefined,
  importFrom: undefined,
  increaseCreditLimit: undefined,
  usedPercent: undefined,
  group: undefined
})

// Watchers
watch(() => props.visible, (newVal) => {
  if (newVal) {
    initializeForm()
  }
})

watch(() => props.mode, () => {
  if (props.visible) {
    initializeForm()
  }
})

watch(() => props.transaction, () => {
  if (props.visible) {
    initializeForm()
  }
})

// Methods
const initializeForm = () => {
  error.value = null
  showAdvanced.value = false

  if (props.mode === 'create') {
    // Create mode - use defaults
    const defaults = createFormDefaults(props.defaultDirection, props.defaultAccountId)
    Object.assign(form.value, defaults)
  } else if (props.transaction) {
    // Edit or View mode - populate from transaction
    form.value = {
      id: props.transaction.id,
      accountId: props.transaction.accountId,
      userId: props.transaction.userId,
      transactionDate: props.transaction.transactionDate.split('T')[0], // Format date for input
      transactionDirection: props.transaction.revenueAmount > 0 ? TransactionDirection.Revenue : TransactionDirection.Spent,
      amount: props.transaction.revenueAmount > 0 ? props.transaction.revenueAmount : props.transaction.spentAmount,
      description: props.transaction.description || '',
      balance: props.transaction.balance,
      balanceCompare: props.transaction.balanceCompare,
      availableLimit: props.transaction.availableLimit,
      availableLimitCompare: props.transaction.availableLimitCompare,
      transactionCode: props.transaction.transactionCode,
      syncMisa: props.transaction.syncMisa,
      syncSms: props.transaction.syncSms,
      vn: props.transaction.vn,
      categorySummary: props.transaction.categorySummary,
      note: props.transaction.note,
      importFrom: props.transaction.importFrom,
      increaseCreditLimit: props.transaction.increaseCreditLimit,
      usedPercent: props.transaction.usedPercent,
      categoryType: props.transaction.categoryType,
      group: props.transaction.group
    } as TransactionCreateRequest
  }
}

const handleSubmit = async () => {
  try {
    isLoading.value = true
    error.value = null

    if (props.mode === 'create') {
      await createTransaction(form.value)
      resetForm(form.value)
      emit('created')
    } else if (props.mode === 'edit') {
      await updateTransaction(form.value as TransactionUpdateRequest)
      emit('updated')
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Có lỗi xảy ra'
  } finally {
    isLoading.value = false
  }
}

const confirmDelete = async () => {
  if (!props.transaction) return
  
  if (confirm(`Bạn có chắc muốn xóa giao dịch "${props.transaction.description || 'Không có mô tả'}"?`)) {
    try {
      isLoading.value = true
      await deleteTransaction(props.transaction.id)
      emit('deleted')
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Có lỗi xảy ra khi xóa'
    } finally {
      isLoading.value = false
    }
  }
}

// Initialize on mount if visible
onMounted(() => {
  if (props.visible) {
    initializeForm()
  }
})
</script> 