/**
 * Global authentication middleware (EN)
 * Middleware xác thực toàn cục (VI)
 * 
 * This middleware runs on every route change and automatically:
 * - Initializes authentication state
 * - Checks if user is authenticated for protected routes
 * - Clears auth state and redirects to login if not authenticated
 * - Allows access to public routes without authentication
 */
export default defineNuxtRouteMiddleware(async (to) => {
  // Skip middleware on server-side rendering initial load
  if (process.server) {
    return
  }
  
  const authStore = useAuthStore()
    // Check if route has auth disabled in page meta
  if (to.meta.auth === false) {
    // If user is already authenticated and trying to access login pages, redirect to home
    if (authStore.isAuthenticated && (to.path.startsWith('/auth/login') || to.path.startsWith('/auth/cover-login'))) {
      console.log('🏠 User already authenticated, redirecting to home')
      return navigateTo('/')
    }
    return
  }
  
  // Initialize auth state if not already done
  if (!authStore.isAuthenticated && !authStore.isLoading) {
    try {
      await authStore.initAuth()
    } catch (error) {
      console.warn('Failed to initialize auth:', error)
    }
  }
  
  // List of public routes that don't require authentication (fallback)
  const publicRoutes = [
    '/auth/login',
    '/auth/cover-login', 
    '/auth/register',
    '/auth/forgot-password',
    '/auth/reset-password',
    '/auth/verify-email'
  ]
  
  // Check if current route is public
  const isPublicRoute = publicRoutes.some(route => to.path.startsWith(route))
    // If it's a public route, allow access
  if (isPublicRoute) {
    // If user is already authenticated and trying to access login pages, redirect to home
    if (authStore.isAuthenticated && (to.path.startsWith('/auth/login') || to.path.startsWith('/auth/cover-login'))) {
      console.log('🏠 User already authenticated, redirecting to home')
      return navigateTo('/')
    }
    return
  }
  
  // For protected routes, check authentication
  if (!authStore.isAuthenticated) {
    console.log('🔒 User not authenticated for protected route:', to.path)
    console.log('🧹 Clearing auth state and redirecting to login')
    
    // Clear authentication state and cookies
    authStore.clearAuthState()
      // Redirect to cover-login page with return URL
    const returnUrl = to.fullPath !== '/' ? `?returnUrl=${encodeURIComponent(to.fullPath)}` : ''
    return navigateTo(`/auth/cover-login${returnUrl}`)
  }
  
  // User is authenticated, allow access
  console.log('✅ User authenticated, allowing access to:', to.path)
})
