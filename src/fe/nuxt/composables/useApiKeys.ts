import type {
  ApiKey,
  CreateApiKeyRequest,
  CreateApiKeyResponse,
  UpdateApiKeyRequest,
  ListApiKeysQuery,
  ListApiKeysResponse,
  RotateApiKeyResponse,
  VerifyApiKeyResponse,
  ApiKeyUsageResponse,
  UsageQueryRequest,
  UseApiKeysReturn
} from '~/types/api-key'
import { ApiKeyStatus } from '~/types/api-key'

/**
 * Composable for API Key management operations (EN)
 * Composable để quản lý các thao tác API Key (VI)
 */
export const useApiKeys = () => {
  const { post, put, delete: deleteRequest } = useApi()
  
  // Reactive state
  const apiKeys = ref<ApiKey[]>([])
  const selectedApiKey = ref<ApiKey | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const pagination = ref({
    nextCursor: null as string | null,
    hasMore: false,
    limit: 20,
    totalCount: null as number | null
  })

  // Current filters
  const currentFilters = ref<ListApiKeysQuery>({
    status: '',
    scope: '',
    search: '',
    limit: 20
  })

  /**
   * Fetch API keys with optional filtering (EN)
   * Lấy danh sách API keys với tùy chọn lọc (VI)
   */
  const fetchApiKeys = async (query?: ListApiKeysQuery): Promise<void> => {
    try {
      isLoading.value = true
      error.value = null

      // Merge current filters with new query
      const mergedQuery = { ...currentFilters.value, ...query }
      
      // Clean up empty filters
      if (query) {
        Object.keys(query).forEach(key => {
          const value = query[key as keyof ListApiKeysQuery]
          if (value === '' || value === null || value === undefined) {
            delete mergedQuery[key as keyof ListApiKeysQuery]
          }
        })
      }
      
      currentFilters.value = mergedQuery

      console.log('Fetching API keys with filters:', mergedQuery)

      const response = await post<ListApiKeysResponse>('/api/identity/enhanced-api-keys/list', mergedQuery)
      
      if (response?.data) {
        // If cursor provided, append to existing data (pagination)
        if (mergedQuery.cursor && apiKeys.value.length > 0) {
          apiKeys.value = [...apiKeys.value, ...response.data]
        } else {
          // Fresh load or first page
          apiKeys.value = response.data
        }
        
        pagination.value = {
          nextCursor: response.pagination.nextCursor || null,
          hasMore: response.pagination.hasMore,
          limit: response.pagination.limit,
          totalCount: response.pagination.totalCount || null
        }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch API keys'
      console.error('Failed to fetch API keys:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Create new API key (EN)
   * Tạo API key mới (VI)
   */
  const createApiKey = async (request: CreateApiKeyRequest): Promise<CreateApiKeyResponse> => {
    try {
      isLoading.value = true
      error.value = null

      console.log('Creating API key:', request)

      const response = await post<CreateApiKeyResponse>('/api/identity/enhanced-api-keys', request)
      
      if (response) {
        // Refresh the list to include new key (without sensitive data)
        await refreshData()
      }
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create API key'
      console.error('Failed to create API key:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Update existing API key (EN)
   * Cập nhật API key hiện tại (VI)
   */
  const updateApiKey = async (id: string, request: UpdateApiKeyRequest): Promise<void> => {
    try {
      isLoading.value = true
      error.value = null

      console.log('Updating API key:', id, request)

      const response = await put<ApiKey>(`/api/identity/enhanced-api-keys/${id}`, request)
      
      if (response) {
        // Update local state
        const index = apiKeys.value.findIndex(key => key.id === id)
        if (index !== -1) {
          apiKeys.value[index] = response
        }
        
        // Update selected if it's the same one
        if (selectedApiKey.value?.id === id) {
          selectedApiKey.value = response
        }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update API key'
      console.error('Failed to update API key:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Revoke API key (EN)
   * Thu hồi API key (VI)
   */
  const revokeApiKey = async (id: string): Promise<void> => {
    try {
      isLoading.value = true
      error.value = null

      console.log('Revoking API key:', id)

      await deleteRequest(`/api/identity/enhanced-api-keys/${id}/revoke`)
      
      // Update local state - mark as revoked
      const key = apiKeys.value.find(k => k.id === id)
      if (key) {
        key.status = ApiKeyStatus.Revoked
        key.isActive = false
        key.revokedAt = new Date().toISOString()
      }
      
      // Update selected if it's the same one
      if (selectedApiKey.value?.id === id) {
        selectedApiKey.value = { ...selectedApiKey.value, status: ApiKeyStatus.Revoked, isActive: false }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to revoke API key'
      console.error('Failed to revoke API key:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Rotate API key (generate new key) (EN)
   * Xoay vòng API key (tạo key mới) (VI)
   */
  const rotateApiKey = async (id: string): Promise<RotateApiKeyResponse> => {
    try {
      isLoading.value = true
      error.value = null

      console.log('Rotating API key:', id)

      const response = await post<RotateApiKeyResponse>(`/api/identity/enhanced-api-keys/${id}/rotate`, {})
      
      if (response) {
        // Update local state with new prefix
        const key = apiKeys.value.find(k => k.id === id)
        if (key) {
          key.keyPrefix = response.newKeyPrefix
          key.updatedAt = response.rotatedAt
        }
        
        // Update selected if it's the same one
        if (selectedApiKey.value?.id === id && key) {
          selectedApiKey.value = { ...key }
        }
      }
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to rotate API key'
      console.error('Failed to rotate API key:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Get API key by ID (EN)
   * Lấy API key theo ID (VI)
   */
  const getApiKeyById = async (id: string): Promise<ApiKey | null> => {
    try {
      isLoading.value = true
      error.value = null

      console.log('Fetching API key by ID:', id)

      const response = await post<ApiKey>(`/api/identity/enhanced-api-keys/${id}`, {})
      
      if (response) {
        selectedApiKey.value = response
        
        // Update in list if exists
        const index = apiKeys.value.findIndex(key => key.id === id)
        if (index !== -1) {
          apiKeys.value[index] = response
        }
      }
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch API key'
      console.error('Failed to fetch API key:', err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Verify API key validity (EN)
   * Xác minh tính hợp lệ của API key (VI)
   */
  const verifyApiKey = async (apiKey: string): Promise<VerifyApiKeyResponse | null> => {
    try {
      isLoading.value = true
      error.value = null

      const response = await post<VerifyApiKeyResponse>('/api/identity/enhanced-api-keys/verify', { apiKey })
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to verify API key'
      console.error('Failed to verify API key:', err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Get API key usage analytics (EN)
   * Lấy phân tích sử dụng API key (VI)
   */
  const getApiKeyUsage = async (id: string, query?: UsageQueryRequest): Promise<ApiKeyUsageResponse | null> => {
    try {
      isLoading.value = true
      error.value = null

      const requestBody = {
        startDate: query?.startDate,
        endDate: query?.endDate,
        groupBy: query?.groupBy || 'day',
        includeErrors: query?.includeErrors ?? true,
        limit: query?.limit || 100
      }

      console.log('Fetching API key usage:', id, requestBody)

      const response = await post<ApiKeyUsageResponse>(`/api/identity/enhanced-api-keys/${id}/usage`, requestBody)
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch API key usage'
      console.error('Failed to fetch API key usage:', err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Load more API keys (pagination) (EN)
   * Tải thêm API keys (phân trang) (VI)
   */
  const loadMore = async (): Promise<void> => {
    if (!pagination.value.hasMore || !pagination.value.nextCursor) {
      return
    }

    await fetchApiKeys({
      ...currentFilters.value,
      cursor: pagination.value.nextCursor
    })
  }

  /**
   * Refresh data (reload first page) (EN)
   * Làm mới dữ liệu (tải lại trang đầu) (VI)
   */
  const refreshData = async (): Promise<void> => {
    // Reset pagination and fetch fresh data
    currentFilters.value.cursor = undefined
    await fetchApiKeys(currentFilters.value)
  }

  /**
   * Clear error state (EN)
   * Xóa trạng thái lỗi (VI)
   */
  const clearError = (): void => {
    error.value = null
  }

  /**
   * Set selected API key (EN)
   * Đặt API key được chọn (VI)
   */
  const setSelectedApiKey = (apiKey: ApiKey | null): void => {
    selectedApiKey.value = apiKey
  }

  /**
   * Filter by status (EN)
   * Lọc theo trạng thái (VI)
   */
  const filterByStatus = async (status: string): Promise<void> => {
    await fetchApiKeys({ ...currentFilters.value, status, cursor: undefined })
  }

  /**
   * Filter by scope (EN)
   * Lọc theo phạm vi (VI)
   */
  const filterByScope = async (scope: string): Promise<void> => {
    await fetchApiKeys({ ...currentFilters.value, scope, cursor: undefined })
  }

  /**
   * Search API keys by name (EN)
   * Tìm kiếm API keys theo tên (VI)
   */
  const searchApiKeys = async (search: string): Promise<void> => {
    await fetchApiKeys({ ...currentFilters.value, search, cursor: undefined })
  }

  /**
   * Clear all filters (EN)
   * Xóa tất cả bộ lọc (VI)
   */
  const clearFilters = async (): Promise<void> => {
    currentFilters.value = { limit: 20 }
    await fetchApiKeys(currentFilters.value)
  }

  /**
   * Get API key security score (EN)
   * Tính điểm bảo mật API key (VI)
   */
  const getSecurityScore = (apiKey: ApiKey): number => {
    let score = 0
    
    // HTTPS requirement (+20)
    if (apiKey.securitySettings.requireHttps) score += 20
    
    // IP validation (+25)
    if (apiKey.securitySettings.enableIpValidation && apiKey.ipWhitelist.length > 0) score += 25
    
    // Rate limiting (+15)
    if (apiKey.securitySettings.enableRateLimiting) score += 15
    
    // Expiration set (+15)
    if (apiKey.expiresAt) score += 15
    
    // Limited scopes (+10 for non-admin, +5 for admin)
    if (apiKey.scopes.includes('admin')) {
      score += 5
    } else {
      score += 10
    }
    
    // Usage analytics (+10)
    if (apiKey.securitySettings.enableUsageAnalytics) score += 10
    
    return Math.min(score, 100)
  }

  return {
    // State
    apiKeys,
    loading: isLoading,
    error,

    // Actions
    fetchApiKeys,
    createApiKey,
    updateApiKey,
    revokeApiKey,
    rotateApiKey,
    getApiKeyById,
    verifyApiKey,
    getApiKeyUsage,
    
    // Pagination
    loadMore,
    refreshData,
    
    // Utilities
    clearError,
    setSelectedApiKey,
    
    // Filtering
    filterByStatus,
    filterByScope,
    searchApiKeys,
    clearFilters,
    
    // Security
    getSecurityScore
  }
} 