import type { SocialProvider, SocialLoginResponse } from '@/types/auth'

/**
 * Unified Social Authentication composable (EN)
 * Composable xác thực xã hội thống nhất (VI)
 */
export const useSocialAuth = () => {
  const googleAuth = useGoogleAuth()
  const facebookAuth = useFacebookAuth()
  
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  /**
   * Check if any social provider is configured (EN)
   * Kiểm tra xem có nhà cung cấp xã hội nào được cấu hình không (VI)
   */
  const hasAvailableProviders = computed(() => {
    return googleAuth.isConfigured.value || facebookAuth.isConfigured.value
  })

  /**
   * Get list of available social providers (EN)
   * Lấy danh sách các nhà cung cấp xã hội có sẵn (VI)
   */
  const availableProviders = computed<SocialProvider[]>(() => {
    const providers: SocialProvider[] = []
    if (googleAuth.isConfigured.value) providers.push('Google')
    if (facebookAuth.isConfigured.value) providers.push('Facebook')
    return providers
  })  /**
   * Login with specified social provider (EN)
   * Đăng nhập với nhà cung cấp xã hội được chỉ định (VI)
   */
  const loginWith = async (provider: SocialProvider): Promise<SocialLoginResponse> => {
    try {
      isLoading.value = true
      error.value = null

      switch (provider) {
        case 'Google':
          if (!googleAuth.isConfigured.value) {
            throw new Error('Google authentication is not configured')
          }
          return await googleAuth.login()

        case 'Facebook':
          if (!facebookAuth.isConfigured.value) {
            throw new Error('Facebook authentication is not configured')
          }
          return await facebookAuth.login()

        default:
          throw new Error(`Unsupported social provider: ${provider}`)
      }
    } catch (err: any) {
      error.value = err.message || `${provider} authentication failed`
      throw err
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Initialize all configured social providers (EN)
   * Khởi tạo tất cả các nhà cung cấp xã hội được cấu hình (VI)
   */
  const initializeProviders = async (): Promise<void> => {
    const initPromises: Promise<void>[] = []

    if (googleAuth.isConfigured.value) {
      initPromises.push(googleAuth.initializeGoogle())
    }

    if (facebookAuth.isConfigured.value) {
      initPromises.push(facebookAuth.initializeFacebook())
    }

    try {
      await Promise.all(initPromises)
    } catch (err) {
      console.warn('Some social providers failed to initialize:', err)
    }
  }

  /**
   * Get provider-specific loading state (EN)
   * Lấy trạng thái loading của từng nhà cung cấp (VI)
   */
  const getProviderLoadingState = (provider: SocialProvider) => {
    switch (provider) {
      case 'Google':
        return googleAuth.isLoading
      case 'Facebook':
        return facebookAuth.isLoading
      default:
        return ref(false)
    }
  }

  /**
   * Get provider-specific error state (EN)
   * Lấy trạng thái lỗi của từng nhà cung cấp (VI)
   */
  const getProviderError = (provider: SocialProvider) => {
    switch (provider) {
      case 'Google':
        return googleAuth.error
      case 'Facebook':
        return facebookAuth.error
      default:
        return ref(null)
    }
  }

  return {
    // State
    isLoading: readonly(isLoading),
    error: readonly(error),
    
    // Computed
    hasAvailableProviders,
    availableProviders,
    
    // Methods
    loginWith,
    initializeProviders,
    getProviderLoadingState,
    getProviderError,
    
    // Individual provider access
    google: googleAuth,
    facebook: facebookAuth
  }
}
