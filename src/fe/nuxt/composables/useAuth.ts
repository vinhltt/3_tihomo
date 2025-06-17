import { useAuthStore } from '@/stores/auth'
import type { LoginCredentials } from '@/types/auth'

/**
 * Authentication composable for managing login, logout, and auth state (EN)
 * Composable xác thực để quản lý đăng nhập, đăng xuất và trạng thái xác thực (VI)
 */
export const useAuth = () => {
  const authStore = useAuthStore()
  const router = useRouter()

  /**
   * Login with email and password (EN)
   * Đăng nhập bằng email và mật khẩu (VI)
   */
  const login = async (credentials: { email: string; password: string }): Promise<boolean> => {
    const success = await authStore.login(credentials)

    if (success) {
      // Redirect to dashboard after successful login
      await router.push('/dashboard')
    }

    return success
  }

  /**
   * Login with Google OAuth (EN)
   * Đăng nhập bằng Google OAuth (VI)
   */
  const loginWithGoogle = async (googleToken: string): Promise<boolean> => {
    const success = await authStore.loginWithGoogle(googleToken)

    if (success) {
      await router.push('/dashboard')
    }

    return success
  }

  /**
   * Logout current user (EN)
   * Đăng xuất người dùng hiện tại (VI)
   */
  const logout = async (): Promise<void> => {
    await authStore.logout()
  }

  /**
   * Check if current user has specific role (EN)
   * Kiểm tra xem người dùng hiện tại có vai trò cụ thể không (VI)
   */
  const hasRole = (roleName: string): boolean => {
    return authStore.hasRole(roleName)
  }

  /**
   * Check if current user is admin (EN)
   * Kiểm tra xem người dùng hiện tại có phải là admin không (VI)
   */
  const isAdmin = (): boolean => {
    return authStore.isAdmin
  }

  /**
   * Clear authentication error (EN)
   * Xóa lỗi xác thực (VI)
   */
  const clearError = (): void => {
    authStore.clearError()
  }

  /**
   * Initialize authentication state (EN)
   * Khởi tạo trạng thái xác thực (VI)
   */
  const initAuth = async (): Promise<void> => {
    await authStore.initAuth()
  }

  /**
   * Refresh authentication token (EN)
   * Làm mới token xác thực (VI)
   */
  const refreshToken = async (): Promise<boolean> => {
    return await authStore.refreshAuthToken()
  }

  return {
    // State
    user: computed(() => authStore.user),
    isAuthenticated: computed(() => authStore.isAuthenticated),
    isLoading: computed(() => authStore.isLoading),
    error: computed(() => authStore.error),
    userFullName: computed(() => authStore.userFullName),

    // Actions
    login,
    loginWithGoogle,
    logout,
    hasRole,
    isAdmin,
    clearError,
    initAuth,
    refreshToken,
  }
}
