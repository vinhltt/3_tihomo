/**
 * Represents pagination information for data retrieval. (EN)
 * 
 * Đại diện cho thông tin phân trang để truy xuất dữ liệu. (VI)
 */
export interface Pagination {
  /**
   * Gets or sets the current page index (1-based). (EN)
   * 
   * Lấy hoặc đặt chỉ mục trang hiện tại (bắt đầu từ 1). (VI)
   */
  pageIndex: number
  /**
   * Gets or sets the number of items per page. (EN)
   * 
   * Lấy hoặc đặt số lượng mục trên mỗi trang. (VI)
   */
  pageSize: number
  /**
   * Gets or sets the total number of rows in the dataset. (EN)
   * 
   * Lấy hoặc đặt tổng số hàng trong tập dữ liệu. (VI)
   */
  totalRow: number
  /**
   * Gets or sets the total number of pages. (EN)
   * 
   * Lấy hoặc đặt tổng số trang. (VI)
   */
  pageCount: number
}

/**
 * Specifies the logical operator used to combine filter conditions. (EN)
 * 
 * Chỉ định toán tử logic được sử dụng để kết hợp các điều kiện lọc. (VI)
 */
export enum FilterLogicalOperator {
  /**
   * Represents the logical AND operator. (EN)
   * 
   * Đại diện cho toán tử logic AND. (VI)
   */
  And = 0,
  /**
   * Represents the logical OR operator. (EN)
   * 
   * Đại diện cho toán tử logic OR. (VI)
   */
  Or = 1
}

/**
 * Defines the types of filters that can be applied. (EN)
 * 
 * Định nghĩa các loại bộ lọc có thể áp dụng. (VI)
 */
export enum FilterType {
  /**
   * Equal to the specified value. (EN)
   * 
   * Bằng giá trị được chỉ định. (VI)
   */
  Equal = 0,
  /**
   * Not equal to the specified value. (EN)
   * 
   * Không bằng giá trị được chỉ định. (VI)
   */
  NotEqual = 1,
  /**
   * Starts with the specified string. (EN)
   * 
   * Bắt đầu bằng chuỗi được chỉ định. (VI)
   */
  StartsWith = 2,
  /**
   * Ends with the specified string. (EN)
   * 
   * Kết thúc bằng chuỗi được chỉ định. (VI)
   */
  EndsWith = 3,
  /**
   * Greater than the specified value. (EN)
   * 
   * Lớn hơn giá trị được chỉ định. (VI)
   */
  GreaterThan = 4,
  /**
   * Less than the specified value. (EN)
   * 
   * Nhỏ hơn giá trị được chỉ định. (VI)
   */
  LessThan = 5,
  /**
   * Less than or equal to the specified value. (EN)
   * 
   * Nhỏ hơn hoặc bằng giá trị được chỉ định. (VI)
   */
  LessThanOrEqual = 6,
  /**
   * Greater than or equal to the specified value. (EN)
   * 
   * Lớn hơn hoặc bằng giá trị được chỉ định. (VI)
   */
  GreaterThanOrEqual = 7,
  /**
   * Between the specified values. (EN)
   * 
   * Nằm giữa các giá trị được chỉ định. (VI)
   */
  Between = 8,
  /**
   * Not between the specified values. (EN)
   * 
   * Không nằm giữa các giá trị được chỉ định. (VI)
   */
  NotBetween = 9,
  /**
   * Is not null. (EN)
   * 
   * Không phải null. (VI)
   */
  IsNotNull = 10,
  /**
   * Is null. (EN)
   * 
   * Là null. (VI)
   */
  IsNull = 11,
  /**
   * Is not null or white space. (EN)
   * 
   * Không phải null hoặc khoảng trắng. (VI)
   */
  IsNotNullOrWhiteSpace = 12,
  /**
   * Is null or white space. (EN)
   * 
   * Là null hoặc khoảng trắng. (VI)
   */
  IsNullOrWhiteSpace = 13,
  /**
   * Is empty string. (EN)
   * 
   * Là chuỗi rỗng. (VI)
   */
  IsEmpty = 14,
  /**
   * Is not empty string. (EN)
   * 
   * Không phải chuỗi rỗng. (VI)
   */
  IsNotEmpty = 15,
  /**
   * Is in the specified list of values. (EN)
   * 
   * Nằm trong danh sách các giá trị được chỉ định. (VI)
   */
  In = 16,
  /**
   * Is not in the specified list of values. (EN)
   * 
   * Không nằm trong danh sách các giá trị được chỉ định. (VI)
   */
  NotIn = 17,
  /**
   * Contains the specified substring. (EN)
   * 
   * Chứa chuỗi con được chỉ định. (VI)
   */
  Contains = 18,
  /**
   * Does not contain the specified substring. (EN)
   * 
   * Không chứa chuỗi con được chỉ định. (VI)
   */
  NotContains = 19,
}

/**
 * Defines the sorting direction. (EN)
 * 
 * Định nghĩa hướng sắp xếp. (VI)
 */
export enum SortDirection {
  /**
   * Ascending order. (EN)
   * 
   * Thứ tự tăng dần. (VI)
   */
  Ascending = 0,
  /**
   * Descending order. (EN)
   * 
   * Thứ tự giảm dần. (VI)
   */
  Descending = 1
}

/**
 * Represents the details of a filter condition. (EN)
 * 
 * Đại diện cho chi tiết của một điều kiện lọc. (VI)
 */
export interface FilterDetailsRequest {
  /**
   * Gets or sets the name of the attribute to filter on. (EN)
   * 
   * Lấy hoặc đặt tên thuộc tính để lọc. (VI)
   */
  attributeName?: string
  /**
   * Gets or sets the value to filter by. (EN)
   * 
   * Lấy hoặc đặt giá trị để lọc. (VI)
   */
  value?: string
  /**
   * Gets or sets the type of filter to apply (e.g., Equals, Contains, GreaterThan). (EN)
   * 
   * Lấy hoặc đặt kiểu lọc để áp dụng (ví dụ: Bằng, Chứa, Lớn hơn). (VI)
   */
  filterType: FilterType
}

/**
 * Represents a filter request with logical operator and details. (EN)
 * 
 * Đại diện cho một yêu cầu lọc với toán tử logic và chi tiết. (VI)
 */
export interface FilterRequest {
  /**
   * Gets or sets the logical operator for combining filter details (e.g., And, Or). (EN)
   * 
   * Lấy hoặc đặt toán tử logic để kết hợp các chi tiết lọc (ví dụ: And, Or). (VI)
   */
  logicalOperator: FilterLogicalOperator
  /**
   * Gets or sets the collection of filter details. (EN)
   * 
   * Lấy hoặc đặt tập hợp các chi tiết lọc. (VI)
   */
  details?: FilterDetailsRequest[]
}

/**
 * Represents sorting criteria for data retrieval. (EN)
 * 
 * Đại diện cho tiêu chí sắp xếp để truy xuất dữ liệu. (VI)
 */
export interface SortDescriptor {
  /**
   * Gets or sets the field to sort by. (EN)
   * 
   * Lấy hoặc đặt trường để sắp xếp. (VI)
   */
  field?: string
  /**
   * Gets or sets the sort direction (Ascending or Descending). (EN)
   * 
   * Lấy hoặc đặt hướng sắp xếp (Tăng dần hoặc Giảm dần). (VI)
   */
  direction: SortDirection
}

/**
 * Represents a filter request body with pagination and sorting options. (EN)
 * 
 * Đại diện cho nội dung yêu cầu lọc với các tùy chọn phân trang và sắp xếp. (VI)
 */
export interface FilterBodyRequest {
  /**
   * Gets or sets the language identifier for the request. (EN)
   * 
   * Lấy hoặc đặt định danh ngôn ngữ cho yêu cầu. (VI)
   */
  langId: string
  /**
   * Gets or sets the search value. (EN)
   * 
   * Lấy hoặc đặt giá trị tìm kiếm. (VI)
   */
  searchValue: string
  /**
   * Gets or sets the filter criteria. (EN)
   * 
   * Lấy hoặc đặt tiêu chí lọc. (VI)
   */
  filter: FilterRequest | {}
  /**
   * Gets or sets the sorting descriptors. (EN)
   * 
   * Lấy hoặc đặt các mô tả sắp xếp. (VI)
   */
  orders: SortDescriptor[]
  /**
   * Gets or sets the pagination information. (EN)
   * 
   * Lấy hoặc đặt thông tin phân trang. (VI)
   */
  pagination: Pagination
}

/**
 * Represents base pagination information for a collection of data. (EN)
 * 
 * Đại diện cho thông tin phân trang cơ sở cho một tập hợp dữ liệu. (VI)
 * 
 * @template T The type of data in the collection.
 */
export interface ApiResponse<T> {
  /**
   * Gets or sets the collection of data for the current page. (EN)
   * 
   * Lấy hoặc đặt tập hợp dữ liệu cho trang hiện tại. (VI)
   */
  data: T[]
  /**
   * Gets or sets the pagination details. (EN)
   * 
   * Lấy hoặc đặt chi tiết phân trang. (VI)
   */
  pagination: Pagination
}

/**
 * Represents an API error response. (EN)
 * 
 * Đại diện cho phản hồi lỗi từ API. (VI)
 */
export interface ApiError {
  /**
   * Gets or sets the error message. (EN)
   * 
   * Lấy hoặc đặt thông báo lỗi. (VI)
   */
  message: string
  /**
   * Gets or sets the HTTP status code. (EN)
   * 
   * Lấy hoặc đặt mã trạng thái HTTP. (VI)
   */
  statusCode: number
  /**
   * Gets or sets the validation errors by field. (EN)
   * 
   * Lấy hoặc đặt các lỗi xác thực theo trường. (VI)
   */
  errors?: Record<string, string[]>
}

/**
 * Represents a view model for transaction data. (EN)
 * 
 * Đại diện cho view model dữ liệu giao dịch. (VI)
 */
export interface TransactionViewModel {
  /**
   * The unique identifier for the transaction. (EN)
   * 
   * Định danh duy nhất cho giao dịch. (VI)
   */
  id: string
  /**
   * The ID of the account associated with the transaction. (EN)
   * 
   * ID của tài khoản liên quan đến giao dịch. (VI)
   */
  accountId: string
  /**
   * The ID of the user who performed the transaction (optional). (EN)
   * 
   * ID của người dùng thực hiện giao dịch (tùy chọn). (VI)
   */
  userId?: string | null
  /**
   * The date and time of the transaction. (EN)
   * 
   * Ngày và giờ thực hiện giao dịch. (VI)
   */
  transactionDate: string
  /**
   * The revenue amount of the transaction. (EN)
   * 
   * Số tiền thu vào của giao dịch. (VI)
   */
  revenueAmount: number
  /**
   * The spent amount of the transaction. (EN)
   * 
   * Số tiền chi ra của giao dịch. (VI)
   */
  spentAmount: number
  /**
   * A description of the transaction (optional). (EN)
   * 
   * Mô tả về giao dịch (tùy chọn). (VI)
   */
  description?: string | null
  /**
   * The balance of the account after the transaction. (EN)
   * 
   * Số dư tài khoản sau giao dịch. (VI)
   */
  balance: number
  /**
   * The balance of the account before the transaction for comparison (optional). (EN)
   * 
   * Số dư tài khoản trước giao dịch để so sánh (tùy chọn). (VI)
   */
  balanceCompare?: number | null
  /**
   * The available credit limit after the transaction for credit card accounts (optional). (EN)
   * 
   * Hạn mức tín dụng khả dụng sau giao dịch cho tài khoản thẻ tín dụng (tùy chọn). (VI)
   */
  availableLimit?: number | null
  /**
   * The available credit limit before the transaction for comparison (optional). (EN)
   * 
   * Hạn mức tín dụng khả dụng trước giao dịch để so sánh (tùy chọn). (VI)
   */
  availableLimitCompare?: number | null
  /**
   * The transaction code (optional). (EN)
   * 
   * Mã giao dịch (tùy chọn). (VI)
   */
  transactionCode?: string | null
  /**
   * Indicates if the transaction was synced from Misa (optional). (EN)
   * 
   * Cho biết giao dịch có được đồng bộ từ Misa hay không (tùy chọn). (VI)
   */
  syncMisa: boolean
  /**
   * Indicates if the transaction was synced from SMS (optional). (EN)
   * 
   * Cho biết giao dịch có được đồng bộ từ SMS hay không (tùy chọn). (VI)
   */
  syncSms: boolean
  /**
   * Indicates if the transaction is related to Vietnam Dong currency (optional). (EN)
   * 
   * Cho biết giao dịch có liên quan đến tiền tệ Việt Nam Đồng hay không (tùy chọn). (VI)
   */
  vn: boolean
  /**
   * A summary of the transaction category (optional). (EN)
   * 
   * Tóm tắt danh mục giao dịch (tùy chọn). (VI)
   */
  categorySummary?: string | null
  /**
   * Additional notes about the transaction (optional). (EN)
   * 
   * Ghi chú bổ sung về giao dịch (tùy chọn). (VI)
   */
  note?: string | null
  /**
   * The source from which the transaction was imported (optional). (EN)
   * 
   * Nguồn mà giao dịch được import từ đó (tùy chọn). (VI)
   */
  importFrom?: string | null
  /**
   * The amount by which the credit limit was increased (optional). (EN)
   * 
   * Số tiền hạn mức tín dụng được tăng thêm (tùy chọn). (VI)
   */
  increaseCreditLimit?: number | null
  /**
   * The percentage of the credit limit used (optional). (EN)
   * 
   * Tỷ lệ phần trăm hạn mức tín dụng đã sử dụng (tùy chọn). (VI)
   */
  usedPercent?: number | null
  /**
   * The type of transaction category. (EN)
   * 
   * Loại danh mục giao dịch. (VI)
   */
  categoryType: number
  /**
   * The transaction group (optional). (EN)
   * 
   * Nhóm giao dịch (tùy chọn). (VI)
   */
  group?: string | null
}

/**
 * Represents a request to create a new transaction. (EN)
 * 
 * Đại diện cho request tạo giao dịch mới. (VI)
 */
export interface TransactionCreateRequest {
  /**
   * The ID of the account associated with the transaction. (EN)
   * 
   * ID của tài khoản liên quan đến giao dịch. (VI)
   */
  accountId: string
  /**
   * The ID of the user who performed the transaction (optional). (EN)
   * 
   * ID của người dùng thực hiện giao dịch (tùy chọn). (VI)
   */
  userId?: string | null
  /**
   * The date and time of the transaction. (EN)
   * 
   * Ngày và giờ thực hiện giao dịch. (VI)
   */
  transactionDate: string
  /**
   * The revenue amount of the transaction. (EN)
   * 
   * Số tiền thu vào của giao dịch. (VI)
   */
  revenueAmount: number
  /**
   * The spent amount of the transaction. (EN)
   * 
   * Số tiền chi ra của giao dịch. (VI)
   */
  spentAmount: number
  /**
   * A description of the transaction (optional). (EN)
   * 
   * Mô tả về giao dịch (tùy chọn). (VI)
   */
  description?: string | null
  /**
   * The balance of the account after the transaction (optional, can be calculated automatically). (EN)
   * 
   * Số dư tài khoản sau giao dịch (tùy chọn, có thể được tính tự động). (VI)
   */
  balance?: number | null
  /**
   * The balance of the account before the transaction for comparison (optional). (EN)
   * 
   * Số dư tài khoản trước giao dịch để so sánh (tùy chọn). (VI)
   */
  balanceCompare?: number | null
  /**
   * The available credit limit after the transaction for credit card accounts (optional). (EN)
   * 
   * Hạn mức tín dụng khả dụng sau giao dịch cho tài khoản thẻ tín dụng (tùy chọn). (VI)
   */
  availableLimit?: number | null
  /**
   * The available credit limit before the transaction for comparison (optional). (EN)
   * 
   * Hạn mức tín dụng khả dụng trước giao dịch để so sánh (tùy chọn). (VI)
   */
  availableLimitCompare?: number | null
  /**
   * The transaction code (optional). (EN)
   * 
   * Mã giao dịch (tùy chọn). (VI)
   */
  transactionCode?: string | null
  /**
   * Indicates if the transaction was synced from Misa (optional). (EN)
   * 
   * Cho biết giao dịch có được đồng bộ từ Misa hay không (tùy chọn). (VI)
   */
  syncMisa: boolean
  /**
   * Indicates if the transaction was synced from SMS (optional). (EN)
   * 
   * Cho biết giao dịch có được đồng bộ từ SMS hay không (tùy chọn). (VI)
   */
  syncSms: boolean
  /**
   * Indicates if the transaction is related to Vietnam Dong currency (optional). (EN)
   * 
   * Cho biết giao dịch có liên quan đến tiền tệ Việt Nam Đồng hay không (tùy chọn). (VI)
   */
  vn: boolean
  /**
   * A summary of the transaction category (optional). (EN)
   * 
   * Tóm tắt danh mục giao dịch (tùy chọn). (VI)
   */
  categorySummary?: string | null
  /**
   * Additional notes about the transaction (optional). (EN)
   * 
   * Ghi chú bổ sung về giao dịch (tùy chọn). (VI)
   */
  note?: string | null
  /**
   * The source from which the transaction was imported (optional). (EN)
   * 
   * Nguồn mà giao dịch được import từ đó (tùy chọn). (VI)
   */
  importFrom?: string | null
  /**
   * The amount by which the credit limit was increased (optional). (EN)
   * 
   * Số tiền hạn mức tín dụng được tăng thêm (tùy chọn). (VI)
   */
  increaseCreditLimit?: number | null
  /**
   * The percentage of the credit limit used (optional). (EN)
   * 
   * Tỷ lệ phần trăm hạn mức tín dụng đã sử dụng (tùy chọn). (VI)
   */
  usedPercent?: number | null
  /**
   * The type of transaction category. (EN)
   * 
   * Loại danh mục giao dịch. (VI)
   */
  categoryType: number
  /**
   * The transaction group (optional). (EN)
   * 
   * Nhóm giao dịch (tùy chọn). (VI)
   */
  group?: string | null
}

/**
 * Represents a request to update an existing transaction. (EN)
 * 
 * Đại diện cho request cập nhật giao dịch hiện có. (VI)
 */
export interface TransactionUpdateRequest extends TransactionCreateRequest {
  /**
   * The unique identifier for the transaction to update. (EN)
   * 
   * Định danh duy nhất cho giao dịch cần cập nhật. (VI)
   */
  id: string
} 