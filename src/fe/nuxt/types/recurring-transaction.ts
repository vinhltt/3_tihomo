/**
 * Enum for recurrence frequency. (EN)
 * 
 * Enum cho tần suất lặp lại. (VI)
 */
export enum RecurrenceFrequency {
  Daily = 0,
  Weekly = 1,
  Biweekly = 2,
  Monthly = 3,
  Quarterly = 4,
  SemiAnnually = 5,
  Annually = 6,
  Custom = 7
}

/**
 * Enum for recurring transaction type. (EN)
 * 
 * Enum cho loại giao dịch định kỳ. (VI)
 */
export enum RecurringTransactionType {
  Revenue = 0,
  Expense = 1
}

/**
 * Enum for expected transaction status. (EN)
 * 
 * Enum cho trạng thái giao dịch dự kiến. (VI)
 */
export enum ExpectedTransactionStatus {
  Pending = 0,
  Confirmed = 1,
  Cancelled = 2,
  Completed = 3
}

/**
 * Interface for base paging response. (EN)
 * 
 * Interface cho phản hồi phân trang cơ bản. (VI)
 */
export interface IBasePaging<T> {
  /**
   * The collection of data for the current page. (EN)
   * 
   * Tập hợp dữ liệu cho trang hiện tại. (VI)
   */
  data: T[]
  /**
   * The current page index (1-based). (EN)
   * 
   * Chỉ mục trang hiện tại (bắt đầu từ 1). (VI)
   */
  pageIndex: number
  /**
   * The number of items per page. (EN)
   * 
   * Số lượng mục trên mỗi trang. (VI)
   */
  pageSize: number
  /**
   * The total number of rows in the dataset. (EN)
   * 
   * Tổng số hàng trong tập dữ liệu. (VI)
   */
  totalRow: number
  /**
   * The total number of pages. (EN)
   * 
   * Tổng số trang. (VI)
   */
  pageCount: number
}

/**
 * View model for recurring transaction template. (EN)
 * 
 * View model cho mẫu giao dịch định kỳ. (VI)
 */
export interface RecurringTransactionTemplateViewModel {
  /**
   * The unique identifier for the template. (EN)
   * 
   * Định danh duy nhất cho mẫu. (VI)
   */
  id: string
  /**
   * The ID of the user who owns the template. (EN)
   * 
   * ID của người dùng sở hữu mẫu. (VI)
   */
  userId: string
  /**
   * The ID of the account associated with the template. (EN)
   * 
   * ID của tài khoản liên quan đến mẫu. (VI)
   */
  accountId: string
  /**
   * The name of the template. (EN)
   * 
   * Tên của mẫu. (VI)
   */
  name: string
  /**
   * The description of the template (optional). (EN)
   * 
   * Mô tả của mẫu (tùy chọn). (VI)
   */
  description?: string | null
  /**
   * The amount for the recurring transaction. (EN)
   * 
   * Số tiền cho giao dịch định kỳ. (VI)
   */
  amount: number
  /**
   * The type of transaction (Revenue or Expense). (EN)
   * 
   * Loại giao dịch (Thu nhập hoặc Chi phí). (VI)
   */
  transactionType: RecurringTransactionType
  /**
   * The category of the transaction (optional). (EN)
   * 
   * Danh mục của giao dịch (tùy chọn). (VI)
   */
  category?: string | null
  /**
   * The frequency of recurrence. (EN)
   * 
   * Tần suất lặp lại. (VI)
   */
  frequency: RecurrenceFrequency
  /**
   * Custom interval in days (for Custom frequency). (EN)
   * 
   * Khoảng thời gian tùy chỉnh tính bằng ngày (cho tần suất Tùy chỉnh). (VI)
   */
  customIntervalDays?: number | null
  /**
   * The next execution date. (EN)
   * 
   * Ngày thực hiện tiếp theo. (VI)
   */
  nextExecutionDate: string
  /**
   * The start date of the template. (EN)
   * 
   * Ngày bắt đầu của mẫu. (VI)
   */
  startDate: string
  /**
   * The end date of the template (optional). (EN)
   * 
   * Ngày kết thúc của mẫu (tùy chọn). (VI)
   */
  endDate?: string | null
  /**
   * Cron expression for complex scheduling (optional). (EN)
   * 
   * Biểu thức cron cho lịch trình phức tạp (tùy chọn). (VI)
   */
  cronExpression?: string | null
  /**
   * Whether the template is active. (EN)
   * 
   * Mẫu có đang hoạt động hay không. (VI)
   */
  isActive: boolean
  /**
   * Whether to automatically generate expected transactions. (EN)
   * 
   * Có tự động sinh giao dịch dự kiến hay không. (VI)
   */
  autoGenerate: boolean
  /**
   * Number of days in advance to generate expected transactions. (EN)
   * 
   * Số ngày trước để sinh giao dịch dự kiến. (VI)
   */
  daysInAdvance: number
  /**
   * Additional notes (optional). (EN)
   * 
   * Ghi chú bổ sung (tùy chọn). (VI)
   */
  notes?: string | null
  /**
   * The creation date. (EN)
   * 
   * Ngày tạo. (VI)
   */
  createdAt: string
  /**
   * The last update date. (EN)
   * 
   * Ngày cập nhật cuối cùng. (VI)
   */
  updatedAt: string
}

/**
 * Request model for creating a recurring transaction template. (EN)
 * 
 * Model yêu cầu để tạo mẫu giao dịch định kỳ. (VI)
 */
export interface RecurringTransactionTemplateCreateRequest {
  /**
   * The ID of the user who owns the template. (EN)
   * 
   * ID của người dùng sở hữu mẫu. (VI)
   */
  userId: string
  /**
   * The ID of the account associated with the template. (EN)
   * 
   * ID của tài khoản liên quan đến mẫu. (VI)
   */
  accountId: string
  /**
   * The name of the template. (EN)
   * 
   * Tên của mẫu. (VI)
   */
  name: string
  /**
   * The description of the template (optional). (EN)
   * 
   * Mô tả của mẫu (tùy chọn). (VI)
   */
  description?: string | null
  /**
   * The amount for the recurring transaction. (EN)
   * 
   * Số tiền cho giao dịch định kỳ. (VI)
   */
  amount: number
  /**
   * The type of transaction (Revenue or Expense). (EN)
   * 
   * Loại giao dịch (Thu nhập hoặc Chi phí). (VI)
   */
  transactionType: RecurringTransactionType
  /**
   * The category of the transaction (optional). (EN)
   * 
   * Danh mục của giao dịch (tùy chọn). (VI)
   */
  category?: string | null
  /**
   * The frequency of recurrence. (EN)
   * 
   * Tần suất lặp lại. (VI)
   */
  frequency: RecurrenceFrequency
  /**
   * Custom interval in days (for Custom frequency). (EN)
   * 
   * Khoảng thời gian tùy chỉnh tính bằng ngày (cho tần suất Tùy chỉnh). (VI)
   */
  customIntervalDays?: number | null
  /**
   * The start date of the template. (EN)
   * 
   * Ngày bắt đầu của mẫu. (VI)
   */
  startDate: string
  /**
   * The end date of the template (optional). (EN)
   * 
   * Ngày kết thúc của mẫu (tùy chọn). (VI)
   */
  endDate?: string | null
  /**
   * Cron expression for complex scheduling (optional). (EN)
   * 
   * Biểu thức cron cho lịch trình phức tạp (tùy chọn). (VI)
   */
  cronExpression?: string | null
  /**
   * Whether the template is active. (EN)
   * 
   * Mẫu có đang hoạt động hay không. (VI)
   */
  isActive: boolean
  /**
   * Whether to automatically generate expected transactions. (EN)
   * 
   * Có tự động sinh giao dịch dự kiến hay không. (VI)
   */
  autoGenerate: boolean
  /**
   * Number of days in advance to generate expected transactions. (EN)
   * 
   * Số ngày trước để sinh giao dịch dự kiến. (VI)
   */
  daysInAdvance: number
  /**
   * Additional notes (optional). (EN)
   * 
   * Ghi chú bổ sung (tùy chọn). (VI)
   */
  notes?: string | null
}

/**
 * Request model for updating a recurring transaction template. (EN)
 * 
 * Model yêu cầu để cập nhật mẫu giao dịch định kỳ. (VI)
 */
export interface RecurringTransactionTemplateUpdateRequest extends RecurringTransactionTemplateCreateRequest {
  /**
   * The unique identifier for the template to update. (EN)
   * 
   * Định danh duy nhất cho mẫu cần cập nhật. (VI)
   */
  id: string
}

/**
 * View model for expected transaction. (EN)
 * 
 * View model cho giao dịch dự kiến. (VI)
 */
export interface ExpectedTransactionViewModel {
  /**
   * The unique identifier for the expected transaction. (EN)
   * 
   * Định danh duy nhất cho giao dịch dự kiến. (VI)
   */
  id: string
  /**
   * The ID of the recurring transaction template. (EN)
   * 
   * ID của mẫu giao dịch định kỳ. (VI)
   */
  recurringTransactionTemplateId: string
  /**
   * The ID of the user. (EN)
   * 
   * ID của người dùng. (VI)
   */
  userId: string
  /**
   * The ID of the account. (EN)
   * 
   * ID của tài khoản. (VI)
   */
  accountId: string
  /**
   * The expected date of the transaction. (EN)
   * 
   * Ngày dự kiến của giao dịch. (VI)
   */
  expectedDate: string
  /**
   * The expected amount. (EN)
   * 
   * Số tiền dự kiến. (VI)
   */
  expectedAmount: number
  /**
   * The original amount (before adjustment). (EN)
   * 
   * Số tiền gốc (trước khi điều chỉnh). (VI)
   */
  originalAmount?: number | null
  /**
   * The description of the expected transaction. (EN)
   * 
   * Mô tả của giao dịch dự kiến. (VI)
   */
  description?: string | null
  /**
   * The type of transaction. (EN)
   * 
   * Loại giao dịch. (VI)
   */
  transactionType: RecurringTransactionType
  /**
   * The category of the transaction. (EN)
   * 
   * Danh mục của giao dịch. (VI)
   */
  category?: string | null
  /**
   * The status of the expected transaction. (EN)
   * 
   * Trạng thái của giao dịch dự kiến. (VI)
   */
  status: ExpectedTransactionStatus
  /**
   * The ID of the actual transaction (when confirmed). (EN)
   * 
   * ID của giao dịch thực tế (khi được xác nhận). (VI)
   */
  actualTransactionId?: string | null
  /**
   * The reason for adjustment or cancellation. (EN)
   * 
   * Lý do điều chỉnh hoặc hủy bỏ. (VI)
   */
  adjustmentReason?: string | null
  /**
   * When the expected transaction was generated. (EN)
   * 
   * Khi nào giao dịch dự kiến được sinh ra. (VI)
   */
  generatedAt: string
  /**
   * When the expected transaction was confirmed. (EN)
   * 
   * Khi nào giao dịch dự kiến được xác nhận. (VI)
   */
  confirmedAt?: string | null
  /**
   * The creation date. (EN)
   * 
   * Ngày tạo. (VI)
   */
  createdAt: string
  /**
   * The last update date. (EN)
   * 
   * Ngày cập nhật cuối cùng. (VI)
   */
  updatedAt: string
}

/**
 * Request model for creating an expected transaction. (EN)
 * 
 * Model yêu cầu để tạo giao dịch dự kiến. (VI)
 */
export interface ExpectedTransactionCreateRequest {
  /**
   * The ID of the recurring transaction template. (EN)
   * 
   * ID của mẫu giao dịch định kỳ. (VI)
   */
  recurringTransactionTemplateId: string
  /**
   * The ID of the user. (EN)
   * 
   * ID của người dùng. (VI)
   */
  userId: string
  /**
   * The ID of the account. (EN)
   * 
   * ID của tài khoản. (VI)
   */
  accountId: string
  /**
   * The expected date of the transaction. (EN)
   * 
   * Ngày dự kiến của giao dịch. (VI)
   */
  expectedDate: string
  /**
   * The expected amount. (EN)
   * 
   * Số tiền dự kiến. (VI)
   */
  expectedAmount: number
  /**
   * The description of the expected transaction. (EN)
   * 
   * Mô tả của giao dịch dự kiến. (VI)
   */
  description?: string | null
  /**
   * The type of transaction. (EN)
   * 
   * Loại giao dịch. (VI)
   */
  transactionType: RecurringTransactionType
  /**
   * The category of the transaction. (EN)
   * 
   * Danh mục của giao dịch. (VI)
   */
  category?: string | null
}

/**
 * Request model for updating an expected transaction. (EN)
 * 
 * Model yêu cầu để cập nhật giao dịch dự kiến. (VI)
 */
export interface ExpectedTransactionUpdateRequest extends ExpectedTransactionCreateRequest {
  /**
   * The unique identifier for the expected transaction to update. (EN)
   * 
   * Định danh duy nhất cho giao dịch dự kiến cần cập nhật. (VI)
   */
  id: string
}

/**
 * Request model for confirming an expected transaction. (EN)
 * 
 * Model yêu cầu để xác nhận giao dịch dự kiến. (VI)
 */
export interface ConfirmTransactionRequest {
  /**
   * The ID of the actual transaction to link. (EN)
   * 
   * ID của giao dịch thực tế để liên kết. (VI)
   */
  actualTransactionId: string
}

/**
 * Request model for cancelling an expected transaction. (EN)
 * 
 * Model yêu cầu để hủy giao dịch dự kiến. (VI)
 */
export interface CancelTransactionRequest {
  /**
   * The reason for cancellation. (EN)
   * 
   * Lý do hủy bỏ. (VI)
   */
  reason: string
}

/**
 * Request model for adjusting an expected transaction. (EN)
 * 
 * Model yêu cầu để điều chỉnh giao dịch dự kiến. (VI)
 */
export interface AdjustTransactionRequest {
  /**
   * The new expected amount. (EN)
   * 
   * Số tiền dự kiến mới. (VI)
   */
  newAmount: number
  /**
   * The reason for adjustment. (EN)
   * 
   * Lý do điều chỉnh. (VI)
   */
  reason: string
} 