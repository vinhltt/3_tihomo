import { defineStore } from 'pinia'
import type { LoginRequest, LoginResponse, User, ApiResponse } from '@/types/auth'

type AuthState = {
  user: User | null
  token: string | null
  refreshToken: string | null
  isLoading: boolean
  error: string | null
  isAuthenticated: boolean
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
    isAuthenticated: false
  }),

  actions: {
    /**
     * Initialize authentication - check for existing tokens
     * Khởi tạo xác thực - kiểm tra token hiện có
     */
    async initAuth(): Promise<void> {
      this.isLoading = true
      this.error = null

      try {
        // Check for existing tokens in cookies
        const tokenCookie = useCookie('auth-token')
        const refreshCookie = useCookie('refresh-token')

        if (tokenCookie.value) {
          this.token = tokenCookie.value
          this.refreshToken = refreshCookie.value || null
          this.isAuthenticated = true
          
          // In a real implementation, validate token with server
          // For now, assume token is valid if it exists
        }
      } catch (error: any) {
        console.error('Auth initialization error:', error)
        this.error = 'Failed to initialize authentication'
      } finally {
        this.isLoading = false
      }
    },    /**
     * Login user with email and password
     * Đăng nhập người dùng bằng email và mật khẩu
     */
    async login(credentials: LoginCredentials): Promise<boolean> {
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
     * Logout user
     * Đăng xuất người dùng
     */
    async logout(): Promise<void> {
      this.isLoading = true

      try {
        // Traditional logout - call API to invalidate token
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
      } catch (error: any) {
        console.error('Logout error:', error)
      } finally {
        // Clear local state regardless of server response
        this.clearAuthState()
        this.isLoading = false
      }
    },    /**
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

      tokenCookie.value = null
      refreshCookie.value = null
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
    },    /**
     * Login user with social provider response (EN)
     * Đăng nhập người dùng với phản hồi từ nhà cung cấp xã hội (VI)
     */
    async socialLogin(socialResponse: import('@/types/auth').SocialLoginResponse): Promise<boolean> {
      this.isLoading = true
      this.error = null

      try {
        // Store social login data
        this.token = socialResponse.accessToken
        this.refreshToken = socialResponse.refreshToken
        
        // Convert UserInfo to User format for auth store
        const nameParts = socialResponse.user.name.split(' ')
        this.user = {
          id: socialResponse.user.id,
          email: socialResponse.user.email,
          firstName: nameParts[0] || '',
          lastName: nameParts.slice(1).join(' ') || '',
          isActive: socialResponse.user.isActive,
          emailConfirmed: true, // Assume email is confirmed from social providers
          roles: [], // Will be populated from backend if needed
          createdAt: socialResponse.user.createdAt,
          updatedAt: socialResponse.user.createdAt, // Use createdAt as fallback
          pictureUrl: socialResponse.user.pictureUrl
        }
        
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

        tokenCookie.value = socialResponse.accessToken
        refreshCookie.value = socialResponse.refreshToken

        console.log('✅ Social login data stored in auth store:', {
          hasUser: !!this.user,
          hasToken: !!this.token,
          userEmail: this.user?.email,
          isAuthenticated: this.isAuthenticated
        })

        return true
      } catch (error: any) {
        this.error = error.message || 'Social login failed'
        console.error('❌ Social login storage error:', error)
        return false
      } finally {
        this.isLoading = false
      }
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
