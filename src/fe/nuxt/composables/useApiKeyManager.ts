import type {
  ApiKeyFormState,
  ApiKeyListState,
  ApiKeyDetailState,
  CreateApiKeyRequest,
  UpdateApiKeyRequest,
  UsageQueryRequest,
  ScopeTemplate,
  ApiKey
} from '~/types/api-key'
import { 
  DEFAULT_API_KEY_REQUEST, 
  SCOPE_TEMPLATES,
  ApiKeyScope,
  ApiKeyStatus 
} from '~/types/api-key'

/**
 * Composable for managing API Key UI state and business logic (EN)
 * Composable để quản lý trạng thái UI và logic nghiệp vụ API Key (VI)
 */
export const useApiKeyManager = () => {
  const { 
    apiKeys, 
    loading, 
    error, 
    fetchApiKeys, 
    createApiKey: createApiKeyApi, 
    updateApiKey: updateApiKeyApi,
    revokeApiKey: revokeApiKeyApi,
    rotateApiKey: rotateApiKeyApi,
    getApiKeyUsage,
    getSecurityScore,
    refreshData
  } = useApiKeys()

  // Form state
  const formState = ref<ApiKeyFormState>({
    form: { ...DEFAULT_API_KEY_REQUEST },
    loading: false,
    showAdvancedSettings: false,
    errors: {},
    securityScore: 0,
    securityWarnings: []
  })

  // List state
  const listState = ref<ApiKeyListState>({
    apiKeys: [],
    filters: {
      status: '',
      scope: '',
      search: ''
    },
    loading: false,
    showAdvancedColumns: false,
    viewMode: 'list',
    pagination: {
      nextCursor: undefined,
      hasMore: false,
      limit: 20,
      totalCount: undefined
    }
  })

  // Detail state
  const detailState = ref<ApiKeyDetailState | null>(null)

  // Show/hide states
  const showCreateModal = ref(false)
  const showEditModal = ref(false)
  const showDetailPanel = ref(false)

  /**
   * Initialize form with default values (EN)
   * Khởi tạo form với giá trị mặc định (VI)
   */
  const initializeForm = (): void => {
    formState.value = {
      form: { ...DEFAULT_API_KEY_REQUEST },
      loading: false,
      showAdvancedSettings: false,
      errors: {},
      securityScore: calculateSecurityScore(),
      securityWarnings: []
    }
  }

  /**
   * Set selected API key for viewing/editing (EN)
   * Đặt API key được chọn để xem/chỉnh sửa (VI)
   */
  const setSelectedApiKey = (apiKey: ApiKey): void => {
    detailState.value = {
      apiKey,
      usage: {
        apiKeyId: apiKey.id,
        statistics: {
          totalRequests: 0,
          successfulRequests: 0,
          failedRequests: 0,
          averageResponseTime: 0,
          requestsToday: apiKey.todayUsageCount,
          requestsThisWeek: 0,
          requestsThisMonth: 0
        },
        usageData: [],
        recentActivities: []
      },
      loading: {
        usage: false,
        updating: false,
        revoking: false,
        rotating: false
      },
      showUsageAnalytics: false,
      showSecurityDetails: true
    }
    
    showDetailPanel.value = true
  }

  /**
   * Show create form modal (EN)
   * Hiển thị modal tạo form (VI)
   */
  const showCreateForm = (): void => {
    initializeForm()
    showCreateModal.value = true
  }

  /**
   * Hide create form modal (EN)
   * Ẩn modal tạo form (VI)
   */
  const hideCreateForm = (): void => {
    showCreateModal.value = false
    initializeForm()
  }

  /**
   * Show edit form modal (EN)
   * Hiển thị modal chỉnh sửa form (VI)
   */
  const showEditForm = (apiKey: ApiKey): void => {
    formState.value.form = {
      name: apiKey.name,
      description: apiKey.description || '',
      scopes: [...apiKey.scopes],
      expiresAt: apiKey.expiresAt,
      rateLimitPerMinute: apiKey.rateLimitPerMinute,
      dailyUsageQuota: apiKey.dailyUsageQuota,
      ipWhitelist: [...apiKey.ipWhitelist],
      securitySettings: { ...apiKey.securitySettings }
    }
    formState.value.securityScore = calculateSecurityScore()
    showEditModal.value = true
  }

  /**
   * Hide edit form modal (EN)
   * Ẩn modal chỉnh sửa form (VI)
   */
  const hideEditForm = (): void => {
    showEditModal.value = false
    initializeForm()
  }

  /**
   * Update filters and refresh data (EN)
   * Cập nhật bộ lọc và làm mới dữ liệu (VI)
   */
  const updateFilters = async (filters: Partial<ApiKeyListState['filters']>): Promise<void> => {
    listState.value.filters = { ...listState.value.filters, ...filters }
    await fetchApiKeys(listState.value.filters)
  }

  /**
   * Validate form data (EN)
   * Xác thực dữ liệu form (VI)
   */
  const validateForm = (): boolean => {
    const errors: Record<string, string> = {}
    const form = formState.value.form

    // Name validation
    if (!form.name.trim()) {
      errors.name = 'API key name is required'
    } else if (form.name.length < 3) {
      errors.name = 'Name must be at least 3 characters'
    } else if (form.name.length > 100) {
      errors.name = 'Name must be less than 100 characters'
    }

    // Description validation
    if (form.description && form.description.length > 500) {
      errors.description = 'Description must be less than 500 characters'
    }

    // Scopes validation
    if (!form.scopes || form.scopes.length === 0) {
      errors.scopes = 'At least one scope must be selected'
    }

    // Rate limit validation
    if (form.rateLimitPerMinute < 1 || form.rateLimitPerMinute > 1000) {
      errors.rateLimitPerMinute = 'Rate limit must be between 1 and 1000 requests per minute'
    }

    // Daily quota validation
    if (form.dailyUsageQuota < 100 || form.dailyUsageQuota > 100000) {
      errors.dailyUsageQuota = 'Daily quota must be between 100 and 100,000 requests'
    }

    // Expiration validation
    if (form.expiresAt) {
      const expiryDate = new Date(form.expiresAt)
      const now = new Date()
      const maxDate = new Date(now.getFullYear() + 2, now.getMonth(), now.getDate())
      
      if (expiryDate <= now) {
        errors.expiresAt = 'Expiration date must be in the future'
      } else if (expiryDate > maxDate) {
        errors.expiresAt = 'Expiration date cannot be more than 2 years from now'
      }
    }

    // IP whitelist validation
    if (form.ipWhitelist.length > 0) {
      for (let i = 0; i < form.ipWhitelist.length; i++) {
        if (!validateIpAddress(form.ipWhitelist[i])) {
          errors.ipWhitelist = `Invalid IP address at position ${i + 1}: ${form.ipWhitelist[i]}`
          break
        }
      }
    }

    formState.value.errors = errors
    return Object.keys(errors).length === 0
  }

  /**
   * Calculate security score for current form (EN)
   * Tính điểm bảo mật cho form hiện tại (VI)
   */
  const calculateSecurityScore = (): number => {
    const form = formState.value.form
    let score = 0
    const warnings: string[] = []

    // HTTPS requirement (+20)
    if (form.securitySettings.requireHttps) {
      score += 20
    } else {
      warnings.push('HTTPS not required - consider enabling for better security')
    }

    // IP validation (+25)
    if (form.securitySettings.enableIpValidation && form.ipWhitelist.length > 0) {
      score += 25
    } else if (form.ipWhitelist.length === 0) {
      warnings.push('No IP restrictions - consider adding IP whitelist')
    }

    // Rate limiting (+15)
    if (form.securitySettings.enableRateLimiting) {
      score += 15
    } else {
      warnings.push('Rate limiting disabled - this could allow abuse')
    }

    // Expiration set (+15)
    if (form.expiresAt) {
      score += 15
    } else {
      warnings.push('No expiration date - consider setting an expiry for better security')
    }

    // Limited scopes (+10 for non-admin, +5 for admin)
    if (form.scopes.includes(ApiKeyScope.Admin)) {
      score += 5
      warnings.push('Admin scope granted - ensure this is necessary')
    } else {
      score += 10
    }

    // Usage analytics (+10)
    if (form.securitySettings.enableUsageAnalytics) {
      score += 10
    } else {
      warnings.push('Usage analytics disabled - you won\'t be able to monitor key usage')
    }

    formState.value.securityWarnings = warnings
    return Math.min(score, 100)
  }

  /**
   * Get scope templates for quick selection (EN)
   * Lấy mẫu scope để chọn nhanh (VI)
   */
  const getScopeTemplates = (): ScopeTemplate[] => {
    return SCOPE_TEMPLATES
  }

  /**
   * Apply scope template to form (EN)
   * Áp dụng mẫu scope vào form (VI)
   */
  const applyScopeTemplate = (template: ScopeTemplate): void => {
    formState.value.form.scopes = [...template.scopes]
    formState.value.selectedTemplate = template
    formState.value.securityScore = calculateSecurityScore()
  }

  /**
   * Validate IP address format (EN)
   * Xác thực định dạng địa chỉ IP (VI)
   */
  const validateIpAddress = (ip: string): boolean => {
    // IPv4 regex
    const ipv4Regex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/
    
    // IPv4 with CIDR regex
    const cidrRegex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\/(?:[0-9]|[1-2][0-9]|3[0-2])$/
    
    // IPv6 basic check (simplified)
    const ipv6Regex = /^(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$/
    
    return ipv4Regex.test(ip) || cidrRegex.test(ip) || ipv6Regex.test(ip)
  }

  /**
   * Get scope suggestions based on existing scopes (EN)
   * Lấy gợi ý scope dựa trên scope hiện có (VI)
   */
  const getScopeSuggestions = (existingScopes: string[]): string[] => {
    const suggestions: string[] = []
    
    // If only read, suggest adding specific read scopes
    if (existingScopes.includes(ApiKeyScope.Read) && existingScopes.length === 1) {
      suggestions.push(ApiKeyScope.AccountsRead, ApiKeyScope.TransactionsRead)
    }
    
    // If has read scopes, suggest corresponding write scopes
    if (existingScopes.includes(ApiKeyScope.AccountsRead) && !existingScopes.includes(ApiKeyScope.AccountsWrite)) {
      suggestions.push(ApiKeyScope.AccountsWrite)
    }
    
    if (existingScopes.includes(ApiKeyScope.TransactionsRead) && !existingScopes.includes(ApiKeyScope.TransactionsWrite)) {
      suggestions.push(ApiKeyScope.TransactionsWrite)
    }
    
    return suggestions
  }

  /**
   * Calculate rate limit suggestion based on scopes (EN)
   * Tính gợi ý giới hạn tốc độ dựa trên scope (VI)
   */
  const calculateRateLimitSuggestion = (scopes: string[]): number => {
    // Admin scope - higher limits
    if (scopes.includes(ApiKeyScope.Admin)) {
      return 500
    }
    
    // Write scopes - moderate limits
    if (scopes.some(scope => scope.includes('write') || scope === ApiKeyScope.Write)) {
      return 200
    }
    
    // Read-only scopes - standard limits
    return 100
  }

  /**
   * Create API key with validation (EN)
   * Tạo API key với xác thực (VI)
   */
  const createApiKey = async (): Promise<{ success: boolean; data?: any; error?: string }> => {
    if (!validateForm()) {
      return { success: false, error: 'Form validation failed' }
    }

    try {
      formState.value.loading = true
      const response = await createApiKeyApi(formState.value.form)
      
      hideCreateForm()
      await refreshData()
      
      return { success: true, data: response }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to create API key'
      return { success: false, error: errorMessage }
    } finally {
      formState.value.loading = false
    }
  }

  /**
   * Update API key with validation (EN)
   * Cập nhật API key với xác thực (VI)
   */
  const updateApiKey = async (id: string): Promise<{ success: boolean; error?: string }> => {
    if (!validateForm()) {
      return { success: false, error: 'Form validation failed' }
    }

    try {
      formState.value.loading = true
      
      const updateRequest: UpdateApiKeyRequest = {
        name: formState.value.form.name,
        description: formState.value.form.description,
        scopes: formState.value.form.scopes,
        expiresAt: formState.value.form.expiresAt,
        rateLimitPerMinute: formState.value.form.rateLimitPerMinute,
        dailyUsageQuota: formState.value.form.dailyUsageQuota,
        ipWhitelist: formState.value.form.ipWhitelist,
        securitySettings: formState.value.form.securitySettings
      }
      
      await updateApiKeyApi(id, updateRequest)
      
      hideEditForm()
      await refreshData()
      
      return { success: true }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update API key'
      return { success: false, error: errorMessage }
    } finally {
      formState.value.loading = false
    }
  }

  /**
   * Fetch usage data for selected API key (EN)
   * Lấy dữ liệu sử dụng cho API key được chọn (VI)
   */
  const fetchUsageData = async (apiKeyId: string, query?: UsageQueryRequest): Promise<void> => {
    if (!detailState.value) return

    try {
      detailState.value.loading.usage = true
      const usage = await getApiKeyUsage(apiKeyId, query)
      
      if (usage) {
        detailState.value.usage = usage
      }
    } catch (err) {
      console.error('Failed to fetch usage data:', err)
    } finally {
      if (detailState.value) {
        detailState.value.loading.usage = false
      }
    }
  }

  /**
   * Toggle usage analytics view (EN)
   * Chuyển đổi hiển thị phân tích sử dụng (VI)
   */
  const toggleUsageAnalytics = (): void => {
    if (detailState.value) {
      detailState.value.showUsageAnalytics = !detailState.value.showUsageAnalytics
      
      // Fetch usage data if showing for first time
      if (detailState.value.showUsageAnalytics && detailState.value.usage.usageData.length === 0) {
        fetchUsageData(detailState.value.apiKey.id)
      }
    }
  }

  /**
   * Watch form changes to update security score (EN)
   * Theo dõi thay đổi form để cập nhật điểm bảo mật (VI)
   */
  watch(
    () => formState.value.form,
    () => {
      formState.value.securityScore = calculateSecurityScore()
    },
    { deep: true }
  )

  return {
    // State
    formState,
    listState,
    detailState,
    
    // Modal states
    showCreateModal,
    showEditModal,
    showDetailPanel,
    
    // Global state from useApiKeys
    apiKeys,
    loading,
    error,

    // Form actions
    showCreateForm,
    hideCreateForm,
    showEditForm,
    hideEditForm,
    initializeForm,
    
    // API actions
    createApiKey,
    updateApiKey,
    
    // Data management
    setSelectedApiKey,
    updateFilters,
    fetchUsageData,
    toggleUsageAnalytics,
    
    // Form validation
    validateForm,
    calculateSecurityScore,
    
    // Utilities
    getScopeTemplates,
    applyScopeTemplate,
    validateIpAddress,
    getScopeSuggestions,
    calculateRateLimitSuggestion,
    
    // Computed
    securityScoreColor: computed(() => {
      const score = formState.value.securityScore
      if (score >= 80) return 'green'
      if (score >= 60) return 'yellow'
      if (score >= 40) return 'orange'
      return 'red'
    }),
    
    isFormValid: computed(() => Object.keys(formState.value.errors).length === 0),
    
    hasSecurityWarnings: computed(() => formState.value.securityWarnings.length > 0)
  }
} 