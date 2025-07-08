// API Key Management Types for TiHoMo Frontend
// Based on backend DTOs from Identity.Contracts.ApiKeyDTOs

// ========================================
// Core Types & Enums
// ========================================

export enum ApiKeyStatus {
  Active = 'active',
  Revoked = 'revoked', 
  Expired = 'expired'
}

export enum ApiKeyScope {
  Read = 'read',
  Write = 'write',
  Delete = 'delete',
  TransactionsRead = 'transactions:read',
  TransactionsWrite = 'transactions:write',
  AccountsRead = 'accounts:read',
  AccountsWrite = 'accounts:write',
  Admin = 'admin'
}

export enum UsageGroupBy {
  Hour = 'hour',
  Day = 'day',
  Week = 'week',
  Month = 'month'
}

// ========================================
// Security Settings
// ========================================

export interface ApiKeySecuritySettings {
  requireHttps: boolean
  allowCorsRequests: boolean
  allowedOrigins: string[]
  enableUsageAnalytics: boolean
  maxRequestsPerSecond?: number
  enableIpValidation: boolean
  enableRateLimiting: boolean
}

// ========================================
// Usage Analytics Types
// ========================================

export interface UsageStatistics {
  totalRequests: number
  successfulRequests: number
  failedRequests: number
  averageResponseTime: number
  requestsToday: number
  requestsThisWeek: number
  requestsThisMonth: number
  mostUsedEndpoint?: string
  peakUsageHour?: number
}

export interface UsageDataPoint {
  date: string // ISO date string
  requestCount: number
  errorCount: number
  averageResponseTime: number
}

export interface ApiKeyUsageLog {
  id: string
  timestamp: string // ISO date string
  method: string
  endpoint: string
  statusCode: number
  responseTime: number
  ipAddress: string
  userAgent?: string
  requestSize: number
  responseSize: number
  errorMessage?: string
  scopesUsed: string[]
  isSuccess: boolean
}

// ========================================
// Pagination & Metadata
// ========================================

export interface PaginationInfo {
  nextCursor?: string
  hasMore: boolean
  limit: number
  totalCount?: number
}

export interface ResponseMeta {
  timestamp: string // ISO date string
  correlationId?: string
  requestId?: string
  processingTime?: number
}

// ========================================
// Core API Key Interface
// ========================================

export interface ApiKey {
  id: string
  name: string
  keyPrefix: string
  description?: string
  scopes: string[]
  status: ApiKeyStatus
  rateLimitPerMinute: number
  dailyUsageQuota: number
  todayUsageCount: number
  usageCount: number
  ipWhitelist: string[]
  securitySettings: ApiKeySecuritySettings
  createdAt: string // ISO date string
  updatedAt: string // ISO date string
  expiresAt?: string // ISO date string
  lastUsedAt?: string // ISO date string
  revokedAt?: string // ISO date string
  isActive: boolean
  isExpired: boolean
  isRevoked: boolean
  isRateLimited: boolean
}

// ========================================
// Request Types (Form Data)
// ========================================

export interface CreateApiKeyRequest {
  name: string
  description?: string
  scopes: string[]
  expiresAt?: string // ISO date string
  rateLimitPerMinute: number
  dailyUsageQuota: number
  ipWhitelist: string[]
  securitySettings: ApiKeySecuritySettings
}

export interface UpdateApiKeyRequest {
  name?: string
  description?: string
  scopes?: string[]
  expiresAt?: string // ISO date string
  rateLimitPerMinute?: number
  dailyUsageQuota?: number
  ipWhitelist?: string[]
  securitySettings?: ApiKeySecuritySettings
}

export interface ListApiKeysQuery {
  status?: string
  scope?: string
  search?: string
  cursor?: string
  limit?: number
}

export interface UsageQueryRequest {
  startDate?: string // ISO date string
  endDate?: string // ISO date string
  groupBy: UsageGroupBy
  includeErrors: boolean
  limit: number
}

// ========================================
// Response Types (API Results)
// ========================================

export interface CreateApiKeyResponse {
  id: string
  name: string
  apiKey: string // Full key - shown only once!
  keyPrefix: string
  description?: string
  scopes: string[]
  rateLimitPerMinute: number
  dailyUsageQuota: number
  ipWhitelist: string[]
  securitySettings: ApiKeySecuritySettings
  createdAt: string // ISO date string
  expiresAt?: string // ISO date string
}

export interface ListApiKeysResponse {
  data: ApiKey[]
  pagination: PaginationInfo
  meta: ResponseMeta
}

export interface ApiKeyUsageResponse {
  apiKeyId: string
  statistics: UsageStatistics
  usageData: UsageDataPoint[]
  recentActivities: ApiKeyUsageLog[]
}

export interface RotateApiKeyResponse {
  id: string
  newApiKey: string // Full new key - shown only once!
  newKeyPrefix: string
  oldKeyPrefix: string
  rotatedAt: string // ISO date string
}

export interface VerifyApiKeyResponse {
  isValid: boolean
  userId?: string
  apiKeyId?: string
  scopes: string[]
  message: string
  errorMessage?: string
}

// ========================================
// UI State Types
// ========================================

export interface ApiKeyFormState {
  // Form data
  form: CreateApiKeyRequest
  
  // UI state
  loading: boolean
  showAdvancedSettings: boolean
  errors: Record<string, string>
  
  // Security score calculation
  securityScore: number
  securityWarnings: string[]
  
  // Scope templates
  selectedTemplate?: ScopeTemplate
}

export interface ScopeTemplate {
  id: string
  name: string
  description: string
  scopes: string[]
  icon: string
  isRecommended: boolean
}

export interface ApiKeyListState {
  // Data
  apiKeys: ApiKey[]
  selectedApiKey?: ApiKey
  
  // Filters
  filters: {
    status: string
    scope: string
    search: string
  }
  
  // UI state
  loading: boolean
  showAdvancedColumns: boolean
  viewMode: 'list' | 'grid'
  
  // Pagination
  pagination: PaginationInfo
}

export interface ApiKeyDetailState {
  apiKey: ApiKey
  usage: ApiKeyUsageResponse
  loading: {
    usage: boolean
    updating: boolean
    revoking: boolean
    rotating: boolean
  }
  showUsageAnalytics: boolean
  showSecurityDetails: boolean
}

// ========================================
// Utility Types
// ========================================

export type ApiKeyAction = 'view' | 'edit' | 'revoke' | 'rotate' | 'regenerate' | 'download'

export interface ApiKeyActionItem {
  action: ApiKeyAction
  label: string
  icon: string
  variant: 'primary' | 'secondary' | 'warning' | 'danger'
  disabled?: boolean
  requiresConfirmation?: boolean
  confirmationMessage?: string
}

// ========================================
// API Error Types
// ========================================

export interface ApiKeyError {
  code: string
  message: string
  field?: string
  details?: Record<string, any>
}

export interface ApiKeyValidationError {
  errors: Record<string, string[]>
  message: string
}

// ========================================
// Composable Return Types
// ========================================

export interface UseApiKeysReturn {
  // State
  apiKeys: Ref<ApiKey[]>
  loading: Ref<boolean>
  error: Ref<string | null>
  
  // Actions
  fetchApiKeys: (query?: ListApiKeysQuery) => Promise<void>
  createApiKey: (request: CreateApiKeyRequest) => Promise<CreateApiKeyResponse>
  updateApiKey: (id: string, request: UpdateApiKeyRequest) => Promise<void>
  revokeApiKey: (id: string) => Promise<void>
  rotateApiKey: (id: string) => Promise<RotateApiKeyResponse>
  
  // Utilities
  refreshData: () => Promise<void>
  clearError: () => void
}

export interface UseApiKeyManagerReturn {
  // Form state
  formState: Ref<ApiKeyFormState>
  
  // List state  
  listState: Ref<ApiKeyListState>
  
  // Detail state
  detailState: Ref<ApiKeyDetailState | null>
  
  // Actions
  setSelectedApiKey: (id: string) => void
  showCreateForm: () => void
  hideCreateForm: () => void
  updateFilters: (filters: Partial<ApiKeyListState['filters']>) => void
  
  // Form validation
  validateForm: () => boolean
  calculateSecurityScore: () => number
  getScopeTemplates: () => ScopeTemplate[]
  
  // Usage analytics
  fetchUsageData: (apiKeyId: string, query?: UsageQueryRequest) => Promise<void>
  
  // Security utilities
  validateIpAddress: (ip: string) => boolean
  getScopeSuggestions: (existingScopes: string[]) => string[]
  calculateRateLimitSuggestion: (scopes: string[]) => number
}

// ========================================
// Constants & Defaults
// ========================================

export const DEFAULT_API_KEY_REQUEST: CreateApiKeyRequest = {
  name: '',
  description: '',
  scopes: [ApiKeyScope.Read],
  rateLimitPerMinute: 100,
  dailyUsageQuota: 10000,
  ipWhitelist: [],
  securitySettings: {
    requireHttps: true,
    allowCorsRequests: false,
    allowedOrigins: [],
    enableUsageAnalytics: true,
    enableIpValidation: false,
    enableRateLimiting: true
  }
}

export const SCOPE_TEMPLATES: ScopeTemplate[] = [
  {
    id: 'read-only',
    name: 'Read Only',
    description: 'View data only, no modifications',
    scopes: [ApiKeyScope.Read, ApiKeyScope.AccountsRead, ApiKeyScope.TransactionsRead],
    icon: 'eye',
    isRecommended: true
  },
  {
    id: 'full-access',
    name: 'Full Access',
    description: 'Complete access to all resources',
    scopes: [ApiKeyScope.Admin],
    icon: 'key',
    isRecommended: false
  },
  {
    id: 'transactions-only',
    name: 'Transactions Only',
    description: 'Access to transaction data only',
    scopes: [ApiKeyScope.TransactionsRead, ApiKeyScope.TransactionsWrite],
    icon: 'credit-card',
    isRecommended: true
  },
  {
    id: 'accounts-only',
    name: 'Accounts Only', 
    description: 'Access to account data only',
    scopes: [ApiKeyScope.AccountsRead, ApiKeyScope.AccountsWrite],
    icon: 'bank',
    isRecommended: true
  }
] 