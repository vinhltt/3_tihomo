import { defineStore } from 'pinia'
import type { LoginRequest, LoginResponse, User, ApiResponse } from '@/types/auth'
import { useSso } from '@/composables/useSso'

type AuthState = {
  user: User | null
  token: string | null
  refreshToken: string | null
  isLoading: boolean
  error: string | null
  isAuthenticated: boolean
  authMode: 'traditional' | 'sso'
}

type LoginCredentials = {
  email: string
  password: string
}

/**
 * Authentication store for managing user login, logout, and session state (EN)
 * Store xác thực để quản lý đăng nhập, đăng xuất và trạng thái phiên của người dùng (VI)
 */
export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    token: null,
    refreshToken: null,
    isLoading: false,
    error: null,
    isAuthenticated: false,
    authMode: 'sso' // Default to SSO mode
  }),

  actions: {
    /**
     * Set authentication mode
     * Đặt chế độ xác thực
     */
    setAuthMode(mode: 'traditional' | 'sso'): void {
      this.authMode = mode
    },

    /**
     * Initialize authentication - check SSO first, fallback to traditional
     * Khởi tạo xác thực - kiểm tra SSO trước, fallback về traditional
     */
    async initAuth(): Promise<void> {
      this.isLoading = true
      this.error = null

      try {
        // Try SSO authentication first
        const sso = useSso()
        sso.initializeSso()
          if (sso.isAuthenticated.value && sso.user.value) {
          // Use SSO authentication
          this.authMode = 'sso'
          this.user = {
            id: sso.user.value.sub,
            email: sso.user.value.email,
            firstName: sso.user.value.name.split(' ')[0] || '',
            lastName: sso.user.value.name.split(' ').slice(1).join(' ') || '',
            isActive: true,
            roles: sso.user.value.roles.map(role => ({ id: role, name: role })),
            emailConfirmed: sso.user.value.email_verified || false,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString()
          }
          this.token = sso.state.accessToken
          this.refreshToken = sso.state.refreshToken
          this.isAuthenticated = true
          return
        }

        // Fallback to traditional token-based authentication
        const tokenCookie = useCookie('auth-token')
        const refreshCookie = useCookie('refresh-token')

        if (tokenCookie.value) {
          this.authMode = 'traditional'
          this.token = tokenCookie.value
          this.refreshToken = refreshCookie.value || null
          this.isAuthenticated = true
          
          // In traditional mode, we assume token is valid
          // Real implementation would validate with server
        }
      } catch (error: any) {
        console.error('Auth initialization error:', error)
        this.error = 'Failed to initialize authentication'
      } finally {
        this.isLoading = false
      }
    },

    /**
     * Login user with email and password (traditional mode only)
     * Đăng nhập người dùng bằng email và mật khẩu (chỉ chế độ traditional)
     */
    async login(credentials: LoginCredentials): Promise<boolean> {
      // Force traditional mode for email/password login
      this.authMode = 'traditional'
      this.isLoading = true
      this.error = null

      try {
        const { data } = await $fetch<ApiResponse<LoginResponse>>('/api/auth/login', {
          method: 'POST',
          body: {
            email: credentials.email,
            password: credentials.password,
          } as LoginRequest,
        })

        if (data) {
          this.token = data.token
          this.refreshToken = data.refreshToken
          this.user = data.user
          this.isAuthenticated = true

          // Store tokens in cookies for SSR
          const tokenCookie = useCookie('auth-token', {
            httpOnly: false,
            secure: true,
            sameSite: 'strict',
            maxAge: 60 * 60 * 24 * 7, // 7 days
          })
          const refreshCookie = useCookie('refresh-token', {
            httpOnly: false,
            secure: true,
            sameSite: 'strict',
            maxAge: 60 * 60 * 24 * 30, // 30 days
          })

          tokenCookie.value = data.token
          refreshCookie.value = data.refreshToken

          return true
        }

        this.error = 'Login failed'
        return false
      } catch (error: any) {
        this.error = error.data?.message || 'Login failed. Please try again.'
        return false
      } finally {
        this.isLoading = false
      }
    },

    /**
     * SSO Login - redirect to SSO server
     * Đăng nhập SSO - chuyển hướng đến SSO server
     */
    async loginWithSso(returnUrl?: string): Promise<void> {
      this.authMode = 'sso'
      this.isLoading = true
      this.error = null

      try {
        const sso = useSso()
        await sso.login(returnUrl)
      } catch (error: any) {
        this.error = error.message || 'SSO login failed'
        this.isLoading = false
      }
    },

    /**
     * Logout user
     * Đăng xuất người dùng
     */
    async logout(): Promise<void> {
      this.isLoading = true

      try {
        if (this.authMode === 'sso') {
          // SSO logout
          const sso = useSso()
          await sso.logout()
        } else {
          // Traditional logout
          try {
            await $fetch('/api/auth/logout', {
              method: 'POST',
              headers: {
                Authorization: `Bearer ${this.token}`
              }
            })
          } catch (error) {
            // Continue with local logout even if server logout fails
            console.warn('Server logout failed:', error)
          }
        }
      } catch (error: any) {
        console.error('Logout error:', error)
      } finally {
        // Clear local state regardless of server response
        this.clearAuthState()
        this.isLoading = false
      }
    },

    /**
     * Clear authentication state
     * Xóa trạng thái xác thực
     */
    clearAuthState(): void {
      this.user = null
      this.token = null
      this.refreshToken = null
      this.isAuthenticated = false
      this.error = null

      // Clear cookies
      const tokenCookie = useCookie('auth-token')
      const refreshCookie = useCookie('refresh-token')
      const ssoTokenCookie = useCookie('sso_access_token')
      const ssoRefreshCookie = useCookie('sso_refresh_token')

      tokenCookie.value = null
      refreshCookie.value = null
      ssoTokenCookie.value = null
      ssoRefreshCookie.value = null
    },

    /**
     * Refresh authentication token (EN)
     * Làm mới token xác thực (VI)
     */
    async refreshAuthToken(): Promise<boolean> {
      if (!this.refreshToken) {
        await this.logout()
        return false
      }

      try {
        const { data } = await $fetch<ApiResponse<LoginResponse>>('/api/auth/refresh-token', {
          method: 'POST',
          body: { refreshToken: this.refreshToken },
        })

        if (data) {
          this.token = data.token
          this.refreshToken = data.refreshToken
          this.user = data.user
          this.isAuthenticated = true

          // Update cookies
          const tokenCookie = useCookie('auth-token')
          const refreshCookie = useCookie('refresh-token')
          tokenCookie.value = data.token
          refreshCookie.value = data.refreshToken

          return true
        }

        await this.logout()
        return false
      } catch (error) {
        await this.logout()
        return false
      }
    },

    /**
     * Clear error state (EN)
     * Xóa trạng thái lỗi (VI)
     */
    clearError(): void {
      this.error = null
    },
  },

  getters: {
    /**
     * Get user's full name (EN)
     * Lấy tên đầy đủ của người dùng (VI)
     */
    userFullName: (state): string => {
      return state.user ? `${state.user.firstName} ${state.user.lastName}` : ''
    },

    /**
     * Check if user has specific role (EN)
     * Kiểm tra xem người dùng có vai trò cụ thể không (VI)
     */
    hasRole: (state) => (roleName: string): boolean => {
      return state.user?.roles?.some(role => role.name === roleName) ?? false
    },

    /**
     * Check if user is admin (EN)
     * Kiểm tra xem người dùng có phải là admin không (VI)
     */
    isAdmin: (state): boolean => {
      return state.user?.roles?.some(role => role.name === 'Admin') ?? false
    },
  },
})
