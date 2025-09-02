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
                <option :value="CategoryType.Investment">Đầu tư</option>
                <option :value="CategoryType.Other">Khác</option>
              </select>
            </div>

            <!-- Investment Suggestion (new) -->
            <div v-if="form.categoryType === CategoryType.Investment && mode === 'create' && !hideInvestmentSuggestion" 
                 class="bg-blue-50 dark:bg-blue-900/20 p-4 rounded-lg border border-blue-200 dark:border-blue-700">
              <div class="flex items-start space-x-3">
                <div class="text-blue-500 mt-1">
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                  </svg>
                </div>
                <div class="flex-1">
                  <p class="text-sm font-medium text-blue-800 dark:text-blue-200">
                    💡 Thêm vào danh mục đầu tư
                  </p>
                  <p class="text-sm text-blue-600 dark:text-blue-300 mt-1">
                    Bạn có muốn thêm giao dịch này vào portfolio đầu tư để theo dõi profit/loss không?
                  </p>
                  <div class="flex space-x-3 mt-3">
                    <button type="button" @click="showInvestmentForm = true" 
                            class="text-sm bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700">
                      Có, thêm vào portfolio
                    </button>
                    <button type="button" @click="hideInvestmentSuggestion = true"
                            class="text-sm text-blue-600 hover:text-blue-800">
                      Bỏ qua
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <!-- Investment Form Modal -->
            <div v-if="showInvestmentForm" 
                 class="bg-green-50 dark:bg-green-900/20 p-4 rounded-lg border border-green-200 dark:border-green-700 space-y-4">
              <div class="flex items-center justify-between">
                <h6 class="text-md font-semibold text-green-800 dark:text-green-200">
                  📈 Thông tin đầu tư
                </h6>
                <button type="button" @click="showInvestmentForm = false" 
                        class="text-green-600 hover:text-green-800">
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"/>
                  </svg>
                </button>
              </div>
              
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="text-sm font-medium text-green-800 dark:text-green-200">
                    Mã cổ phiếu/Symbol *
                  </label>
                  <input 
                    v-model="investmentForm.symbol" 
                    type="text" 
                    class="form-input text-sm"
                    placeholder="VIC, FPT, BID..."
                    required
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-green-800 dark:text-green-200">
                    Tên đầu tư
                  </label>
                  <input 
                    v-model="investmentForm.name" 
                    type="text" 
                    class="form-input text-sm"
                    placeholder="Vingroup, FPT Corp..."
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-green-800 dark:text-green-200">
                    Số lượng *
                  </label>
                  <input 
                    v-model.number="investmentForm.quantity" 
                    type="number" 
                    min="1"
                    step="1"
                    class="form-input text-sm"
                    placeholder="100"
                    required
                  />
                </div>
                <div>
                  <label class="text-sm font-medium text-green-800 dark:text-green-200">
                    Giá mua/CP (VND)
                  </label>
                  <input 
                    v-model.number="investmentForm.pricePerUnit" 
                    type="number" 
                    step="0.01"
                    class="form-input text-sm"
                    placeholder="50000"
                  />
                </div>
              </div>
              
              <div>
                <label class="text-sm font-medium text-green-800 dark:text-green-200">
                  Ghi chú đầu tư
                </label>
                <textarea 
                  v-model="investmentForm.description" 
                  rows="2"
                  class="form-textarea text-sm"
                  placeholder="Mua cổ phiếu theo khuyến nghị..."
                ></textarea>
              </div>
              
              <div class="flex space-x-3">
                <button type="button" @click="handleCreateInvestment" 
                        :disabled="isCreatingInvestment"
                        class="text-sm bg-green-600 text-white px-3 py-2 rounded hover:bg-green-700 disabled:opacity-50">
                  <div v-if="isCreatingInvestment" class="animate-spin rounded-full h-4 w-4 border-b-2 border-white inline-block mr-2"></div>
                  {{ isCreatingInvestment ? 'Đang tạo...' : 'Tạo Investment' }}
                </button>
                <button type="button" @click="showInvestmentForm = false"
                        class="text-sm text-green-600 hover:text-green-800 px-3 py-2">
                  Hủy
                </button>
              </div>
              
              <div v-if="investmentError" class="text-sm text-red-600">
                {{ investmentError }}
              </div>
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
import type { TransactionViewModel, TransactionCreateRequest, TransactionUpdateRequest, TransactionDirectionType } from '~/types/transaction'
import type { AccountViewModel } from '~/types/account'
import { TransactionDirection, CategoryType } from '~/types/transaction'

// Props
interface Props {
  visible: boolean
  transaction?: TransactionViewModel | null
  accounts: AccountViewModel[]
  mode: 'create' | 'edit' | 'view'
  defaultDirection?: TransactionDirectionType
  defaultAccountId?: string
}

const props = withDefaults(defineProps<Props>(), {
  transaction: null,
  defaultDirection: TransactionDirection.Spent as TransactionDirectionType,
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
const { 
  createInvestmentFromTransaction, 
  parseTransactionForInvestment, 
  isEligibleForInvestment,
  isCreating: isCreatingInvestment,
  error: investmentError 
} = useInvestmentSuggestion()

// State
const isLoading = ref(false)
const error = ref<string | null>(null)
const showAdvanced = ref(false)

// Investment suggestion state
const showInvestmentForm = ref(false)
const hideInvestmentSuggestion = ref(false)

// Investment form state
const investmentForm = ref({
  symbol: '',
  name: '',
  quantity: 1,
  pricePerUnit: 0,
  description: ''
})

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

// Watch for form changes to auto-suggest investment details
watch(() => [form.value.description, form.value.amount], () => {
  if (form.value.categoryType === CategoryType.Investment && showInvestmentForm.value) {
    const transactionData = {
      description: form.value.description,
      spentAmount: form.value.transactionDirection === TransactionDirection.Spent ? form.value.amount : 0,
      revenueAmount: form.value.transactionDirection === TransactionDirection.Revenue ? form.value.amount : 0,
      categoryType: form.value.categoryType
    } as TransactionViewModel

    const suggestion = parseTransactionForInvestment(transactionData)
    if (suggestion.suggested) {
      investmentForm.value.symbol = suggestion.symbol
      investmentForm.value.name = suggestion.name
      investmentForm.value.quantity = suggestion.quantity
      investmentForm.value.pricePerUnit = suggestion.pricePerUnit
    } else if (!investmentForm.value.pricePerUnit && form.value.amount > 0) {
      // Auto-calculate price per unit if not suggested
      investmentForm.value.pricePerUnit = form.value.amount / investmentForm.value.quantity
    }
  }
}, { deep: true })

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

const handleCreateInvestment = async () => {
  if (!investmentForm.value.symbol || !investmentForm.value.quantity) {
    error.value = 'Vui lòng nhập đầy đủ thông tin Symbol và Số lượng'
    return
  }

  try {
    // Calculate amount from form
    const amount = form.value.amount || 0
    
    // Create a mock transaction object for the investment creation
    const transactionData = {
      id: 'temp-id', // Temporary ID since transaction hasn't been created yet
      accountId: form.value.accountId,
      transactionDate: form.value.transactionDate,
      revenueAmount: form.value.transactionDirection === TransactionDirection.Revenue ? amount : 0,
      spentAmount: form.value.transactionDirection === TransactionDirection.Spent ? amount : 0,
      description: form.value.description,
      balance: form.value.balance,
      categoryType: form.value.categoryType,
      syncMisa: form.value.syncMisa,
      syncSms: form.value.syncSms,
      vn: form.value.vn
    } as TransactionViewModel

    await createInvestmentFromTransaction(transactionData, {
      symbol: investmentForm.value.symbol,
      name: investmentForm.value.name || investmentForm.value.symbol,
      quantity: investmentForm.value.quantity,
      pricePerUnit: investmentForm.value.pricePerUnit || (amount / investmentForm.value.quantity),
      description: investmentForm.value.description
    })

    // Reset investment form and hide it
    investmentForm.value = {
      symbol: '',
      name: '',
      quantity: 1,
      pricePerUnit: 0,
      description: ''
    }
    showInvestmentForm.value = false
    hideInvestmentSuggestion.value = true

    // Show success message
    // You can add a toast/notification here if available
    
  } catch (err) {
    console.error('Error creating investment:', err)
    // The error will be handled by the composable
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