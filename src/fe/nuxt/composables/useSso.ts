import type { SsoConfig, SsoState, SsoUserInfo, TokenResponse } from '@/types/sso'
import { 
  buildAuthorizationUrl, 
  buildLogoutUrl, 
  generatePkceChallenge, 
  validateAuthorizationCallback,
  exchangeCodeForTokens,
  cleanupSsoSession
} from '@/utils/sso'

/**
 * SSO composable for Single Sign-On authentication
 * Composable SSO cho xác thực Single Sign-On
 */
export const useSso = () => {
  const config = useRuntimeConfig()
  const router = useRouter()
  const route = useRoute()
  // SSO configuration
  const ssoConfig: SsoConfig = {
    baseUrl: config.public.ssoBase as string,
    clientId: config.public.ssoClientId as string,
    redirectUri: config.public.ssoRedirectUri as string,
    logoutRedirectUri: config.public.ssoLogoutRedirectUri as string,
    scope: 'openid profile email roles offline_access',
    responseType: 'code',
    grantType: 'authorization_code'
  }

  // Reactive state
  const state = reactive<SsoState>({
    isLoading: false,
    isAuthenticated: false,
    user: null,
    accessToken: null,
    refreshToken: null,
    error: null
  })

  /**
   * Clear any error state
   * Xóa trạng thái lỗi
   */
  const clearError = (): void => {
    state.error = null
  }

  /**
   * Store tokens securely in cookies
   * Lưu trữ tokens an toàn trong cookies
   */
  const storeTokens = (tokenResponse: TokenResponse): void => {
    const accessTokenCookie = useCookie('sso_access_token', {
      httpOnly: false,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      maxAge: tokenResponse.expires_in
    })

    const refreshTokenCookie = useCookie('sso_refresh_token', {
      httpOnly: false,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      maxAge: 60 * 60 * 24 * 30 // 30 days
    })

    accessTokenCookie.value = tokenResponse.access_token
    if (tokenResponse.refresh_token) {
      refreshTokenCookie.value = tokenResponse.refresh_token
    }

    state.accessToken = tokenResponse.access_token
    state.refreshToken = tokenResponse.refresh_token || null
  }

  /**
   * Clear stored tokens
   * Xóa tokens đã lưu trữ
   */
  const clearTokens = (): void => {
    const accessTokenCookie = useCookie('sso_access_token')
    const refreshTokenCookie = useCookie('sso_refresh_token')

    accessTokenCookie.value = null
    refreshTokenCookie.value = null

    state.accessToken = null
    state.refreshToken = null
    state.user = null
    state.isAuthenticated = false
  }

  /**
   * Decode JWT token to extract user info
   * Giải mã JWT token để trích xuất thông tin user
   */
  const decodeToken = (token: string): SsoUserInfo | null => {
    try {
      const payload = token.split('.')[1]
      const decoded = JSON.parse(atob(payload))
      
      return {
        sub: decoded.sub,
        name: decoded.name || decoded.given_name + ' ' + decoded.family_name,
        email: decoded.email,
        roles: decoded.role ? (Array.isArray(decoded.role) ? decoded.role : [decoded.role]) : [],
        email_verified: decoded.email_verified
      }
    } catch (error) {
      console.error('Failed to decode token:', error)
      return null
    }
  }

  /**
   * Initialize SSO authentication with PKCE
   * Khởi tạo xác thực SSO với PKCE
   */
  const login = async (returnUrl?: string): Promise<void> => {
    try {
      state.isLoading = true
      clearError()

      // Store return URL for after login
      if (returnUrl) {
        sessionStorage.setItem('sso_return_url', returnUrl)
      }

      // Generate PKCE challenge for security
      const { verifier, challenge } = await generatePkceChallenge()

      // Build authorization URL with PKCE
      const authUrl = buildAuthorizationUrl(ssoConfig, {
        codeChallenge: challenge,
        codeChallengeMethod: 'S256'
      }, verifier)

      // Redirect to SSO server
      window.location.href = authUrl

    } catch (error: any) {
      state.error = error.message || 'Failed to initiate SSO login'
      state.isLoading = false
    }
  }

  /**
   * Handle authorization callback from SSO server
   * Xử lý callback authorization từ SSO server
   */
  const handleCallback = async (): Promise<void> => {
    try {
      state.isLoading = true
      clearError()

      const searchParams = new URLSearchParams(window.location.search)
      
      // Validate callback parameters
      const validation = validateAuthorizationCallback(searchParams)
      if (!validation.isValid) {
        throw new Error(validation.error?.error_description || 'Invalid authorization callback')
      }

      const code = searchParams.get('code')!
      
      // Exchange authorization code for tokens
      const tokenResponse = await exchangeCodeForTokens(ssoConfig, code)
      
      // Store tokens
      storeTokens(tokenResponse)
      
      // Decode access token to get user info
      const userInfo = decodeToken(tokenResponse.access_token)
      if (userInfo) {
        state.user = userInfo
        state.isAuthenticated = true
      }

      // Clean up session storage
      cleanupSsoSession()

      // Redirect to return URL or dashboard
      const returnUrl = sessionStorage.getItem('sso_return_url') || '/dashboard'
      sessionStorage.removeItem('sso_return_url')
      
      await router.push(returnUrl)

    } catch (error: any) {
      state.error = error.message || 'Failed to complete SSO login'
      console.error('SSO callback error:', error)
      
      // Redirect to login page on error
      await router.push('/auth/login')
    } finally {
      state.isLoading = false
    }
  }

  /**
   * Logout from SSO
   * Đăng xuất khỏi SSO
   */
  const logout = async (): Promise<void> => {
    try {
      state.isLoading = true
      
      const idToken = state.accessToken // In practice, you'd store ID token separately
      
      // Clear local tokens
      clearTokens()
      
      // Clean up session storage
      cleanupSsoSession()
        // Build logout URL
      const logoutUrl = buildLogoutUrl(ssoConfig, idToken || undefined)
      
      // Redirect to SSO logout
      window.location.href = logoutUrl

    } catch (error: any) {
      state.error = error.message || 'Failed to logout'
      console.error('SSO logout error:', error)
    } finally {
      state.isLoading = false
    }
  }

  /**
   * Initialize SSO state from stored tokens
   * Khởi tạo trạng thái SSO từ tokens đã lưu
   */  const initializeSso = (): void => {
    const accessTokenCookie = useCookie('sso_access_token')
    const refreshTokenCookie = useCookie('sso_refresh_token')

    if (accessTokenCookie.value) {
      state.accessToken = accessTokenCookie.value
      state.refreshToken = refreshTokenCookie.value || null

      // Decode token to get user info
      const userInfo = decodeToken(accessTokenCookie.value)
      if (userInfo) {
        state.user = userInfo
        state.isAuthenticated = true
      }
    }
  }

  /**
   * Check if user has specific role
   * Kiểm tra user có role cụ thể không
   */
  const hasRole = (roleName: string): boolean => {
    return state.user?.roles?.includes(roleName) || false
  }

  /**
   * Check if user is admin
   * Kiểm tra user có phải admin không
   */
  const isAdmin = (): boolean => {
    return hasRole('Admin')
  }

  // Initialize on composable creation
  if (process.client) {
    initializeSso()
  }

  return {
    // State
    state: readonly(state),
    isAuthenticated: computed(() => state.isAuthenticated),
    isLoading: computed(() => state.isLoading),
    user: computed(() => state.user),
    error: computed(() => state.error),
    
    // Actions
    login,
    logout,
    handleCallback,
    clearError,
    hasRole,
    isAdmin,
    initializeSso
  }
}
