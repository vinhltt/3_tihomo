/**
 * Authentication related types for Identity & Access Management (EN)
 * Các kiểu dữ liệu liên quan đến xác thực cho quản lý danh tính và truy cập (VI)
 */

/**
 * Login request data (EN)
 * Dữ liệu yêu cầu đăng nhập (VI)
 */
export type LoginRequest = {
  email: string
  password: string
}

/**
 * Google login request data (EN)
 * Dữ liệu yêu cầu đăng nhập Google (VI)
 */
export type GoogleLoginRequest = {
  token: string
}

/**
 * Refresh token request data (EN)
 * Dữ liệu yêu cầu làm mới token (VI)
 */
export type RefreshTokenRequest = {
  refreshToken: string
}

/**
 * Logout request data (EN)
 * Dữ liệu yêu cầu đăng xuất (VI)
 */
export type LogoutRequest = {
  refreshToken: string
}

/**
 * User role information (EN)
 * Thông tin vai trò người dùng (VI)
 */
export type UserRole = {
  id: string
  name: string
  description?: string
}

/**
 * User profile information (EN)
 * Thông tin hồ sơ người dùng (VI)
 */
export type User = {
  id: string
  email: string
  firstName: string
  lastName: string
  isActive: boolean
  emailConfirmed: boolean
  roles: UserRole[]
  createdAt: string
  updatedAt: string
  pictureUrl?: string
}

/**
 * Login response data (EN)
 * Dữ liệu phản hồi đăng nhập (VI)
 */
export type LoginResponse = {
  token: string
  refreshToken: string
  user: User
  expiresAt: string
}

/**
 * Generic API response wrapper (EN)
 * Wrapper phản hồi API chung (VI)
 */
export type ApiResponse<T = any> = {
  success: boolean
  message: string
  data?: T
  errors?: string[]
}

/**
 * Authentication error types (EN)
 * Các kiểu lỗi xác thực (VI)
 */
export type AuthError = {
  code: string
  message: string
  details?: Record<string, any>
}

/**
 * Password reset request (EN)
 * Yêu cầu đặt lại mật khẩu (VI)
 */
export type PasswordResetRequest = {
  email: string
}

/**
 * Password reset confirmation (EN)
 * Xác nhận đặt lại mật khẩu (VI)
 */
export type PasswordResetConfirmRequest = {
  token: string
  email: string
  newPassword: string
  confirmPassword: string
}

/**
 * User registration request (EN)
 * Yêu cầu đăng ký người dùng (VI)
 */
export type RegisterRequest = {
  email: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
}

/**
 * Email confirmation request (EN)
 * Yêu cầu xác nhận email (VI)
 */
export type EmailConfirmationRequest = {
  email: string
  token: string
}

/**
 * Change password request (EN)
 * Yêu cầu thay đổi mật khẩu (VI)
 */
export type ChangePasswordRequest = {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

/**
 * Update profile request (EN)
 * Yêu cầu cập nhật hồ sơ (VI)
 */
export type UpdateProfileRequest = {
  firstName: string
  lastName: string
}

/**
 * Social login providers (EN)
 * Các nhà cung cấp đăng nhập xã hội (VI)
 */
export type SocialProvider = 'Google' | 'Facebook'

/**
 * Social login request data (EN)
 * Dữ liệu yêu cầu đăng nhập xã hội (VI)
 */
export type SocialLoginRequest = {
  Provider: SocialProvider
  Token: string
}

/**
 * Social login response from provider (EN)
 * Phản hồi đăng nhập xã hội từ nhà cung cấp (VI)
 */
export type SocialLoginResponse = {
  user: UserInfo
  accessToken: string
  refreshToken: string
  expiresAt: string
}

/**
 * User information from social login (EN)
 * Thông tin người dùng từ đăng nhập xã hội (VI)
 */
export type UserInfo = {
  id: string
  email: string
  name?: string
  pictureUrl?: string
  isActive: boolean
  createdAt: string
  providers: string[]
}
