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
    isAuthenticated: false,
  }),

  actions: {
    /**
     * Login user with email and password (EN)
     * Đăng nhập người dùng bằng email và mật khẩu (VI)
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
     * Login with Google OAuth (EN)
     * Đăng nhập bằng Google OAuth (VI)
     */
    async loginWithGoogle(googleToken: string): Promise<boolean> {
      this.isLoading = true
      this.error = null

      try {
        const { data } = await $fetch<ApiResponse<LoginResponse>>('/api/auth/google-login', {
          method: 'POST',
          body: { token: googleToken },
        })

        if (data) {
          this.token = data.token
          this.refreshToken = data.refreshToken
          this.user = data.user
          this.isAuthenticated = true

          // Store tokens in cookies
          const tokenCookie = useCookie('auth-token')
          const refreshCookie = useCookie('refresh-token')
          tokenCookie.value = data.token
          refreshCookie.value = data.refreshToken

          return true
        }

        return false
      } catch (error: any) {
        this.error = error.data?.message || 'Google login failed'
        return false
      } finally {
        this.isLoading = false
      }
    },

    /**
     * Logout user and clear session (EN)
     * Đăng xuất người dùng và xóa phiên (VI)
     */
    async logout(): Promise<void> {
      try {
        if (this.refreshToken) {
          await $fetch('/api/auth/logout', {
            method: 'POST',
            body: { refreshToken: this.refreshToken },
          })
        }
      } catch (error) {
        // Handle logout error silently
        console.warn('Logout API call failed:', error)
      } finally {
        // Clear state regardless of API call result
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

        // Redirect to login
        await navigateTo('/auth/cover-login')
      }
    },

    /**
     * Initialize auth state from stored tokens (EN)
     * Khởi tạo trạng thái xác thực từ token đã lưu (VI)
     */
    async initAuth(): Promise<void> {
      const tokenCookie = useCookie('auth-token')
      const refreshCookie = useCookie('refresh-token')

      if (tokenCookie.value && refreshCookie.value) {
        this.token = tokenCookie.value
        this.refreshToken = refreshCookie.value

        try {
          // Verify token and get user info
          const { data } = await $fetch<ApiResponse<User>>('/api/auth/me', {
            headers: {
              Authorization: `Bearer ${this.token}`,
            },
          })

          if (data) {
            this.user = data
            this.isAuthenticated = true
          } else {
            await this.refreshAuthToken()
          }
        } catch (error) {
          // Try to refresh token
          await this.refreshAuthToken()
        }
      }
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
