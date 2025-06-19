/**
 * Authentication middleware for protected routes (EN)
 * Middleware xác thực cho các route được bảo vệ (VI)
 */
export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()
  
  // List of public routes that don't require authentication
  const publicRoutes = [
    '/auth/login',
    '/auth/cover-login',
    '/auth/register',
    '/auth/forgot-password',
    '/auth/reset-password'
  ]
  
  // Check if current route is public
  const isPublicRoute = publicRoutes.some(route => to.path.startsWith(route))
  
  // If it's a public route, allow access
  if (isPublicRoute) {
    return
  }
  
  // For protected routes, check authentication
  if (!authStore.isAuthenticated) {
    console.log('🔒 User not authenticated, clearing auth state and redirecting to login')
    
    // Clear authentication state and cookies
    authStore.clearAuthState()
    
    // Redirect to login page
    return navigateTo('/auth/login')
  }
  
  // User is authenticated, allow access
  console.log('✅ User authenticated, allowing access to:', to.path)
})
